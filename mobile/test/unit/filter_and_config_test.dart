import 'package:decimal/decimal.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:unishop_app/core/theme/app_colors.dart';
import 'package:unishop_app/features/catalog/domain/catalog_filter.dart';
import 'package:unishop_app/features/home/domain/home_data.dart';

void main() {
  group('CatalogFilter equality', () {
    test('attribute id order does not affect equality', () {
      // Providers are keyed on the filter, so an insertion-order difference
      // would trigger a redundant refetch of identical results.
      const a = CatalogFilter(attributeValueIds: {1, 2, 3});
      const b = CatalogFilter(attributeValueIds: {3, 1, 2});

      expect(a, equals(b));
      expect(a.hashCode, equals(b.hashCode));
    });

    test('differing facets are not equal', () {
      const a = CatalogFilter(categorySlug: 'phones');
      const b = CatalogFilter(categorySlug: 'laptops');
      expect(a, isNot(equals(b)));
    });
  });

  group('CatalogFilter.toQueryParameters', () {
    test('sends Decimal prices as exact strings', () {
      // toDouble() would round-trip 1999.99 through a binary float.
      final query = CatalogFilter(
        minPrice: Decimal.parse('1999.99'),
        maxPrice: Decimal.parse('5000'),
      ).toQueryParameters();

      expect(query['minPrice'], '1999.99');
      expect(query['maxPrice'], '5000');
    });

    test('omits the default sort and inactive booleans', () {
      // Null values are stripped by ApiClient._clean, so the request URL stays
      // free of onSale=false noise.
      final query = const CatalogFilter().toQueryParameters();

      expect(query['sort'], isNull);
      expect(query['onSale'], isNull);
      expect(query['inStock'], isNull);
      expect(query['av'], isNull);
    });

    test('sends the wire value for a non-default sort', () {
      final query = const CatalogFilter(sort: CatalogSort.priceAscending)
          .toQueryParameters();
      expect(query['sort'], 'price_asc');
    });

    test('attribute ids are sent as a list for repeated binding', () {
      // ASP.NET Core binds a repeated av=1&av=2 into List<int>.
      final query =
          const CatalogFilter(attributeValueIds: {5, 9}).toQueryParameters();
      expect(query['av'], isA<List<int>>());
      expect((query['av'] as List).length, 2);
    });

    test('a blank query is treated as absent', () {
      expect(
        const CatalogFilter(query: '   ').toQueryParameters()['q'],
        isNull,
      );
    });
  });

  group('CatalogFilter mutation', () {
    test('clear flags remove a value that copyWith(null) cannot', () {
      // The classic copyWith trap: passing null means "leave unchanged".
      final filter = CatalogFilter(
        minPrice: Decimal.fromInt(100),
        categorySlug: 'phones',
      );

      expect(filter.copyWith(minPrice: null).minPrice, isNotNull);
      expect(filter.copyWith(clearPriceRange: true).minPrice, isNull);
      expect(filter.copyWith(clearCategory: true).categorySlug, isNull);
    });

    test('toggleAttributeValue adds then removes', () {
      const filter = CatalogFilter();

      final added = filter.toggleAttributeValue(7);
      expect(added.attributeValueIds, {7});

      final removed = added.toggleAttributeValue(7);
      expect(removed.attributeValueIds, isEmpty);
    });

    test('clearFacets keeps the category scope and sort', () {
      // "Clear filters" on a category page must not navigate the user out of
      // that category.
      final filter = CatalogFilter(
        categorySlug: 'phones',
        sort: CatalogSort.newest,
        minPrice: Decimal.fromInt(100),
        onSaleOnly: true,
        attributeValueIds: const {1},
      );

      final cleared = filter.clearFacets();

      expect(cleared.categorySlug, 'phones');
      expect(cleared.sort, CatalogSort.newest);
      expect(cleared.minPrice, isNull);
      expect(cleared.onSaleOnly, isFalse);
      expect(cleared.attributeValueIds, isEmpty);
    });

    test('activeCount excludes sort but counts each attribute value', () {
      // Sort is always set, so counting it would never let the badge hide.
      final filter = CatalogFilter(
        categorySlug: 'phones',
        minPrice: Decimal.fromInt(100),
        onSaleOnly: true,
        attributeValueIds: const {1, 2},
        sort: CatalogSort.newest,
      );

      expect(filter.activeCount, 5);
      expect(const CatalogFilter(sort: CatalogSort.newest).activeCount, 0);
    });

    test('isEmpty ignores a non-default sort', () {
      expect(const CatalogFilter(sort: CatalogSort.newest).isEmpty, isTrue);
      expect(const CatalogFilter(onSaleOnly: true).isEmpty, isFalse);
    });
  });

  group('AppConfig.isVersionUnsupported', () {
    test('compares components numerically, not lexicographically', () {
      // String comparison puts "1.10.0" before "1.9.0" and would wrongly
      // block a newer build.
      const config = AppConfig(minSupportedVersion: '1.9.0');

      expect(config.isVersionUnsupported('1.10.0'), isFalse);
      expect(config.isVersionUnsupported('1.8.9'), isTrue);
    });

    test('the exact minimum version is supported', () {
      const config = AppConfig(minSupportedVersion: '2.0.0');
      expect(config.isVersionUnsupported('2.0.0'), isFalse);
      expect(config.isVersionUnsupported('1.99.99'), isTrue);
    });

    test('build metadata is stripped before comparing', () {
      // package_info_plus can report a version carrying +build or -beta.
      const config = AppConfig(minSupportedVersion: '1.2.0');
      expect(config.isVersionUnsupported('1.2.0+42'), isFalse);
      expect(config.isVersionUnsupported('1.1.0+42'), isTrue);
    });

    test('unparseable input fails OPEN', () {
      // A version-parsing bug must never lock every user out of the app.
      const config = AppConfig(minSupportedVersion: '1.0.0');
      expect(config.isVersionUnsupported('not-a-version'), isFalse);

      const broken = AppConfig(minSupportedVersion: 'abc');
      expect(broken.isVersionUnsupported('1.0.0'), isFalse);
    });

    test('a short version string is zero-padded', () {
      const config = AppConfig(minSupportedVersion: '1.2.0');
      expect(config.isVersionUnsupported('1.2'), isFalse);
      expect(config.isVersionUnsupported('1.1'), isTrue);
    });
  });

  group('StoreSettings', () {
    test('parses the admin-configured primary colour', () {
      final settings = StoreSettings.fromJson({
        'siteName': 'يونى شوب',
        'primaryColor': '#FF6B35',
        'shippingFee': 50,
        'freeShippingAbove': 5000,
      });

      expect(settings.primaryColor.value, 0xFFFF6B35);
      expect(settings.shippingFee, Decimal.fromInt(50));
    });

    test('falls back to the brand default for a malformed colour', () {
      // Colour values are free text in the database, so an admin typo must
      // not produce a colourless theme.
      final settings = StoreSettings.fromJson({'primaryColor': 'not-a-colour'});
      // Asserted against AppColors.primary rather than a literal. The literal
      // used to be 0xFF1A6BFF, and this test failed when the palette was
      // corrected to the website's actual --primary (#330077) — the assertion
      // was pinning a value the storefront never rendered. Referencing the
      // constant keeps the test about the FALLBACK BEHAVIOUR, which is what it
      // exists to protect, instead of re-stating the brand colour.
      expect(settings.primaryColor.value, AppColors.primary.value);
    });

    test('tagline falls back across languages', () {
      const arOnly = StoreSettings(taglineAr: 'كل ما تحتاجه');
      expect(arOnly.tagline('en'), 'كل ما تحتاجه');
      expect(arOnly.tagline('ar'), 'كل ما تحتاجه');
    });
  });
}
