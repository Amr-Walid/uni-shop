import 'package:dio/dio.dart';

import '../../../core/network/api_client.dart';
import '../../../core/network/paged_result.dart';
import '../../home/domain/home_data.dart';
import '../domain/catalog_filter.dart';
import '../domain/category.dart';
import '../domain/product.dart';
import '../domain/product_attribute.dart';

/// Read-only catalog access.
///
/// Every method returns a parsed model or throws an `AppException` — the
/// ApiClient guarantees that, so callers never see a `DioException` and no
/// try/catch appears in a provider or widget.
class CatalogRepository {
  const CatalogRepository(this._client);

  final ApiClient _client;

  /// One page of products. Backed by `GET /catalog/products`.
  ///
  /// [cancelToken] matters here: the catalog refetches on every filter change,
  /// and without cancellation a slow first request can resolve *after* a
  /// faster second one and overwrite the newer results.
  Future<PagedResult<Product>> getProducts({
    required CatalogFilter filter,
    int page = 1,
    int pageSize = PagedResult.defaultPageSize,
    CancelToken? cancelToken,
  }) async {
    final json = await _client.get<Map<String, dynamic>>(
      'products',
      query: {
        ...filter.toQueryParameters(),
        'page': page,
        'pageSize': pageSize,
      },
      cancelToken: cancelToken,
    );

    return PagedResult.fromJson(json, Product.fromJson);
  }

  /// Full detail by slug. Backed by `GET /catalog/products/{slug}`.
  ///
  /// Slug rather than id because the same URL is shareable and is what the
  /// website uses, so a deep link from a shared page resolves identically.
  Future<ProductDetail> getProductBySlug(
    String slug, {
    CancelToken? cancelToken,
  }) async {
    final json = await _client.get<Map<String, dynamic>>(
      'products/$slug',
      cancelToken: cancelToken,
    );
    return ProductDetail.fromJson(json);
  }

  /// Related products for the detail page.
  Future<List<Product>> getRelatedProducts(
    int productId, {
    int limit = 8,
    CancelToken? cancelToken,
  }) async {
    final json = await _client.get<List<dynamic>>(
      'products/$productId/related',
      query: {'limit': limit},
      cancelToken: cancelToken,
    );
    return _parseList(json, Product.fromJson);
  }

  Future<List<Product>> getFeaturedProducts({
    int limit = 10,
    CancelToken? cancelToken,
  }) async {
    final json = await _client.get<List<dynamic>>(
      'products/featured',
      query: {'limit': limit},
      cancelToken: cancelToken,
    );
    return _parseList(json, Product.fromJson);
  }

  Future<List<Category>> getCategories({CancelToken? cancelToken}) async {
    final json = await _client.get<List<dynamic>>(
      'categories',
      cancelToken: cancelToken,
    );
    return _parseList(json, Category.fromJson);
  }

  Future<List<Brand>> getBrands({CancelToken? cancelToken}) async {
    final json = await _client.get<List<dynamic>>(
      'brands',
      cancelToken: cancelToken,
    );
    return _parseList(json, Brand.fromJson);
  }

  /// Filterable attributes for a category, used to build the filter sheet.
  ///
  /// Fetched per category rather than globally: the full attribute set across
  /// every category is large and mostly irrelevant to the page being viewed.
  Future<List<ProductAttribute>> getCategoryAttributes(
    int categoryId, {
    CancelToken? cancelToken,
  }) async {
    final json = await _client.get<List<dynamic>>(
      'categories/$categoryId/attributes',
      cancelToken: cancelToken,
    );
    return _parseList(json, ProductAttribute.fromJson);
  }

  /// Full-text search. Backed by `GET /catalog/search`.
  Future<PagedResult<Product>> search({
    required String query,
    int page = 1,
    int pageSize = PagedResult.defaultPageSize,
    CatalogSort sort = CatalogSort.defaultOrder,
    CancelToken? cancelToken,
  }) async {
    final json = await _client.get<Map<String, dynamic>>(
      'search',
      query: {
        'q': query,
        'page': page,
        'pageSize': pageSize,
        'sort': sort == CatalogSort.defaultOrder ? null : sort.wireValue,
      },
      cancelToken: cancelToken,
    );
    return PagedResult.fromJson(json, Product.fromJson);
  }

  /// Lightweight autocomplete for the search field.
  Future<List<String>> searchSuggestions(
    String query, {
    int limit = 8,
    CancelToken? cancelToken,
  }) async {
    final json = await _client.get<List<dynamic>>(
      'search/suggest',
      query: {'q': query, 'limit': limit},
      cancelToken: cancelToken,
    );
    return json.whereType<String>().toList();
  }

  /// The whole home screen in one round-trip.
  Future<HomeData> getHomeData({CancelToken? cancelToken}) async {
    final json = await _client.get<Map<String, dynamic>>(
      'home',
      cancelToken: cancelToken,
    );
    return HomeData.fromJson(json);
  }

  Future<StoreSettings> getPublicSettings({CancelToken? cancelToken}) async {
    final json = await _client.get<Map<String, dynamic>>(
      'settings/public',
      cancelToken: cancelToken,
    );
    return StoreSettings.fromJson(json);
  }

  /// Parses a bare JSON array, skipping malformed entries.
  ///
  /// Mirrors `Json.list` but for a top-level array rather than a field, since
  /// several endpoints return an array directly instead of an envelope.
  static List<T> _parseList<T>(
    List<dynamic> json,
    T Function(Map<String, dynamic>) parse,
  ) {
    final result = <T>[];
    for (final entry in json) {
      if (entry is! Map) continue;
      try {
        result.add(parse(Map<String, dynamic>.from(entry)));
      } catch (_) {
        continue;
      }
    }
    return result;
  }
}
