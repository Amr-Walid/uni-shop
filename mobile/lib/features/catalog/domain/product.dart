import 'package:decimal/decimal.dart';
import 'package:equatable/equatable.dart';

import '../../../core/utils/json.dart';

/// A product as it appears in a grid or list.
///
/// Mirrors `ProductListItemDto`. Field names are camelCase because ASP.NET
/// Core's default `JsonSerializerOptions` camel-cases property names.
///
/// Note what is deliberately absent: there is no `stock` count. The backend
/// treats the raw number as commercially sensitive and sends only [inStock] /
/// [lowStock], so the app has nothing to leak.
class Product extends Equatable {
  const Product({
    required this.id,
    required this.slug,
    required this.name,
    required this.price,
    required this.inStock,
    required this.lowStock,
    required this.isFeatured,
    this.shortDesc,
    this.oldPrice,
    this.discountPercent,
    this.badge,
    this.emoji,
    this.imageUrl,
    this.brandName,
    this.categoryName,
    this.categorySlug,
  });

  final int id;
  final String slug;

  /// Already resolved to the request language by the server — never translated
  /// again on the client, which would risk showing a stale string.
  final String name;
  final String? shortDesc;

  final Decimal price;
  final Decimal? oldPrice;

  /// Computed server-side so every client shows the identical figure and
  /// cannot drift through independent rounding.
  final int? discountPercent;

  final String? badge;
  final String? emoji;

  /// Absolute URL. The server resolves it via MediaUrlHelper because the app
  /// has no way to turn `/uploads/x.jpg` into something fetchable.
  final String? imageUrl;

  final String? brandName;
  final String? categoryName;
  final String? categorySlug;

  final bool inStock;
  final bool lowStock;
  final bool isFeatured;

  /// True when the product has a struck-through original price to show.
  ///
  /// `oldPrice` alone is not sufficient: an admin can leave a stale old price
  /// that is at or below the current one, and rendering "was 100, now 100"
  /// looks broken.
  bool get isOnSale =>
      oldPrice != null && oldPrice! > price && (discountPercent ?? 0) > 0;

  /// Availability as a single value, so widgets branch once instead of
  /// combining two booleans in the wrong order.
  ProductAvailability get availability {
    if (!inStock) return ProductAvailability.outOfStock;
    if (lowStock) return ProductAvailability.lowStock;
    return ProductAvailability.inStock;
  }

  factory Product.fromJson(Map<String, dynamic> json) => Product(
        id: Json.integer(json, 'id'),
        slug: Json.str(json, 'slug'),
        name: Json.str(json, 'name'),
        shortDesc: Json.strOrNull(json, 'shortDesc'),
        price: Json.money(json, 'price'),
        oldPrice: Json.moneyOrNull(json, 'oldPrice'),
        discountPercent: Json.intOrNull(json, 'discountPercent'),
        badge: Json.strOrNull(json, 'badge'),
        emoji: Json.strOrNull(json, 'emoji'),
        imageUrl: Json.strOrNull(json, 'imageUrl'),
        brandName: Json.strOrNull(json, 'brandName'),
        categoryName: Json.strOrNull(json, 'categoryName'),
        categorySlug: Json.strOrNull(json, 'categorySlug'),
        inStock: Json.boolean(json, 'inStock'),
        lowStock: Json.boolean(json, 'lowStock'),
        isFeatured: Json.boolean(json, 'isFeatured'),
      );

  @override
  List<Object?> get props => [id, slug, price, inStock, lowStock];
}

enum ProductAvailability { inStock, lowStock, outOfStock }

/// One `attribute: value` row in the specifications table.
class ProductSpec extends Equatable {
  const ProductSpec({
    required this.attributeId,
    required this.name,
    required this.valueId,
    required this.value,
  });

  final int attributeId;
  final String name;
  final int valueId;
  final String value;

  factory ProductSpec.fromJson(Map<String, dynamic> json) => ProductSpec(
        attributeId: Json.integer(json, 'attributeId'),
        name: Json.str(json, 'name'),
        valueId: Json.integer(json, 'valueId'),
        value: Json.str(json, 'value'),
      );

