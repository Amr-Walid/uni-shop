import '../l10n/app_localizations.dart';

/// Form validators.
///
/// Every rule mirrors the DataAnnotations on the matching backend DTO. Keeping
/// them aligned means the user is corrected inline instead of receiving a
/// server-side `VALIDATION_FAILED` after a round trip — but the server remains
/// the authority, and its per-field errors are still surfaced.
abstract final class Validators {
  /// Egyptian mobile numbers: 11 digits beginning 010/011/012/015.
  /// Matches the phone rule used by the web checkout.
  static final RegExp _egyptPhone = RegExp(r'^01[0125][0-9]{8}$');

  static final RegExp _email = RegExp(
    r"^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)+$",
  );

  static String? required(String? value, AppLocalizations l10n) {
    if (value == null || value.trim().isEmpty) {
      return l10n.t('validationRequired');
    }
    return null;
  }

  static String? email(String? value, AppLocalizations l10n,
      {bool isRequired = true}) {
    final trimmed = value?.trim() ?? '';
    if (trimmed.isEmpty) {
      return isRequired ? l10n.t('validationRequired') : null;
    }
    return _email.hasMatch(trimmed) ? null : l10n.t('validationEmail');
  }

  static String? phone(String? value, AppLocalizations l10n,
      {bool isRequired = true}) {
    // Users paste numbers with spaces, dashes and a +20 country code; strip
    // formatting before validating rather than rejecting a valid number.
    final digits = (value ?? '').replaceAll(RegExp(r'[^0-9]'), '');
    final normalised = digits.startsWith('20') && digits.length == 12
        ? '0${digits.substring(2)}'
        : digits;

    if (normalised.isEmpty) {
      return isRequired ? l10n.t('validationRequired') : null;
    }
    return _egyptPhone.hasMatch(normalised) ? null : l10n.t('validationPhone');
  }

  /// Backend Identity policy: minimum 6 characters, no complexity requirement.
  static String? password(String? value, AppLocalizations l10n) {
    if (value == null || value.isEmpty) return l10n.t('validationRequired');
    if (value.length < 6) return l10n.t('validationPasswordShort');
    return null;
  }

  static String? confirmPassword(
    String? value,
    String original,
    AppLocalizations l10n,
  ) {
    if (value == null || value.isEmpty) return l10n.t('validationRequired');
    if (value != original) return l10n.t('validationPasswordMismatch');
    return null;
  }

  static String? minLength(String? value, int min, AppLocalizations l10n) {
    final trimmed = value?.trim() ?? '';
    if (trimmed.isEmpty) return l10n.t('validationRequired');
    if (trimmed.length < min) {
      return l10n.tf('validationMinLength', {'n': min});
    }
    return null;
  }

  /// Order-tracking second factor: exactly the last 4 phone digits.
  static String? phoneLast4(String? value, AppLocalizations l10n) {
    final trimmed = value?.trim() ?? '';
    if (trimmed.isEmpty) return l10n.t('validationRequired');
    if (!RegExp(r'^[0-9]{4}$').hasMatch(trimmed)) {
      return l10n.t('validationDigitsOnly');
    }
    return null;
  }

  /// Normalises a phone number to the `01XXXXXXXXX` form the API stores, so a
  /// number entered as `+20 100 123 4567` matches on tracking lookups.
  static String normalisePhone(String value) {
    final digits = value.replaceAll(RegExp(r'[^0-9]'), '');
    if (digits.startsWith('20') && digits.length == 12) {
      return '0${digits.substring(2)}';
    }
    return digits;
  }
}
