import 'package:decimal/decimal.dart';
import 'package:equatable/equatable.dart';

/// Sort options accepted by `GET /api/v1/catalog/products?sort=`.
///
/// The wire values are the exact strings `CatalogController.ParseSort` matches;
/// anything else falls back to `Default` server-side, so an unknown value is
/// safe but silently ignored — hence the enum rather than free text.
enum CatalogSort {
  /// Featured first, then the admin's manual SortOrder.
  defaultOrder('default', 'sortDefault'),
  priceAscending('price_asc', 'sortPriceAsc'),
  priceDescending('price_desc', 'sortPriceDesc'),
  newest('newest', 'sortNewest'),
  nameAscending('name_asc', 'sortNameAsc');

  const CatalogSort(this.wireValue, this.labelKey);

  /// Value sent in the `sort` query parameter.
  final String wireValue;

  /// Key into [AppStrings] for the user-facing label.
  final String labelKey;
}

/// Immutable catalog query state.
///
/// Held as a value object so the provider can be keyed on it: two screens
/// requesting the same filter share one cached result, and changing any facet
/// produces a new key that refetches from page 1. [Equatable] is what makes
/// that keying work.
class CatalogFilter extends Equatable {
  const CatalogFilter({
    this.categoryId,
    this.categorySlug,
    this.brandId,
    this.brandSlug,
    this.query,
    this.minPrice,
    this.maxPrice,
    this.attributeValueIds = const {},
    this.onSaleOnly = false,
    this.inStockOnly = false,
    this.sort = CatalogSort.defaultOrder,
  });

  final int? categoryId;
  final String? categorySlug;
  final int? brandId;
  final String? brandSlug;
  final String? query;

  final Decimal? minPrice;
  final Decimal? maxPrice;

  /// Attribute value ids. A product must match ALL of them (AND semantics),
  /// matching the per-value EXISTS the backend generates.
  ///
  /// A [Set] rather than a list: selecting the same chip twice must not send a
  /// duplicate id, which would add a redundant EXISTS clause.
  final Set<int> attributeValueIds;

  final bool onSaleOnly;
  final bool inStockOnly;
  final CatalogSort sort;

  /// True when nothing but the default sort is applied — used to decide
  /// whether to show the "clear filters" affordance.
  bool get isEmpty =>
      categoryId == null &&
      categorySlug == null &&
      brandId == null &&
      brandSlug == null &&
      (query == null || query!.trim().isEmpty) &&
      minPrice == null &&
      maxPrice == null &&
      attributeValueIds.isEmpty &&
      !onSaleOnly &&
      !inStockOnly;

  /// Number of active facets, for the badge on the filter button.
  /// Sort is excluded: it is always set, so counting it would never show zero.
  int get activeCount {
    var count = 0;
    if (categoryId != null || categorySlug != null) count++;
    if (brandId != null || brandSlug != null) count++;
    if (minPrice != null || maxPrice != null) count++;
    if (onSaleOnly) count++;
    if (inStockOnly) count++;
    count += attributeValueIds.length;
    return count;
  }

  /// Serialises to query parameters.
  ///
  /// Nulls are emitted and stripped by `ApiClient._clean` rather than being
  /// omitted here, which keeps this method a pure description of the filter.
  /// `Decimal` is sent via `toString()` so the exact decimal digits reach the
  /// server — `toDouble()` would round-trip 1999.99 through a binary float.
  Map<String, dynamic> toQueryParameters() => {
        'categoryId': categoryId,
        'category': categorySlug,
        'brandId': brandId,
        'brand': brandSlug,
        'q': (query?.trim().isEmpty ?? true) ? null : query!.trim(),
        'minPrice': minPrice?.toString(),
        'maxPrice': maxPrice?.toString(),
        // ASP.NET Core binds a repeated `av=1&av=2` into List<int>; Dio
        // expands an Iterable value into exactly that form.
        'av': attributeValueIds.isEmpty ? null : attributeValueIds.toList(),
        'onSale': onSaleOnly ? true : null,
        'inStock': inStockOnly ? true : null,
        'sort': sort == CatalogSort.defaultOrder ? null : sort.wireValue,
      };

  CatalogFilter copyWith({
    int? categoryId,
    String? categorySlug,
    int? brandId,
    String? brandSlug,
    String? query,
    Decimal? minPrice,
    Decimal? maxPrice,
    Set<int>? attributeValueIds,
    bool? onSaleOnly,
    bool? inStockOnly,
    CatalogSort? sort,
    // Explicit clear flags: `copyWith(minPrice: null)` cannot distinguish
    // "leave unchanged" from "remove", which is the classic copyWith trap.
    bool clearCategory = false,
    bool clearBrand = false,
    bool clearQuery = false,
    bool clearPriceRange = false,
  }) {
    return CatalogFilter(
      categoryId: clearCategory ? null : (categoryId ?? this.categoryId),
      categorySlug:
          clearCategory ? null : (categorySlug ?? this.categorySlug),
      brandId: clearBrand ? null : (brandId ?? this.brandId),
      brandSlug: clearBrand ? null : (brandSlug ?? this.brandSlug),
      query: clearQuery ? null : (query ?? this.query),
      minPrice: clearPriceRange ? null : (minPrice ?? this.minPrice),
      maxPrice: clearPriceRange ? null : (maxPrice ?? this.maxPrice),
      attributeValueIds: attributeValueIds ?? this.attributeValueIds,
      onSaleOnly: onSaleOnly ?? this.onSaleOnly,
      inStockOnly: inStockOnly ?? this.inStockOnly,
      sort: sort ?? this.sort,
    );
  }

  /// Toggles one attribute value chip.
  CatalogFilter toggleAttributeValue(int valueId) {
    final next = Set<int>.from(attributeValueIds);
    if (!next.remove(valueId)) next.add(valueId);
    return copyWith(attributeValueIds: next);
  }

  /// Clears every facet but keeps the category/brand scope and the sort, which
  /// is what "clear filters" means on a category page.
  CatalogFilter clearFacets() => CatalogFilter(
        categoryId: categoryId,
        categorySlug: categorySlug,
        brandId: brandId,
        brandSlug: brandSlug,
        query: query,
        sort: sort,
      );

  @override
  List<Object?> get props => [
        categoryId,
        categorySlug,
        brandId,
        brandSlug,
        query,
        minPrice,
        maxPrice,
        // Sorted so two equal sets with different insertion order compare
        // equal and do not trigger a redundant refetch.
        attributeValueIds.toList()..sort(),
        onSaleOnly,
        inStockOnly,
        sort,
      ];
}
