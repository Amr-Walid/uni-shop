import 'package:decimal/decimal.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:unishop_app/features/cart/domain/cart_item.dart';
import 'package:unishop_app/features/cart/domain/cart_validation.dart';
import 'package:unishop_app/features/catalog/domain/product.dart';

Product _product({
  int id = 1,
  String price = '100.00',
  bool inStock = true,
  bool lowStock = false,
  String? oldPrice,
  int? discountPercent,
}) {
  return Product(
    id: id,
    slug: 'product-$id',
    name: 'منتج $id',
    price: Decimal.parse(price),
    oldPrice: oldPrice == null ? null : Decimal.parse(oldPrice),
    discountPercent: discountPercent,
    inStock: inStock,
    lowStock: lowStock,
    isFeatured: false,
  );
}

void main() {
  group('Cart money arithmetic', () {
    test('line subtotal is exact for a repeating decimal', () {
      final item = CartItem.fromProduct(
        _product(price: '33.33'),
        quantity: 3,
      );
      // double would give 99.99000000000001 here.
      expect(item.subTotal, Decimal.parse('99.99'));
    });

    test('cart subtotal sums lines exactly', () {
      final cart = Cart(
        items: [
          CartItem.fromProduct(_product(id: 1, price: '19.99'), quantity: 3),
          CartItem.fromProduct(_product(id: 2, price: '0.01'), quantity: 7),
        ],
      );
      expect(cart.subTotal, Decimal.parse('60.04'));
    });

    test('itemsCount counts units, not lines', () {
      final cart = Cart(
        items: [
          CartItem.fromProduct(_product(id: 1), quantity: 2),
          CartItem.fromProduct(_product(id: 2), quantity: 3),
        ],
      );
      expect(cart.items.length, 2);
      expect(cart.itemsCount, 5);
    });
  });

  group('Cart shipping rules', () {
    test('shipping is waived at or above the threshold', () {
      final cart = Cart(
        items: [CartItem.fromProduct(_product(price: '5000'))],
        shippingFee: Decimal.fromInt(50),
        freeShippingAbove: Decimal.fromInt(5000),
      );

      // Boundary: the threshold is inclusive (>=), matching the backend's
      // `SubTotal >= FreeShippingAbove`.
      expect(cart.hasFreeShipping, isTrue);
      expect(cart.effectiveShipping, Decimal.zero);
      expect(cart.total, Decimal.parse('5000'));
      expect(cart.remainingForFreeShipping, isNull);
    });

    test('shipping is charged below the threshold', () {
      final cart = Cart(
        items: [CartItem.fromProduct(_product(price: '4999.99'))],
        shippingFee: Decimal.fromInt(50),
        freeShippingAbove: Decimal.fromInt(5000),
      );

      expect(cart.hasFreeShipping, isFalse);
      expect(cart.total, Decimal.parse('5049.99'));
      expect(cart.remainingForFreeShipping, Decimal.parse('0.01'));
    });
  });

  group('CartItem storage round-trip', () {
    test('survives encode then decode', () {
      final original = CartItem.fromProduct(
        _product(price: '1234.56'),
        quantity: 4,
      );

      final restored = CartItem.fromStorageJson(original.toStorageJson());

      expect(restored.productId, original.productId);
      expect(restored.quantity, 4);
      // The price must come back as the same Decimal, not a float
      // approximation of it.
      expect(restored.unitPrice, Decimal.parse('1234.56'));
      expect(restored.productName, original.productName);
    });

    test('storage shape uses the productId/qty keys the backend expects', () {
      // UserCart.CartJson stores [{"ProductId":1,"Qty":2}]; matching the key
      // names is what lets a signed-in cart merge server-side without a
      // translation step.
      final json = CartItem.fromProduct(_product(), quantity: 2)
          .toStorageJson();

      expect(json.containsKey('productId'), isTrue);
      expect(json.containsKey('qty'), isTrue);
      expect(json['qty'], 2);
    });
  });

  group('Product.isOnSale', () {
    test('requires a higher old price AND a discount percent', () {
      // Guards against an admin leaving a stale old price, which would
      // otherwise render "was 100, now 100".
      expect(
        _product(price: '100', oldPrice: '100', discountPercent: 0).isOnSale,
        isFalse,
      );
      expect(
        _product(price: '100', oldPrice: '80', discountPercent: 10).isOnSale,
        isFalse,
      );
      expect(
        _product(price: '80', oldPrice: '100', discountPercent: 20).isOnSale,
        isTrue,
      );
      expect(_product(price: '80').isOnSale, isFalse);
    });
  });

  group('Product.availability', () {
    test('out of stock takes precedence over low stock', () {
      expect(
        _product(inStock: false, lowStock: true).availability,
        ProductAvailability.outOfStock,
      );
      expect(
        _product(inStock: true, lowStock: true).availability,
        ProductAvailability.lowStock,
      );
      expect(
        _product(inStock: true).availability,
        ProductAvailability.inStock,
      );
    });
  });

  group('CartIssueKind.fromCode', () {
    test('maps the server codes the app branches on', () {
      expect(
        CartIssueKind.fromCode('INSUFFICIENT_STOCK'),
        CartIssueKind.insufficientStock,
      );
      expect(
        CartIssueKind.fromCode('PRODUCT_UNAVAILABLE'),
        CartIssueKind.unavailable,
      );
      expect(
        CartIssueKind.fromCode('PRICE_CHANGED'),
        CartIssueKind.priceChanged,
      );
    });

    test('an unrecognised code degrades to unknown, not a crash', () {
      // A new issue type added server-side must not break an old build.
      expect(
        CartIssueKind.fromCode('SOMETHING_NEW'),
        CartIssueKind.unknown,
      );
    });

    test('only stock and price issues are auto-fixable', () {
      const stock = CartIssue(
        productId: 1,
        productName: 'x',
        kind: CartIssueKind.insufficientStock,
        message: '',
      );
      const unavailable = CartIssue(
        productId: 1,
        productName: 'x',
        kind: CartIssueKind.unavailable,
        message: '',
      );

      expect(stock.isAutoFixable, isTrue);
      // Nothing can make an unavailable product purchasable.
      expect(unavailable.isAutoFixable, isFalse);
    });
  });
}
