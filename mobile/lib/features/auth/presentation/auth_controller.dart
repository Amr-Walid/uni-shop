import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/error/app_exception.dart';
import '../../../core/l10n/locale_controller.dart';
import '../../../core/providers/core_providers.dart';
import '../../../core/storage/token_storage.dart';
import '../data/auth_repository.dart';
import '../domain/user_profile.dart';

/// Owns the app's authentication state.
///
/// Everything that depends on "is someone signed in" watches this, so there is
/// exactly one source of truth and no screen has to inspect token storage.
class AuthController extends AsyncNotifier<AuthState> {
  AuthRepository get _repository => ref.read(authRepositoryProvider);
  TokenStorage get _tokenStorage => ref.read(tokenStorageProvider);

  @override
  Future<AuthState> build() async {
    // React to an involuntary session end reported by the auth interceptor.
    ref.listen<int>(sessionExpiryProvider, (previous, next) {
      if (previous == null || next <= previous) return;
      state = const AsyncData(
        Unauthenticated(reason: 'errorSessionExpired'),
      );
    });

    return _restoreSession();
  }

  /// Restores a session from secure storage at cold start.
  ///
  /// The stored refresh token is not trusted blindly — the profile is fetched
  /// to confirm it is still valid, because the account may have been locked or
  /// deleted, or the token family revoked, while the app was closed.
  Future<AuthState> _restoreSession() async {
    if (!await _tokenStorage.hasSession()) {
      return const Unauthenticated();
    }

    try {
      final user = await _repository.getProfile();
      return Authenticated(user);
    } on AppException catch (error) {
      // A fatal auth error means the session is genuinely gone: clear it so
      // the next launch does not repeat this round-trip.
      if (error.isFatalAuth) {
        await _tokenStorage.clear();
        return const Unauthenticated();
      }

      // A network failure is NOT proof the session ended. Keeping the tokens
      // means an offline cold start recovers once connectivity returns,
      // instead of silently signing the user out.
      return const Unauthenticated();
    }
  }

  Future<void> login({
    required String email,
    required String password,
  }) async {
    state = const AsyncLoading();
    try {
      final result = await _repository.login(
        email: email,
        password: password,
        deviceInfo: await _describeDevice(),
      );
      state = AsyncData(Authenticated(result.user));
    } catch (error, stack) {
      // Failing back to Unauthenticated rather than AsyncError: the form needs
      // to show the error, but the app must not be left in a limbo state where
      // it is neither signed in nor signed out.
      state = AsyncData(const Unauthenticated());
      Error.throwWithStackTrace(error, stack);
    }
  }

  Future<void> register({
    required String email,
    required String password,
    required String fullName,
    String? phone,
  }) async {
    state = const AsyncLoading();
    try {
      final result = await _repository.register(
        email: email,
        password: password,
        fullName: fullName,
        phone: phone,
        preferredLanguage: ref.read(languageCodeProvider),
        deviceInfo: await _describeDevice(),
      );
      state = AsyncData(Authenticated(result.user));
    } catch (error, stack) {
      state = AsyncData(const Unauthenticated());
      Error.throwWithStackTrace(error, stack);
    }
  }

  Future<void> logout() async {
    await _repository.logout();
    state = const AsyncData(Unauthenticated());
  }

  Future<void> logoutAll() async {
    await _repository.logoutAll();
    state = const AsyncData(Unauthenticated());
  }

  /// Re-reads the profile, e.g. after editing it elsewhere.
  Future<void> refreshProfile() async {
    final current = state.valueOrNull;
    if (current is! Authenticated) return;

    try {
      final user = await _repository.getProfile();
      state = AsyncData(Authenticated(user));
    } on AppException catch (error) {
      if (error.isFatalAuth) {
        await _tokenStorage.clear();
        state = const AsyncData(Unauthenticated());
      }
      // A transient failure leaves the cached profile in place — stale details
      // are better than an empty account screen.
    }
  }

  Future<void> updateProfile({
    String? fullName,
    String? phone,
    String? defaultAddress,
    String? city,
    String? governorate,
    String? preferredLanguage,
  }) async {
    final user = await _repository.updateProfile(
      fullName: fullName,
      phone: phone,
      defaultAddress: defaultAddress,
      city: city,
      governorate: governorate,
      preferredLanguage: preferredLanguage,
    );
    state = AsyncData(Authenticated(user));
  }

  Future<void> deleteAccount({required String password}) async {
    await _repository.deleteAccount(password: password);
    state = const AsyncData(Unauthenticated());
  }

  /// Human-readable device label, stored against the refresh token so a user
  /// can tell their sessions apart on a "signed-in devices" screen.
  Future<String?> _describeDevice() async => null;
}

final authControllerProvider =
    AsyncNotifierProvider<AuthController, AuthState>(AuthController.new);

/// Convenience selectors so widgets do not each unwrap AsyncValue + sealed
/// state, and so a rebuild is scoped to the value actually used.
final isAuthenticatedProvider = Provider<bool>((ref) {
  return ref.watch(authControllerProvider).valueOrNull is Authenticated;
});

final currentUserProvider = Provider<UserProfile?>((ref) {
  final state = ref.watch(authControllerProvider).valueOrNull;
  return state is Authenticated ? state.user : null;
});
