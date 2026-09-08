// Live end-to-end smoke test.
//
// Drives the REAL repositories against a RUNNING API — no mocks, no fakes. It
// is the closest thing to launching the app that can be verified in a headless
// environment, and it is what caught the `catalog/` route-prefix bug that every
// mocked test missed.
//
// Kept OUTSIDE test/ on purpose. `flutter test` compiles every suite in
// test/ in parallel, and on a memory-constrained machine that starves this
// file's isolate until the 12-minute load timeout fires. Run it explicitly:
//
//   1) cd .. && ./run-demo-api.sh
//   2) flutter test integration_test/live_api_smoke_test.dart \
//        --dart-define=API_BASE_URL=http://localhost:5100
//
// Skips itself when the API is unreachable, so it is safe in CI.
@Timeout(Duration(seconds: 90))
library;

import 'package:decimal/decimal.dart';
import 'package:dio/dio.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:unishop_app/core/config/env.dart';
import 'package:unishop_app/core/network/api_client.dart';
import 'package:unishop_app/core/storage/token_storage.dart';
import 'package:unishop_app/features/cart/domain/cart_item.dart';
import 'package:unishop_app/features/catalog/data/catalog_repository.dart';
import 'package:unishop_app/features/catalog/domain/catalog_filter.dart';
import 'package:unishop_app/features/content/data/content_repository.dart';
import 'package:unishop_app/features/orders/data/orders_repository.dart';

Future<bool> _apiIsUp() async {
  try {
    final probe = Dio(BaseOptions(
      connectTimeout: const Duration(seconds: 3),
      receiveTimeout: const Duration(seconds: 3),
    ));
    final response = await probe.get<dynamic>('${Env.apiBaseUrl}/health');
    return response.statusCode == 200;
  } catch (_) {
    return false;
  }
}

