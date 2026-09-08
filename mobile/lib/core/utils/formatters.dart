import 'package:decimal/decimal.dart';
import 'package:intl/intl.dart';

/// Display formatting for money, dates and numbers.
///
/// All money values are [Decimal]. `double` is never used for a price: the
/// backend stores `decimal(18,2)` in SQL Server, and round-tripping through
/// IEEE-754 introduces drift that shows up as an off-by-a-piastre total.
abstract final class Formatters {
  /// Egyptian pound. The backend has no multi-currency support, so the symbol
  /// is fixed rather than read from settings.
  static const String currencySymbolAr = 'ج.م';
  static const String currencySymbolEn = 'EGP';

  /// Formats a price with thousands separators and the currency symbol.
  ///
  /// Western digits are used for both languages: Egyptian e-commerce
  /// conventionally shows prices in Western numerals even in Arabic copy, and
  /// `intl`'s `ar` locale would otherwise render Eastern Arabic-Indic digits
  /// (٤٥٠) which users read as unfamiliar in a price context.
  static String price(
    Decimal? value, {
    String lang = 'ar',
    bool withSymbol = true,
  }) {
    if (value == null) return '—';

    final formatter = NumberFormat('#,##0.##', 'en');
    final amount = formatter.format(value.toDouble());

    if (!withSymbol) return amount;

    final symbol = lang == 'en' ? currencySymbolEn : currencySymbolAr;
    // Arabic places the currency after the amount; English before.
    return lang == 'en' ? '$symbol $amount' : '$amount $symbol';
  }

  /// Compact integer, e.g. 1,250.
  static String number(num? value) {
    if (value == null) return '—';
    return NumberFormat('#,##0', 'en').format(value);
  }

  /// Percentage badge value, e.g. `25` for a 25% discount.
  static String percent(int? value) => value == null ? '' : '$value%';

  /// Absolute date, e.g. `٦ سبتمبر ٢٠٢٦` / `6 September 2026`.
  ///
  /// The API returns UTC timestamps; they are converted to local time before
  /// formatting so an order placed at 11pm Cairo time does not display as the
  /// next day.
  static String date(DateTime? value, {String lang = 'ar'}) {
    if (value == null) return '—';
    return DateFormat('d MMMM y', lang).format(value.toLocal());
  }

  /// Date with time, e.g. `6 Sep 2026 — 11:42 PM`.
  static String dateTime(DateTime? value, {String lang = 'ar'}) {
    if (value == null) return '—';
    final local = value.toLocal();
    final d = DateFormat('d MMM y', lang).format(local);
    final t = DateFormat('h:mm a', lang).format(local);
    return '$d — $t';
  }

  /// Short relative time for order lists ("منذ ٣ أيام" / "3 days ago").
  ///
  /// Only the coarse buckets a shopper cares about are implemented; anything
  /// older than a month falls back to an absolute date, which is more useful
  /// than "منذ 7 أشهر".
  static String relative(DateTime? value, {String lang = 'ar'}) {
    if (value == null) return '—';
    final ar = lang != 'en';
    final diff = DateTime.now().difference(value.toLocal());

    if (diff.inMinutes < 1) return ar ? 'الآن' : 'just now';
    if (diff.inMinutes < 60) {
      final n = diff.inMinutes;
      return ar ? 'منذ $n دقيقة' : '$n min ago';
    }
    if (diff.inHours < 24) {
      final n = diff.inHours;
      return ar ? 'منذ $n ساعة' : '$n hr ago';
    }
    if (diff.inDays < 30) {
      final n = diff.inDays;
      return ar ? 'منذ $n يوم' : '$n days ago';
    }
    return date(value, lang: lang);
  }

  /// Masks all but the last four digits of a phone number for display on a
  /// confirmation screen, so a shared screenshot does not leak it in full.
  static String maskPhone(String? phone) {
    if (phone == null || phone.length < 4) return phone ?? '—';
    final tail = phone.substring(phone.length - 4);
    return '${'•' * (phone.length - 4)}$tail';
  }
}
