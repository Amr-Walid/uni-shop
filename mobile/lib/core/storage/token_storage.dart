import 'package:flutter_secure_storage/flutter_secure_storage.dart';

/// Persists the access + refresh token pair.
///
/// Uses [FlutterSecureStorage] (iOS Keychain / Android EncryptedSharedPreferences)
/// rather than SharedPreferences: a refresh token is a long-lived credential and
/// would otherwise be readable in plain text on a rooted or jailbroken device.
///
/// The user's password is NEVER stored — only these tokens, which the server can
/// revoke.
class TokenStorage {
  TokenStorage({FlutterSecureStorage? storage})
      : _storage = storage ??
            const FlutterSecureStorage(
              aOptions: AndroidOptions(encryptedSharedPreferences: true),
              iOptions: IOSOptions(
                // Tokens are needed on the first unlocked launch (e.g. to
                // refresh in the background), but must not sync to iCloud and
                // must not be restored onto a different device from a backup.
                accessibility: KeychainAccessibility.first_unlock_this_device,
              ),
            );

  final FlutterSecureStorage _storage;

  static const _accessTokenKey = 'access_token';
  static const _refreshTokenKey = 'refresh_token';
  static const _userIdKey = 'user_id';

  /// In-memory mirror of the access token.
  ///
  /// Secure storage reads hit the platform keystore and are slow enough to be
  /// noticeable when every request needs the token. The cache is populated on
  /// first read and kept in sync by [saveTokens] / [clear].
  String? _cachedAccessToken;
  bool _accessTokenLoaded = false;

  Future<String?> getAccessToken() async {
    if (_accessTokenLoaded) return _cachedAccessToken;

    _cachedAccessToken = await _storage.read(key: _accessTokenKey);
    _accessTokenLoaded = true;
    return _cachedAccessToken;
  }

  Future<String?> getRefreshToken() => _storage.read(key: _refreshTokenKey);

  Future<String?> getUserId() => _storage.read(key: _userIdKey);

  Future<void> saveTokens({
    required String accessToken,
    required String refreshToken,
    String? userId,
  }) async {
    await Future.wait([
      _storage.write(key: _accessTokenKey, value: accessToken),
      _storage.write(key: _refreshTokenKey, value: refreshToken),
      if (userId != null) _storage.write(key: _userIdKey, value: userId),
    ]);

    _cachedAccessToken = accessToken;
    _accessTokenLoaded = true;
  }

  /// Wipes every credential. Called on sign-out and whenever the session
  /// becomes unrecoverable (revoked/reused refresh token).
  Future<void> clear() async {
    await Future.wait([
      _storage.delete(key: _accessTokenKey),
      _storage.delete(key: _refreshTokenKey),
      _storage.delete(key: _userIdKey),
    ]);

    _cachedAccessToken = null;
    _accessTokenLoaded = true;
  }

  Future<bool> hasSession() async {
    final refresh = await getRefreshToken();
    return refresh != null && refresh.isNotEmpty;
  }
}
