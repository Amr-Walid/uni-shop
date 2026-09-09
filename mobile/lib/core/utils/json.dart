import 'package:decimal/decimal.dart';

/// Defensive JSON readers used by every `fromJson`.
///
/// Hand-written models mean there is no generated null/type checking, so these
/// helpers absorb the three things that actually break a mobile client in
/// production:
///   1. a field that is absent because the server is an older/newer version,
///   2. a number arriving as `int` where `decimal` was expected (System.Text.
///      Json serialises `100.00m` as `100` when the fraction is zero),
///   3. `null` in a list that the contract declares non-nullable.
///
/// Every reader returns a safe default rather than throwing: one malformed
/// product must not blank the whole catalogue screen.
abstract final class Json {
  static String str(Map<String, dynamic> json, String key,
          {String fallback = ''}) =>
      json[key] is String ? json[key] as String : fallback;

  static String? strOrNull(Map<String, dynamic> json, String key) {
    final value = json[key];
    if (value is! String) return null;
    // Treat whitespace-only as absent: the admin UI saves empty text fields as
    // "" rather than NULL, and an empty badge would render as a blank chip.
    final trimmed = value.trim();
    return trimmed.isEmpty ? null : trimmed;
  }

  static int integer(Map<String, dynamic> json, String key,
      {int fallback = 0}) {
    final value = json[key];
    if (value is int) return value;
    if (value is num) return value.toInt();
    if (value is String) return int.tryParse(value) ?? fallback;
    return fallback;
  }

  static int? intOrNull(Map<String, dynamic> json, String key) {
    final value = json[key];
    if (value is int) return value;
    if (value is num) return value.toInt();
    if (value is String) return int.tryParse(value);
    return null;
  }

  static bool boolean(Map<String, dynamic> json, String key,
      {bool fallback = false}) {
    final value = json[key];
    if (value is bool) return value;
    if (value is num) return value != 0;
    if (value is String) return value.toLowerCase() == 'true';
    return fallback;
  }

  /// Reads a monetary value as [Decimal].
  ///
  /// Parsing goes through the *string* form on purpose. `Decimal.parse(
  /// value.toString())` preserves the exact digits the server sent, whereas
  /// `Decimal.parse(jsonDouble.toString())` would already have been through a
  /// binary float and can yield 99.99000000000001.
  static Decimal money(Map<String, dynamic> json, String key,
      {String fallback = '0'}) {
    return moneyOrNull(json, key) ?? Decimal.parse(fallback);
  }

  static Decimal? moneyOrNull(Map<String, dynamic> json, String key) {
    final value = json[key];
    if (value == null) return null;
    if (value is String) {
      final trimmed = value.trim();
      return trimmed.isEmpty ? null : Decimal.tryParse(trimmed);
    }
    if (value is int) return Decimal.fromInt(value);
    if (value is num) return Decimal.tryParse(value.toString());
    return null;
  }

  /// Parses an ISO-8601 timestamp.
  ///
  /// The API emits UTC. When the server omits the `Z` suffix Dart would parse
  /// the value as *local* time, shifting every date by the timezone offset, so
  /// a naive timestamp is explicitly re-interpreted as UTC.
  static DateTime? dateTime(Map<String, dynamic> json, String key) {
    final value = json[key];
    if (value is! String || value.trim().isEmpty) return null;
    final parsed = DateTime.tryParse(value);
    if (parsed == null) return null;
    return parsed.isUtc ? parsed : DateTime.utc(
          parsed.year,
          parsed.month,
          parsed.day,
          parsed.hour,
          parsed.minute,
          parsed.second,
          parsed.millisecond,
        );
  }

  /// Maps a JSON array into models, skipping entries that fail to parse.
  ///
  /// Skipping rather than propagating is deliberate: one bad row in a list of
  /// 20 products should cost the user that row, not the entire screen.
  static List<T> list<T>(
    Map<String, dynamic> json,
    String key,
    T Function(Map<String, dynamic>) parse,
  ) {
    final value = json[key];
    if (value is! List) return const [];

    final result = <T>[];
    for (final entry in value) {
      if (entry is! Map) continue;
      try {
        result.add(parse(Map<String, dynamic>.from(entry)));
      } catch (_) {
        continue;
      }
    }
    return result;
  }

  /// Reads an array of plain strings (feature bullets, image URLs).
  static List<String> stringList(Map<String, dynamic> json, String key) {
    final value = json[key];
    if (value is! List) return const [];
    return value
        .whereType<String>()
        .map((s) => s.trim())
        .where((s) => s.isNotEmpty)
        .toList();
  }

  static Map<String, dynamic>? object(Map<String, dynamic> json, String key) {
    final value = json[key];
    return value is Map ? Map<String, dynamic>.from(value) : null;
  }
}
