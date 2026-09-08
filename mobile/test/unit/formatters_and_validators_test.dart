import 'package:decimal/decimal.dart';
import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'package:unishop_app/core/l10n/app_localizations.dart';
import 'package:unishop_app/core/utils/formatters.dart';
import 'package:unishop_app/core/utils/validators.dart';

void main() {
  // Mirrors main(): DateFormat with an explicit locale needs the per-locale
  // symbols loaded, or it throws LocaleDataException.
  setUpAll(initializeDateFormatting);

  const ar = AppLocalizations(Locale('ar'));
  const en = AppLocalizations(Locale('en'));

  group('Formatters.price', () {
    test('places the currency after the amount in Arabic', () {
      expect(
        Formatters.price(Decimal.parse('1500'), lang: 'ar'),
        '1,500 ج.م',
      );
    });

    test('places the currency before the amount in English', () {
      expect(
        Formatters.price(Decimal.parse('1500'), lang: 'en'),
        'EGP 1,500',
      );
    });

    test('uses Western digits even in Arabic', () {
      // Egyptian e-commerce shows prices in Western numerals even in Arabic
      // copy; intl's `ar` locale would emit ٤٥٠.
      final formatted = Formatters.price(Decimal.parse('450'), lang: 'ar');
      expect(formatted, contains('450'));
      expect(formatted, isNot(contains('٤')));
    });

    test('keeps significant decimals and drops trailing zeros', () {
      expect(
        Formatters.price(Decimal.parse('99.99'), lang: 'ar', withSymbol: false),
        '99.99',
      );
      expect(
        Formatters.price(Decimal.parse('100.00'), lang: 'ar', withSymbol: false),
        '100',
      );
    });

    test('renders a dash for a null amount', () {
      expect(Formatters.price(null), '—');
    });
  });

  group('Formatters.maskPhone', () {
    test('reveals only the last four digits', () {
      // This appears on the order screen, which is the most likely to be
      // screenshotted and shared.
      expect(Formatters.maskPhone('01012345678'), '•••••••5678');
    });

    test('handles short or absent input without throwing', () {
      expect(Formatters.maskPhone('123'), '123');
      expect(Formatters.maskPhone(null), '—');
    });
  });

  group('Formatters.relative', () {
    test('buckets recent times', () {
      final now = DateTime.now();
      expect(
        Formatters.relative(now.subtract(const Duration(seconds: 5)),
            lang: 'ar'),
        'الآن',
      );
      expect(
        Formatters.relative(now.subtract(const Duration(hours: 3)),
            lang: 'en'),
        '3 hr ago',
      );
    });

    test('falls back to an absolute date beyond a month', () {
      // "منذ 7 أشهر" is less useful than the actual date.
      final old = DateTime.now().subtract(const Duration(days: 200));
      final formatted = Formatters.relative(old, lang: 'en');
      expect(formatted, isNot(contains('ago')));
    });
  });

  group('Validators.phone', () {
    test('accepts the four Egyptian mobile prefixes', () {
      for (final prefix in ['010', '011', '012', '015']) {
        expect(
          Validators.phone('${prefix}12345678', ar),
          isNull,
          reason: '$prefix must be accepted',
        );
      }
    });

    test('rejects a wrong prefix or length', () {
      expect(Validators.phone('01312345678', ar), isNotNull);
      expect(Validators.phone('0101234567', ar), isNotNull);
      expect(Validators.phone('010123456789', ar), isNotNull);
    });

    test('accepts formatted and international forms', () {
      // Users paste numbers with spaces, dashes and a +20 country code;
      // rejecting a valid number over formatting is pure friction.
      expect(Validators.phone('+20 100 123 4567', ar), isNull);
      expect(Validators.phone('010-1234-5678', ar), isNull);
      expect(Validators.phone('0101 234 5678', ar), isNull);
    });

    test('is optional when isRequired is false', () {
      expect(Validators.phone('', ar, isRequired: false), isNull);
      expect(Validators.phone('', ar), isNotNull);
    });
  });

  group('Validators.normalisePhone', () {
    test('promotes +20 to the stored 01 form', () {
      // Guest tracking matches on the stored format, so normalising is what
      // lets an order be found later.
      expect(Validators.normalisePhone('+20 100 123 4567'), '01001234567');
      expect(Validators.normalisePhone('201001234567'), '01001234567');
    });

    test('strips formatting from an already-local number', () {
      expect(Validators.normalisePhone('010-1234-5678'), '01012345678');
    });
  });

  group('Validators.email', () {
    test('accepts valid addresses and rejects malformed ones', () {
      expect(Validators.email('user@example.com', ar), isNull);
      expect(Validators.email('user.name+tag@sub.example.co', ar), isNull);
      expect(Validators.email('no-at-sign', ar), isNotNull);
      expect(Validators.email('user@nodot', ar), isNotNull);
      expect(Validators.email('@example.com', ar), isNotNull);
    });
  });

  group('Validators.password', () {
    test('enforces the backend Identity minimum of 6', () {
      expect(Validators.password('12345', ar), isNotNull);
      expect(Validators.password('123456', ar), isNull);
    });

    test('confirmPassword requires an exact match', () {
      expect(Validators.confirmPassword('abc123', 'abc123', ar), isNull);
      expect(Validators.confirmPassword('abc123', 'abc124', ar), isNotNull);
    });
  });

  group('Validators.phoneLast4', () {
    test('requires exactly four digits', () {
      expect(Validators.phoneLast4('5678', ar), isNull);
      expect(Validators.phoneLast4('567', ar), isNotNull);
      expect(Validators.phoneLast4('56789', ar), isNotNull);
      expect(Validators.phoneLast4('56a8', ar), isNotNull);
    });
  });

  group('AppLocalizations', () {
    test('resolves Arabic and English for the same key', () {
      expect(ar.t('navCart'), 'السلة');
      expect(en.t('navCart'), 'Cart');
    });

    test('an unsupported locale falls back to Arabic', () {
      const fr = AppLocalizations(Locale('fr'));
      expect(fr.languageCode, 'ar');
      expect(fr.t('navCart'), 'السلة');
    });

    test('direction is RTL for Arabic and LTR for English', () {
      expect(ar.textDirection, TextDirection.rtl);
      expect(en.textDirection, TextDirection.ltr);
      expect(ar.isArabic, isTrue);
    });

    test('tf interpolates placeholders', () {
      // Arabic word order differs from English, so the placeholder cannot be
      // built by appending a suffix.
      expect(ar.tf('catalogResultsCount', {'n': 12}), '12 منتج');
      expect(en.tf('catalogResultsCount', {'n': 12}), '12 products');
    });

    test('every key defines both languages', () {
      // A missing translation would silently render the key name in release.
      for (final entry in [
        'navHome',
        'cartCheckout',
        'checkoutPlaceOrder',
        'orderTrackNotFound',
        'errorSessionExpired',
        'validationPhone',
      ]) {
        expect(ar.t(entry), isNot(equals(entry)));
        expect(en.t(entry), isNot(equals(entry)));
      }
    });
  });
}
