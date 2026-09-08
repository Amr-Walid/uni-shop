import 'package:decimal/decimal.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:unishop_app/core/utils/json.dart';

/// The Json helpers absorb the three failure modes that actually break a
/// mobile client in production, so each is asserted explicitly.
void main() {
  group('Json.money', () {
    test('parses an integer JSON number without float drift', () {
      // System.Text.Json serialises 100.00m as 100 when the fraction is zero.
      expect(Json.money({'price': 100}, 'price'), Decimal.parse('100'));
    });

    test('preserves exact decimal digits from a double', () {
      // The critical case: going via double.toString() could yield
      // 99.99000000000001. Parsing the string form must not.
      expect(Json.money({'price': 99.99}, 'price'), Decimal.parse('99.99'));
    });

    test('parses a string-encoded amount', () {
      expect(
        Json.money({'price': '1999.50'}, 'price'),
        Decimal.parse('1999.50'),
      );
    });

    test('falls back to zero when the field is missing', () {
      expect(Json.money({}, 'price'), Decimal.zero);
    });

    test('moneyOrNull returns null for null, absent and blank', () {
      expect(Json.moneyOrNull({'oldPrice': null}, 'oldPrice'), isNull);
      expect(Json.moneyOrNull({}, 'oldPrice'), isNull);
      expect(Json.moneyOrNull({'oldPrice': '   '}, 'oldPrice'), isNull);
    });

    test('sums of parsed decimals are exact', () {
      // The whole reason Decimal is used instead of double: three 33.33 lines
      // must total exactly 99.99.
      final unit = Json.money({'p': 33.33}, 'p');
      expect(unit * Decimal.fromInt(3), Decimal.parse('99.99'));
    });
  });

  group('Json.dateTime', () {
    test('treats a naive timestamp as UTC', () {
      // Without this, Dart parses a Z-less timestamp as LOCAL time and every
      // order date shifts by the timezone offset.
      final parsed = Json.dateTime({'at': '2026-09-06T10:30:00'}, 'at');
      expect(parsed!.isUtc, isTrue);
      expect(parsed.hour, 10);
    });

    test('keeps an explicit UTC timestamp', () {
      final parsed = Json.dateTime({'at': '2026-09-06T10:30:00Z'}, 'at');
      expect(parsed!.isUtc, isTrue);
      expect(parsed.hour, 10);
    });

    test('returns null for unparseable or absent values', () {
      expect(Json.dateTime({'at': 'not-a-date'}, 'at'), isNull);
      expect(Json.dateTime({'at': ''}, 'at'), isNull);
      expect(Json.dateTime({}, 'at'), isNull);
    });
  });

  group('Json.strOrNull', () {
    test('treats whitespace-only as absent', () {
      // The admin UI saves empty text fields as "" rather than NULL, and an
      // empty badge would render as a blank chip.
      expect(Json.strOrNull({'badge': '   '}, 'badge'), isNull);
      expect(Json.strOrNull({'badge': ''}, 'badge'), isNull);
    });

    test('trims surrounding whitespace', () {
      expect(Json.strOrNull({'badge': '  جديد  '}, 'badge'), 'جديد');
    });
  });

  group('Json.integer', () {
    test('coerces a numeric string and a double', () {
      expect(Json.integer({'n': '42'}, 'n'), 42);
      expect(Json.integer({'n': 42.0}, 'n'), 42);
    });

    test('uses the fallback for a non-numeric value', () {
      expect(Json.integer({'n': 'abc'}, 'n', fallback: 7), 7);
    });
  });

  group('Json.list', () {
    test('skips entries that fail to parse rather than throwing', () {
      // One bad row in a page of 20 products should cost the user that row,
      // not the entire screen.
      final result = Json.list<int>(
        {
          'items': [
            {'v': 1},
            'not-an-object',
            {'v': 2},
            {'bad': true},
          ]
        },
        'items',
        (json) {
          final value = json['v'];
          if (value is! int) throw const FormatException('missing v');
          return value;
        },
      );

      expect(result, [1, 2]);
    });

    test('returns an empty list when the field is not an array', () {
      expect(Json.list({'items': 'nope'}, 'items', (_) => 1), isEmpty);
      expect(Json.list({}, 'items', (_) => 1), isEmpty);
    });
  });

  group('Json.stringList', () {
    test('drops non-strings and blanks', () {
      final result = Json.stringList(
        {'features': ['أول', '', 42, '  ثانٍ  ', null]},
        'features',
      );
      expect(result, ['أول', 'ثانٍ']);
    });
  });
}
