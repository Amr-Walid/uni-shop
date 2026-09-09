import '../../../core/network/api_client.dart';
import '../../../core/storage/token_storage.dart';
import '../domain/user_profile.dart';

/// Authentication and account management.
///
/// Token persistence lives here rather than in the controller so there is one
/// place where a session begins and ends — a screen that forgets to save the
/// rotated refresh token would silently break the session on next launch.
class AuthRepository {
  const AuthRepository({
    required ApiClient client,
    required TokenStorage tokenStorage,
  })  : _client = client,
        _tokenStorage = tokenStorage;

  final ApiClient _client;
  final TokenStorage _tokenStorage;

  Future<AuthResult> register({
    required String email,
    required String password,
    required String fullName,
    String? phone,
    String? preferredLanguage,
    String? deviceInfo,
  }) async {
    final json = await _client.post<Map<String, dynamic>>(
      'auth/register',
      body: {
        'email': email.trim(),
        'password': password,
        'fullName': fullName.trim(),
        'phone': phone?.trim(),
        'preferredLanguage': preferredLanguage,
        'deviceInfo': deviceInfo,
      },
    );

    final result = AuthResult.fromJson(json);
    await _persist(result);
    return result;
  }

  Future<AuthResult> login({
    required String email,
    required String password,
    String? deviceInfo,
  }) async {
    final json = await _client.post<Map<String, dynamic>>(
      'auth/login',
      body: {
        'email': email.trim(),
        'password': password,
        'deviceInfo': deviceInfo,
      },
    );

    final result = AuthResult.fromJson(json);
    await _persist(result);
    return result;
  }

  /// Ends the session on this device.
  ///
  /// The stored tokens are cleared even if the network call fails: the user
  /// asked to sign out, and leaving a usable token on the device because the
  /// server was unreachable is the wrong trade-off. The refresh token stays
  /// valid server-side until it expires, which is why the request is still
  /// attempted first.
  Future<void> logout() async {
    final refreshToken = await _tokenStorage.getRefreshToken();

    try {
      if (refreshToken != null) {
        await _client.post<Map<String, dynamic>>(
          'auth/logout',
          body: {'refreshToken': refreshToken},
        );
      }
    } catch (_) {
      // Deliberately swallowed — see above.
    } finally {
      await _tokenStorage.clear();
    }
  }

  /// Revokes every session for the account (all devices).
  Future<void> logoutAll() async {
    try {
      await _client.post<Map<String, dynamic>>('auth/logout-all');
    } finally {
      await _tokenStorage.clear();
    }
  }

  Future<UserProfile> getProfile() async {
    final json = await _client.get<Map<String, dynamic>>('me');
    return UserProfile.fromJson(json);
  }

  Future<UserProfile> updateProfile({
    String? fullName,
    String? phone,
    String? defaultAddress,
    String? city,
    String? governorate,
    String? preferredLanguage,
  }) async {
    final json = await _client.put<Map<String, dynamic>>(
      'me',
      body: {
        'fullName': fullName?.trim(),
        'phone': phone?.trim(),
        'defaultAddress': defaultAddress?.trim(),
        'city': city?.trim(),
        'governorate': governorate?.trim(),
        'preferredLanguage': preferredLanguage,
      },
    );
    return UserProfile.fromJson(json);
  }

  /// Saves just the shipping address, so checkout can offer "save this
  /// address" without submitting the whole profile.
  Future<UserProfile> updateAddress({
    required String address,
    String? city,
    String? governorate,
  }) async {
    final json = await _client.put<Map<String, dynamic>>(
      'me/address',
      body: {
        'defaultAddress': address.trim(),
        'city': city?.trim(),
        'governorate': governorate?.trim(),
      },
    );
    return UserProfile.fromJson(json);
  }

  Future<void> changePassword({
    required String currentPassword,
    required String newPassword,
  }) async {
    await _client.post<Map<String, dynamic>>(
      'auth/change-password',
      body: {
        'currentPassword': currentPassword,
        'newPassword': newPassword,
      },
    );
  }

  /// Requests a reset email.
  ///
  /// Always reports success. The server does the same regardless of whether
  /// the address exists, because a differing response would turn this into an
  /// account-enumeration oracle.
  Future<void> forgotPassword(String email) async {
    await _client.post<Map<String, dynamic>>(
      'auth/forgot-password',
      body: {'email': email.trim()},
    );
  }

  Future<void> resetPassword({
    required String email,
    required String token,
    required String newPassword,
  }) async {
    await _client.post<Map<String, dynamic>>(
      'auth/reset-password',
      body: {
        'email': email.trim(),
        'token': token,
        'newPassword': newPassword,
      },
    );
  }

  /// Deletes the account.
  ///
  /// Server-side this anonymises the user and nulls `SalesOrder.UserId` rather
  /// than cascading, so the store's financial history survives — the customer's
  /// personal data goes, the accounting does not.
  Future<void> deleteAccount({required String password}) async {
    try {
      await _client.delete<Map<String, dynamic>>(
        'auth/account',
        body: {'password': password},
      );
    } finally {
      await _tokenStorage.clear();
    }
  }

  Future<void> _persist(AuthResult result) async {
    await _tokenStorage.saveTokens(
      accessToken: result.accessToken,
      refreshToken: result.refreshToken,
      // Stored so a cold start knows whose session is being restored before
      // the profile request completes.
      userId: result.user.id,
    );
  }
}
