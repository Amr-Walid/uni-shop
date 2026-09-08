import 'package:decimal/decimal.dart';
import 'package:equatable/equatable.dart';

import '../../../core/utils/json.dart';
import '../../catalog/domain/product.dart';

/// One line in the cart.
///
/// The cart is held locally and only *validated* against the server, so this
/// model is both a local value object and the parse target for
/// `CartItemDto`/`CartResponseDto`.
class CartItem extends Equatable {
  const CartItem({
    required this.productId,
    required this.productName,
    required this.unitPrice,
    required this.quantity,
    this.slug,
    this.emoji,
    this.imageUrl,
    this.brandName,
    this.inStock = true,
  });

  final int productId;
  final String productName;

  /// Price captured when the item was added. The server re-prices at
  /// validation and checkout, so this is a display value only — never the
  /// basis for the amount charged.
  final Decimal unitPrice;

  final int quantity;
  final String? slug;
  final String? emoji;
  final String? imageUrl;
  final String? brandName;
  final bool inStock;

  /// Line total. Computed with [Decimal] multiplication so a 3 × 33.33 line
  /// is exactly 99.99 and the sum of lines matches the server's total.
  Decimal get subTotal => unitPrice * Decimal.fromInt(quantity);

  factory CartItem.fromProduct(Product product, {int quantity = 1}) => CartItem(
        productId: product.id,
        productName: product.name,
        unitPrice: product.price,
        quantity: quantity,
        slug: product.slug,
        emoji: product.emoji,
        imageUrl: product.imageUrl,
        brandName: product.brandName,
        inStock: product.inStock,
      );

  factory CartItem.fromJson(Map<String, dynamic> json) => CartItem(
        productId: Json.integer(json, 'productId'),
        productName: Json.str(json, 'productName'),
        unitPrice: Json.money(json, 'unitPrice'),
        quantity: Json.integer(json, 'quantity', fallback: 1),
        slug: Json.strOrNull(json, 'productSlug') ?? Json.strOrNull(json, 'slug'),
        emoji: Json.strOrNull(json, 'emoji'),
        // The server sends `imagePath` on the legacy CartItemDto and
        // `imageUrl` on the newer shapes; accept both.
        imageUrl: Json.strOrNull(json, 'imageUrl') ??
            Json.strOrNull(json, 'imagePath'),
        brandName: Json.strOrNull(json, 'brandName'),
        inStock: Json.boolean(json, 'inStock', fallback: true),
      );

  /// Local persistence shape.
  ///
  /// Intentionally the SAME `{productId, qty}` pairs the backend's
  /// `UserCart.CartJson` uses, so a signed-in user's cart can be merged
  /// server-side without a translation step.
  Map<String, dynamic> toStorageJson() => {
        'productId': productId,
        'qty': quantity,
        // Cached for offline display; refreshed on every validate.
        'name': productName,
        'price': unitPrice.toString(),
        'slug': slug,
        'emoji': emoji,
        'imageUrl': imageUrl,
        'brandName': brandName,
      };

  factory CartItem.fromStorageJson(Map<String, dynamic> json) => CartItem(
        productId: Json.integer(json, 'productId'),
        productName: Json.str(json, 'name'),
        unitPrice: Json.money(json, 'price'),
        quantity: Json.integer(json, 'qty', fallback: 1),
        slug: Json.strOrNull(json, 'slug'),
        emoji: Json.strOrNull(json, 'emoji'),
        imageUrl: Json.strOrNull(json, 'imageUrl'),
        brandName: Json.strOrNull(json, 'brandName'),
      );

  CartItem copyWith({int? quantity, Decimal? unitPrice, bool? inStock}) =>
      CartItem(
        productId: productId,
        productName: productName,
        unitPrice: unitPrice ?? this.unitPrice,
        quantity: quantity ?? this.quantity,
        slug: slug,
        emoji: emoji,
        imageUrl: imageUrl,
        brandName: brandName,
        inStock: inStock ?? this.inStock,
      );

  @override
  List<Object?> get props => [productId, quantity, unitPrice];
}

/// The cart plus its money totals.
///
/// Totals are recomputed locally for instant feedback, but the server's
/// validation response is authoritative and overwrites them — the shipping
/// rules live in SiteSettings and can change without an app release.
class Cart extends Equatable {
  const Cart({
    this.items = const [],
    Decimal? shippingFee,
    Decimal? freeShippingAbove,
  })  : _shippingFee = shippingFee,
        _freeShippingAbove = freeShippingAbove;

  final List<CartItem> items;
  final Decimal? _shippingFee;
  final Decimal? _freeShippingAbove;

  Decimal get shippingFee => _shippingFee ?? Decimal.zero;

  /// Backend `CartDto.FreeShippingAbove` default.
  Decimal get freeShippingAbove =>
      _freeShippingAbove ?? Decimal.fromInt(5000);

  bool get isEmpty => items.isEmpty;
  bool get isNotEmpty => items.isNotEmpty;

  /// Total number of units, not lines — this is what the tab badge shows.
  int get itemsCount =>
      items.fold(0, (sum, item) => sum + item.quantity);

  Decimal get subTotal => items.fold(
        Decimal.zero,
        (sum, item) => sum + item.subTotal,
      );

  bool get hasFreeShipping => subTotal >= freeShippingAbove;

  Decimal get effectiveShipping =>
      hasFreeShipping ? Decimal.zero : shippingFee;

  Decimal get total => subTotal + effectiveShipping;

  /// How much more the customer must spend to earn free shipping, or null when
  /// already qualified — drives the progress nudge above the checkout button.
  Decimal? get remainingForFreeShipping {
    if (hasFreeShipping) return null;
    final remaining = freeShippingAbove - subTotal;
    return remaining <= Decimal.zero ? null : remaining;
  }

  CartItem? findItem(int productId) {
    for (final item in items) {
      if (item.productId == productId) return item;
    }
    return null;
  }

  int quantityOf(int productId) => findItem(productId)?.quantity ?? 0;

  Cart copyWith({
    List<CartItem>? items,
    Decimal? shippingFee,
    Decimal? freeShippingAbove,
  }) =>
      Cart(
        items: items ?? this.items,
        shippingFee: shippingFee ?? _shippingFee,
        freeShippingAbove: freeShippingAbove ?? _freeShippingAbove,
      );

  /// Parses `CartResponseDto` / `CartValidationResultDto`.
  factory Cart.fromJson(Map<String, dynamic> json) => Cart(
        items: Json.list(json, 'items', CartItem.fromJson),
        shippingFee: Json.moneyOrNull(json, 'shippingFee'),
        freeShippingAbove: Json.moneyOrNull(json, 'freeShippingAbove'),
      );

  /// Payload for `POST /api/v1/cart/validate` and checkout.
  List<Map<String, dynamic>> toRequestItems() => items
      .map((item) => {
            'productId': item.productId,
            'quantity': item.quantity,
          })
      .toList();

  @override
  List<Object?> get props => [items, _shippingFee, _freeShippingAbove];
}
