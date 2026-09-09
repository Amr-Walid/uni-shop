import 'package:equatable/equatable.dart';

import '../../../core/utils/json.dart';

/// Category tile. Mirrors `CategoryListItemDto`.
class Category extends Equatable {
  const Category({
    required this.id,
    required this.name,
    required this.slug,
    required this.productsCount,
    this.emoji,
    this.imageUrl,
  });

  final int id;
  final String name;
  final String slug;
  final String? emoji;
  final String? imageUrl;

  /// Counted server-side over *visible* products only, so a category whose
  /// stock is all hidden correctly reads as empty.
  final int productsCount;

  bool get isEmpty => productsCount == 0;

  factory Category.fromJson(Map<String, dynamic> json) => Category(
        id: Json.integer(json, 'id'),
        name: Json.str(json, 'name'),
        slug: Json.str(json, 'slug'),
        emoji: Json.strOrNull(json, 'emoji'),
        imageUrl: Json.strOrNull(json, 'imageUrl'),
        productsCount: Json.integer(json, 'productsCount'),
      );

  @override
  List<Object?> get props => [id, slug, productsCount];
}

/// Brand tile. Mirrors `BrandListItemDto`.
class Brand extends Equatable {
  const Brand({
    required this.id,
    required this.name,
    required this.slug,
    required this.productsCount,
    this.logoUrl,
    this.bannerUrl,
  });

  final int id;
  final String name;
  final String slug;
  final String? logoUrl;
  final String? bannerUrl;
  final int productsCount;

  factory Brand.fromJson(Map<String, dynamic> json) => Brand(
        id: Json.integer(json, 'id'),
        name: Json.str(json, 'name'),
        slug: Json.str(json, 'slug'),
        logoUrl: Json.strOrNull(json, 'logoUrl'),
        bannerUrl: Json.strOrNull(json, 'bannerUrl'),
        productsCount: Json.integer(json, 'productsCount'),
      );

  @override
  List<Object?> get props => [id, slug, productsCount];
}
