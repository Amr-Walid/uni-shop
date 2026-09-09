import 'package:equatable/equatable.dart';

import '../../../core/utils/json.dart';

/// The signed-in user. Mirrors `UserProfileDto`.
class UserProfile extends Equatable {
  const UserProfile({
    required this.id,
    required this.email,
    required this.preferredLanguage,
    required this.createdAt,
    this.fullName,
    this.phone,
    this.defaultAddress,
    this.city,
    this.governorate,
    this.roles = const [],
    this.isAdmin = false,
  });

  final String id;
  final String email;
  final String? fullName;
  final String? phone;

  /// Saved shipping details, used to prefill checkout. Their column lengths
  /// deliberately match `SalesOrder`, so a saved profile can never overflow
  /// the order it is copied into.
  final String? defaultAddress;
  final String? city;
  final String? governorate;

  final String preferredLanguage;
  final DateTime createdAt;

  final List<String> roles;

  /// Sent by the server rather than derived from [roles] on the client.
  ///
  /// This is a display hint ONLY — it must never gate a privileged action.
  /// Authorization is enforced by the `[Authorize(Roles = "Admin")]` attributes
  /// on the API, because anything the client decides can be tampered with.
  final bool isAdmin;

  /// Name for greetings, falling back to the local part of the email so the
  /// header is never blank for a user who skipped the name field.
  String get displayName {
    final name = fullName?.trim();
    if (name != null && name.isNotEmpty) return name;
    final at = email.indexOf('@');
    return at > 0 ? email.substring(0, at) : email;
  }

  /// First letter for the avatar placeholder.
  ///
  /// Taken by rune rather than `substring(0, 1)`: a UTF-16 substring splits a
  /// surrogate pair, and a name starting with an emoji would render as a
  /// replacement glyph.
  String get initial {
    final name = displayName;
    if (name.isEmpty) return '?';
    return String.fromCharCode(name.runes.first).toUpperCase();
  }

  /// True when checkout can be prefilled without the user retyping.
  bool get hasCompleteShippingDetails =>
      (fullName?.trim().isNotEmpty ?? false) &&
      (phone?.trim().isNotEmpty ?? false) &&
      (defaultAddress?.trim().isNotEmpty ?? false);

  factory UserProfile.fromJson(Map<String, dynamic> json) => UserProfile(
        id: Json.str(json, 'id'),
        email: Json.str(json, 'email'),
        fullName: Json.strOrNull(json, 'fullName'),
        phone: Json.strOrNull(json, 'phone'),
        defaultAddress: Json.strOrNull(json, 'defaultAddress'),
        city: Json.strOrNull(json, 'city'),
        governorate: Json.strOrNull(json, 'governorate'),
        preferredLanguage:
            Json.str(json, 'preferredLanguage', fallback: 'ar'),
        createdAt: Json.dateTime(json, 'createdAt') ?? DateTime.now().toUtc(),
        roles: Json.stringList(json, 'roles'),
        isAdmin: Json.boolean(json, 'isAdmin'),
      );

  @override
  List<Object?> get props => [id, email, fullName, phone, defaultAddress];
}

/// A successful authentication. Mirrors `AuthResultDto`.
class AuthResult extends Equatable {
  const AuthResult({
    required this.accessToken,
    required this.refreshToken,
    required this.expiresIn,
    required this.user,
    this.tokenType = 'Bearer',
  });

  final String accessToken;

  /// Rotated on every refresh. The server hashes it and tracks a token family,
  /// so presenting a superseded token is treated as theft and revokes the
  /// whole family — the client must therefore always store the newest one.
  final String refreshToken;

  /// Access-token lifetime in seconds (15 minutes server-side).
  final int expiresIn;

  final String tokenType;
  final UserProfile user;

  /// Absolute expiry, computed on receipt.
  ///
  /// A 30-second safety margin is subtracted so a token is never sent in the
  /// instant it lapses in flight.
  DateTime get expiresAt =>
      DateTime.now().toUtc().add(Duration(seconds: expiresIn - 30));

  factory AuthResult.fromJson(Map<String, dynamic> json) => AuthResult(
        accessToken: Json.str(json, 'accessToken'),
        refreshToken: Json.str(json, 'refreshToken'),
        expiresIn: Json.integer(json, 'expiresIn', fallback: 900),
        tokenType: Json.str(json, 'tokenType', fallback: 'Bearer'),
        user: UserProfile.fromJson(Json.object(json, 'user') ?? const {}),
      );

  @override
  List<Object?> get props => [accessToken, refreshToken, user];
}

/// Authentication state for the whole app.
sealed class AuthState extends Equatable {
  const AuthState();

  @override
  List<Object?> get props => [];
}

/// Tokens are being restored from secure storage at startup.
///
/// Distinct from [Unauthenticated] on purpose: showing a login screen during
/// this window would flash it in front of a user who is already signed in.
class AuthLoading extends AuthState {
  const AuthLoading();
}

/// No session. The store is fully browsable in this state — sign-in is only
/// required to persist orders, addresses and the wishlist.
class Unauthenticated extends AuthState {
  const Unauthenticated({this.reason});

  /// Set when the session ended involuntarily (refresh reuse detected,
  /// account locked), so the UI can explain why the user was signed out.
  final String? reason;

  @override
  List<Object?> get props => [reason];
}

class Authenticated extends AuthState {
  const Authenticated(this.user);

  final UserProfile user;

  @override
  List<Object?> get props => [user];
}
