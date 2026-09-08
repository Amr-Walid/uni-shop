import 'dart:async';

import 'package:dio/dio.dart';

import '../config/env.dart';
import '../error/api_error_codes.dart';
import '../error/app_exception.dart';
import '../storage/token_storage.dart';

/// Attaches the bearer token and transparently refreshes it on expiry.
///
/// ─────────────────────────────────────────────────────────────────────────
/// WHY THE MUTEX IS NOT OPTIONAL
/// ─────────────────────────────────────────────────────────────────────────
/// The home screen fires several requests at once. When the access token has
/// just expired they ALL come back 401 at roughly the same moment.
///
/// Without serialisation each one would call `/auth/refresh` with the same
/// refresh token. The server ROTATES refresh tokens and treats a replayed
/// (already-revoked) token as theft, so the second call onwards would be seen
/// as a reuse attack and the server would revoke the entire token family —
/// signing the user out precisely because the app tried to keep them signed in.
///
/// So: the first 401 performs the refresh while the others await the same
/// future, then every request is replayed with the new token.
class AuthInterceptor extends QueuedInterceptor {
  AuthInterceptor({
    required TokenStorage tokenStorage,
    required Dio refreshClient,
    this.onSessionExpired,
  })  : _tokenStorage = tokenStorage,
        _refreshClient = refreshClient;

  final TokenStorage _tokenStorage;

  /// A SEPARATE Dio instance for the refresh call.
  ///
  /// Using the main client would send the request back through this
  /// interceptor, so a failing refresh would recurse until the stack blew up.
  final Dio _refreshClient;

  /// Invoked when the session cannot be recovered, so the app can route to
  /// sign-in and clear user-scoped state.
  final void Function()? onSessionExpired;

  /// The in-flight refresh, shared by all waiters. Null when none is running.
  Future<bool>? _refreshFuture;

  /// Endpoints that must never carry (or wait for) a token. Sending an expired
  /// bearer to `/auth/refresh` or `/auth/login` would be pointless, and
  /// treating their 401 as "refresh me" would loop.
  static const _publicAuthPaths = {
    '/auth/login',
    '/auth/admin/login',
    '/auth/register',
    '/auth/refresh',
    '/auth/forgot-password',
    '/auth/reset-password',
  };

  bool _isPublicAuthPath(String path) =>
      _publicAuthPaths.any((p) => path.contains(p));

  @override
  Future<void> onRequest(
    RequestOptions options,
    RequestInterceptorHandler handler,
  ) async {
    if (!_isPublicAuthPath(options.path)) {
      // A secure-storage failure must NOT fail the request. The keystore can
      // be unavailable (a plain Dart VM with no plugins, a device whose
      // keystore is corrupt or still locked), and the entire public catalogue
      // is browsable without a token — so degrade to an anonymous request
      // instead of throwing.
      String? token;
      try {
        token = await _tokenStorage.getAccessToken();
      } catch (_) {
        token = null;
      }

      if (token != null && token.isNotEmpty) {
        options.headers['Authorization'] = 'Bearer $token';
      }
    }
    handler.next(options);
  }

  @override
  Future<void> onError(
    DioException err,
    ErrorInterceptorHandler handler,
  ) async {
    final status = err.response?.statusCode;
    final path = err.requestOptions.path;

    // Only a 401 on a protected endpoint is a refresh candidate.
    if (status != 401 || _isPublicAuthPath(path)) {
      return handler.next(err);
    }

    final errorCode = _extractErrorCode(err);

    // A structurally invalid or revoked credential cannot be refreshed —
    // refreshing would just fail again. End the session immediately.
    if (ApiErrorCodes.fatalAuthCodes.contains(errorCode)) {
      await _endSession();
      return handler.next(err);
    }

    // No refresh token means this was an anonymous request hitting a protected
    // route; there is nothing to refresh. A storage failure is treated the
    // same way — without a readable token the 401 is simply propagated.
    String? refreshToken;
    try {
      refreshToken = await _tokenStorage.getRefreshToken();
    } catch (_) {
      refreshToken = null;
    }

    if (refreshToken == null || refreshToken.isEmpty) {
      return handler.next(err);
    }

    final refreshed = await _refreshOnce();
    if (!refreshed) {
      await _endSession();
      return handler.next(err);
    }

    // Replay the original request with the new token.
    try {
      final response = await _retry(err.requestOptions);
      return handler.resolve(response);
    } on DioException catch (retryError) {
      return handler.next(retryError);
    }
  }

  /// Runs the refresh, or joins the one already in progress.
  Future<bool> _refreshOnce() {
    // Joining the existing future is what prevents concurrent refreshes from
    // tripping the server's reuse detection.
    final inFlight = _refreshFuture;
    if (inFlight != null) return inFlight;

    final future = _performRefresh();
    _refreshFuture = future;

    // Clear the slot once settled so the NEXT expiry can refresh again.
    future.whenComplete(() => _refreshFuture = null);

    return future;
  }

  Future<bool> _performRefresh() async {
    try {
      final refreshToken = await _tokenStorage.getRefreshToken();
      if (refreshToken == null || refreshToken.isEmpty) return false;

      final response = await _refreshClient.post<Map<String, dynamic>>(
        '${Env.apiRoot}/auth/refresh',
        data: {'refreshToken': refreshToken},
      );

      final data = response.data;
      if (data == null) return false;

      final newAccess = data['accessToken'] as String?;
      final newRefresh = data['refreshToken'] as String?;
      if (newAccess == null || newRefresh == null) return false;

      final user = data['user'];
      final userId = user is Map<String, dynamic> ? user['id'] as String? : null;

      await _tokenStorage.saveTokens(
        accessToken: newAccess,
        refreshToken: newRefresh,
        userId: userId,
      );
      return true;
    } on DioException {
      // Includes REFRESH_TOKEN_REUSED, which the server answers after revoking
      // the family — the only correct response is to sign out.
      return false;
    } catch (_) {
      return false;
    }
  }

  /// Re-issues a request, now that a fresh token is stored.
  Future<Response<dynamic>> _retry(RequestOptions options) {
    final token = _tokenStorage.getAccessToken();

    return token.then((value) {
      final headers = Map<String, dynamic>.from(options.headers);
      if (value != null && value.isNotEmpty) {
        headers['Authorization'] = 'Bearer $value';
      }

      return _refreshClient.fetch(
        options.copyWith(headers: headers),
      );
    });
  }

  Future<void> _endSession() async {
    await _tokenStorage.clear();
    onSessionExpired?.call();
  }

  String? _extractErrorCode(DioException err) {
    // ErrorInterceptor may already have mapped this.
    final mapped = err.error;
    if (mapped is AppException) return mapped.code;

    final data = err.response?.data;
    if (data is Map<String, dynamic>) return data['errorCode'] as String?;
    return null;
  }
}