  @override
  List<Object?> get props => [attributeId, valueId];
}

/// Full product detail. Mirrors `ProductDetailDto`.
class ProductDetail extends Equatable {
  const ProductDetail({
    required this.id,
    required this.slug,
    required this.name,
    required this.price,
    required this.inStock,
    required this.lowStock,
    required this.isFeatured,
    required this.categoryId,
    this.shortDesc,
    this.description,
    this.oldPrice,
    this.discountPercent,
    this.badge,
    this.emoji,
    this.features = const [],
    this.brandId,
    this.brandName,
    this.brandSlug,
    this.brandLogoUrl,
    this.categoryName,
    this.categorySlug,
    this.images = const [],
    this.specs = const [],
  });

  final int id;
  final String slug;
  final String name;
  final String? shortDesc;
  final String? description;

  final Decimal price;
  final Decimal? oldPrice;
  final int? discountPercent;

  final String? badge;
  final String? emoji;

  final bool inStock;
  final bool lowStock;
  final bool isFeatured;

  /// Admin-authored bullets. The server parses the stored JSON defensively and
  /// sends an empty list when it is malformed.
  final List<String> features;

  final int? brandId;
  final String? brandName;
  final String? brandSlug;
  final String? brandLogoUrl;

  final int categoryId;
  final String? categoryName;
  final String? categorySlug;

  /// Absolute URLs, main image first.
  final List<String> images;

  final List<ProductSpec> specs;

  bool get isOnSale =>
      oldPrice != null && oldPrice! > price && (discountPercent ?? 0) > 0;

  ProductAvailability get availability {
    if (!inStock) return ProductAvailability.outOfStock;
    if (lowStock) return ProductAvailability.lowStock;
    return ProductAvailability.inStock;
  }

  /// Image for the gallery's initial frame, or null to show a placeholder.
  String? get primaryImage => images.isEmpty ? null : images.first;

  factory ProductDetail.fromJson(Map<String, dynamic> json) => ProductDetail(
        id: Json.integer(json, 'id'),
        slug: Json.str(json, 'slug'),
        name: Json.str(json, 'name'),
        shortDesc: Json.strOrNull(json, 'shortDesc'),
        description: Json.strOrNull(json, 'description'),
        price: Json.money(json, 'price'),
        oldPrice: Json.moneyOrNull(json, 'oldPrice'),
        discountPercent: Json.intOrNull(json, 'discountPercent'),
        badge: Json.strOrNull(json, 'badge'),
        emoji: Json.strOrNull(json, 'emoji'),
        inStock: Json.boolean(json, 'inStock'),
        lowStock: Json.boolean(json, 'lowStock'),
        isFeatured: Json.boolean(json, 'isFeatured'),
        features: Json.stringList(json, 'features'),
        brandId: Json.intOrNull(json, 'brandId'),
        brandName: Json.strOrNull(json, 'brandName'),
        brandSlug: Json.strOrNull(json, 'brandSlug'),
        brandLogoUrl: Json.strOrNull(json, 'brandLogoUrl'),
        categoryId: Json.integer(json, 'categoryId'),
        categoryName: Json.strOrNull(json, 'categoryName'),
        categorySlug: Json.strOrNull(json, 'categorySlug'),
        images: Json.stringList(json, 'images'),
        specs: Json.list(json, 'specs', ProductSpec.fromJson),
      );

  /// Projects to the list shape so a detail screen can seed a "related
  /// products" carousel or the cart without a second fetch.
  Product toListItem() => Product(
        id: id,
        slug: slug,
        name: name,
        shortDesc: shortDesc,
        price: price,
        oldPrice: oldPrice,
        discountPercent: discountPercent,
        badge: badge,
        emoji: emoji,
        imageUrl: primaryImage,
        brandName: brandName,
        categoryName: categoryName,
        categorySlug: categorySlug,
        inStock: inStock,
        lowStock: lowStock,
        isFeatured: isFeatured,
      );

  @override
  List<Object?> get props => [id, slug, price, inStock, lowStock];
}