void main() {
  late ApiClient client;
  late CatalogRepository catalog;
  late OrdersRepository orders;
  late ContentRepository content;
  var reachable = false;

  setUpAll(() async {
    reachable = await _apiIsUp();
    if (!reachable) {
      // ignore: avoid_print
      print('SKIPPING: no API at ${Env.apiBaseUrl} — run ./run-demo-api.sh');
      return;
    }

    // The real client, with the real interceptor stack.
    client = ApiClient(tokenStorage: TokenStorage());
    catalog = CatalogRepository(client);
    orders = OrdersRepository(client);
    content = ContentRepository(client);
  });

  group('catalog', () {
    test('home returns the aggregated payload', () async {
      if (!reachable) return;

      final home = await catalog.getHomeData();

      expect(home.categories, isNotEmpty, reason: 'seeded categories');
      expect(home.settings.siteName, isNotEmpty);
      // Proves the shipping rules arrive from SiteSettings rather than being
      // hardcoded in the app.
      expect(home.settings.shippingFee, greaterThan(Decimal.zero));
    });

    test('products page parses and respects the page size', () async {
      if (!reachable) return;

      final page = await catalog.getProducts(
        filter: const CatalogFilter(),
        pageSize: 3,
      );

      expect(page.items, isNotEmpty);
      expect(page.items.length, lessThanOrEqualTo(3));
      expect(page.totalCount, greaterThan(0));

      final product = page.items.first;
      expect(product.name, isNotEmpty, reason: 'localized by the server');
      expect(product.slug, isNotEmpty);
      expect(product.price, greaterThan(Decimal.zero));
    });

    test('Accept-Language switches the localized fields', () async {
      if (!reachable) return;

      client.language = 'ar';
      final arabic = await catalog.getCategories();

      client.language = 'en';
      final english = await catalog.getCategories();

      client.language = 'ar';

      expect(arabic, isNotEmpty);
      expect(english, isNotEmpty);
      // Same records, different resolved names — proves the server is doing
      // the localization and the header is wired through.
      expect(arabic.first.id, english.first.id);
      expect(arabic.first.name, isNot(equals(english.first.name)));
    });

    test('detail-by-slug round-trips from the list', () async {
      if (!reachable) return;

      final page = await catalog.getProducts(
        filter: const CatalogFilter(),
        pageSize: 1,
      );
      final slug = page.items.first.slug;

      final detail = await catalog.getProductBySlug(slug);

      expect(detail.slug, slug);
      expect(detail.categoryId, greaterThan(0));
    });

    test('category filter narrows the result set', () async {
      if (!reachable) return;

      final categories = await catalog.getCategories();
      final target = categories.firstWhere((c) => c.productsCount > 0);

      final filtered = await catalog.getProducts(
        filter: CatalogFilter(categorySlug: target.slug),
      );

      expect(filtered.items, isNotEmpty);
      expect(filtered.totalCount, target.productsCount);
      for (final product in filtered.items) {
        expect(product.categorySlug, target.slug);
      }
    });

    test('price sort is actually applied server-side', () async {
      if (!reachable) return;

      final ascending = await catalog.getProducts(
        filter: const CatalogFilter(sort: CatalogSort.priceAscending),
        pageSize: 10,
      );

      final prices = ascending.items.map((p) => p.price).toList();
      for (var i = 1; i < prices.length; i++) {
        expect(prices[i] >= prices[i - 1], isTrue, reason: 'ascending order');
      }
    });

    test('search finds a product by its own name', () async {
      if (!reachable) return;

      final page = await catalog.getProducts(
        filter: const CatalogFilter(),
        pageSize: 1,
      );
      // First word of the seeded Arabic name.
      final term = page.items.first.name.split(' ').first;

      final results = await catalog.search(query: term);
      expect(results.items, isNotEmpty);
    });

    test('brands and settings load', () async {
      if (!reachable) return;

      expect(await catalog.getBrands(), isNotEmpty);
      final settings = await catalog.getPublicSettings();
      expect(settings.primaryColorHex, startsWith('#'));
    });
  });

  group('cart validation', () {
    test('a valid cart passes and totals match the server', () async {
      if (!reachable) return;

      final page = await catalog.getProducts(
        filter: const CatalogFilter(inStockOnly: true),
        pageSize: 2,
      );

      final cart = Cart(
        items: page.items
            .map((p) => CartItem.fromProduct(p, quantity: 1))
            .toList(),
      );

      final validation = await orders.validateCart(cart);

      expect(validation.isValid, isTrue);
      // The server is authoritative on money; this asserts the client's local
      // arithmetic agrees with it.
      expect(validation.cart.subTotal, cart.subTotal);
    });

    test('an unknown product is reported, not silently dropped', () async {
      if (!reachable) return;

      // Not const: Decimal.one is a runtime getter, not a constant.
      final cart = Cart(
        items: [
          CartItem(
            productId: 999999,
            productName: 'ghost',
            unitPrice: Decimal.one,
            quantity: 1,
          ),
        ],
      );

      final validation = await orders.validateCart(cart);
      expect(validation.hasIssues || !validation.isValid, isTrue);
    });
  });

  group('orders', () {
    test('the status ladder is served for the timeline', () async {
      if (!reachable) return;

      final statuses = await orders.getStatuses();

      expect(statuses.length, greaterThanOrEqualTo(7));
      // Colours come from the DB so an admin-added status still renders.
      expect(statuses.first.colorHex, startsWith('#'));
    });

    test('guest checkout then tracking works end to end', () async {
      if (!reachable) return;

      final page = await catalog.getProducts(
        filter: const CatalogFilter(inStockOnly: true),
        pageSize: 1,
      );
      final cart = Cart(
        items: [CartItem.fromProduct(page.items.first, quantity: 2)],
      );

      const phone = '01012345678';
      final result = await orders.checkout(
        cart: cart,
        customerName: 'عميل اختبار',
        customerPhone: phone,
        address: 'شارع التحرير، وسط البلد، القاهرة',
        city: 'القاهرة',
        governorate: 'القاهرة',
        idempotencyKey: OrdersRepository.newIdempotencyKey(),
      );

      expect(result.orderNumber, isNotEmpty);
      expect(result.total, greaterThan(Decimal.zero));

      // Guest tracking with the phone-digits second factor.
      final tracked = await orders.trackOrder(
        orderNumber: result.orderNumber,
        phoneLast4: phone.substring(phone.length - 4),
      );

      expect(tracked.orderNumber, result.orderNumber);
      expect(tracked.items, isNotEmpty);
      expect(tracked.timeline, isNotEmpty);
      expect(tracked.total, result.total);
    });

    test('the same idempotency key does NOT create a second order', () async {
      if (!reachable) return;

      final page = await catalog.getProducts(
        filter: const CatalogFilter(inStockOnly: true),
        pageSize: 1,
      );
      final cart = Cart(
        items: [CartItem.fromProduct(page.items.first)],
      );

      final key = OrdersRepository.newIdempotencyKey();

      final first = await orders.checkout(
        cart: cart,
        customerName: 'عميل مكرر',
        customerPhone: '01087654321',
        address: 'شارع الهرم، الجيزة، مصر',
        idempotencyKey: key,
      );

      // Simulates a retry after a lost response — the exact scenario the key
      // exists for. Without replay protection this is a duplicate order and a
      // double stock deduction.
      final replay = await orders.checkout(
        cart: cart,
        customerName: 'عميل مكرر',
        customerPhone: '01087654321',
        address: 'شارع الهرم، الجيزة، مصر',
        idempotencyKey: key,
      );

      expect(replay.orderNumber, first.orderNumber);
      expect(replay.orderId, first.orderId);
    });

    test('tracking with wrong phone digits is refused', () async {
      if (!reachable) return;

      final page = await catalog.getProducts(
        filter: const CatalogFilter(inStockOnly: true),
        pageSize: 1,
      );
      final result = await orders.checkout(
        cart: Cart(items: [CartItem.fromProduct(page.items.first)]),
        customerName: 'عميل خصوصية',
        customerPhone: '01111222333',
        address: 'شارع النيل، المعادي، القاهرة',
        idempotencyKey: OrdersRepository.newIdempotencyKey(),
      );

      // The second factor is what stops an order number alone from exposing a
      // customer's name and address.
      await expectLater(
        orders.trackOrder(
          orderNumber: result.orderNumber,
          phoneLast4: '0000',
        ),
        throwsA(anything),
      );
    });
  });

  group('content', () {
    test('app config drives force-update and maintenance', () async {
      if (!reachable) return;

      final config = await content.getAppConfig();

      expect(config.minSupportedVersion, isNotEmpty);
      // The current build must not be locked out by its own config.
      expect(config.isVersionUnsupported('1.0.0'), isFalse);
    });

    test('contact info is served', () async {
      if (!reachable) return;

      // Every field is nullable by design (admin visibility flags), so this
      // asserts it parses rather than asserting any particular value.
      await content.getContactInfo();
    });
  });
}
