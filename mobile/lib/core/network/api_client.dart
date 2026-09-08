import 'dart:async';

import 'package:dio/dio.dart';
import 'package:flutter/foundation.dart';

import '../config/env.dart';
import '../error/api_error_codes.dart';
import '../error/app_exception.dart';
import '../storage/token_storage.dart';
import 'auth_interceptor.dart';
import 'error_interceptor.dart';

/// Thin, typed wrapper over Dio.
///
/// Every repository goes through this, so cross-cutting behaviour (auth,
/// language, error normalisation) is configured exactly once and cannot be
/// forgotten at a call site.
class ApiClient {
  ApiClient({
    required TokenStorage tokenStorage,
    Dio? dio,
    Dio? refreshDio,
    void Function()? onSessionExpired,
  })  : _tokenStorage = tokenStorage,
        _dio = dio ?? Dio(),
        _refreshDio = refreshDio ?? Dio() {
    _configure(_dio);
    // The refresh client shares the base options but carries NO
    // AuthInterceptor, otherwise a failing refresh would recurse through the
    // same handler.
    _configure(_refreshDio);

    _dio.interceptors.addAll([
      _LanguageInterceptor(resolveLanguage: () => _language),
      AuthInterceptor(
        tokenStorage: _tokenStorage,
        refreshClient: _refreshDio,
        onSessionExpired: onSessionExpired,
      ),
      ErrorInterceptor(),
      if (Env.enableNetworkLogging) _LogInterceptor(),
    ]);

    _refreshDio.interceptors.add(ErrorInterceptor());
  }

  final Dio _dio;
  final Dio _refreshDio;
  final TokenStorage _tokenStorage;

  /// Language sent as `Accept-Language`. The API resolves the paired Ar/En
  /// columns from it and returns a single localized field per value, which is
  /// what keeps payloads small and widgets free of language branching.
  String _language = 'ar';

  String get language => _language;

  set language(String value) {
    _language = (value == 'en') ? 'en' : 'ar';
  }

  void _configure(Dio dio) {
    dio.options = BaseOptions(
      connectTimeout: Env.connectTimeout,
      receiveTimeout: Env.receiveTimeout,
      validateStatus: (status) =>
          status != null && status >= 200 && status < 300,
      headers: {'Content-Type': 'application/json'},
      responseType: ResponseType.json,
    );
  }

  // ── Verbs ─────────────────────────────────────────────────────────────────

  Future<T> get<T>(
    String path, {
    Map<String, dynamic>? query,
    CancelToken? cancelToken,
  }) =>
      _send<T>(() => _dio.get<T>(
            _url(path),
            queryParameters: _clean(query),
            cancelToken: cancelToken,
          ));

  Future<T> post<T>(
    String path, {
    Object? body,
    Map<String, dynamic>? query,
    Map<String, String>? headers,
    CancelToken? cancelToken,
  }) =>
      _send<T>(() => _dio.post<T>(
            _url(path),
            data: body,
            queryParameters: _clean(query),
            options: headers == null ? null : Options(headers: headers),
            cancelToken: cancelToken,
          ));

  Future<T> put<T>(
    String path, {
    Object? body,
    CancelToken? cancelToken,
  }) =>
      _send<T>(() => _dio.put<T>(
            _url(path),
            data: body,
            cancelToken: cancelToken,
          ));

  Future<T> delete<T>(
    String path, {
    Object? body,
    CancelToken? cancelToken,
  }) =>
      _send<T>(() => _dio.delete<T>(
            _url(path),
            data: body,
            cancelToken: cancelToken,
          ));

  /// Unwraps the response and guarantees an [AppException] on failure.
  Future<T> _send<T>(Future<Response<T>> Function() request) async {
    try {
      final response = await request();
      final data = response.data;

      if (data == null) {
        // A 204, or an empty body on an endpoint the caller expected data from.
        throw const AppException(
          code: ApiErrorCodes.unknown,
          message: 'استجابة فارغة من الخادم.',
        );
      }
      return data;
    } on DioException catch (e) {
      // ErrorInterceptor attaches the mapped exception; fall back defensively
      // in case an error bypassed it.
      final mapped = e.error;
      if (mapped is AppException) throw mapped;

      throw AppException(
        code: ApiErrorCodes.unknown,
        message: e.message ?? 'حدث خطأ غير متوقع.',
      );
    }
  }

  /// Joins [path] onto [Env.apiRoot] with exactly one separating slash.
  ///
  /// Naive interpolation produced `/api/v1products`: apiRoot carries no
  /// trailing slash and repository paths carry no leading one, so every
  /// request 404'd. Normalising both sides means a caller can pass
  /// `'products'` or `'/products'` and neither can yield a doubled or
  /// missing slash.
  String _url(String path) {
    if (path.startsWith('http')) return path;

    final root = Env.apiRoot.endsWith('/')
        ? Env.apiRoot.substring(0, Env.apiRoot.length - 1)
        : Env.apiRoot;
    final suffix = path.startsWith('/') ? path : '/$path';

    return '$root$suffix';
  }

  /// Strips null query values so the URL never carries `?brand=null`, which the
  /// server would bind as the literal string "null".
  Map<String, dynamic>? _clean(Map<String, dynamic>? query) {
    if (query == null) return null;

    final cleaned = <String, dynamic>{};
    query.forEach((key, value) {
      if (value != null) cleaned[key] = value;
    });
    return cleaned.isEmpty ? null : cleaned;
  }
}

/// Adds `Accept-Language` to every request.
class _LanguageInterceptor extends Interceptor {
  _LanguageInterceptor({required this.resolveLanguage});

  final String Function() resolveLanguage;

  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    options.headers['Accept-Language'] = resolveLanguage();
    handler.next(options);
  }
}

/// Debug-only request logging.
///
/// Logs the method, path and status but NEVER headers or bodies: those carry
/// bearer tokens, passwords and customer addresses, and device logs are
/// readable by other tooling on the handset.
class _LogInterceptor extends Interceptor {
  @override
  void onRequest(RequestOptions options, RequestInterceptorHandler handler) {
    debugPrint('→ ${options.method} ${options.path}');
    handler.next(options);
  }

  @override
  void onResponse(
    Response<dynamic> response,
    ResponseInterceptorHandler handler,
  ) {
    debugPrint('← ${response.statusCode} ${response.requestOptions.path}');
    handler.next(response);
  }

  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    debugPrint(
        '✗ ${err.response?.statusCode ?? '-'} ${err.requestOptions.path}');
    handler.next(err);
  }
}
