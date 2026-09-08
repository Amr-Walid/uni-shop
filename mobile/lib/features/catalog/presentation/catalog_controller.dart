import 'package:dio/dio.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/l10n/locale_controller.dart';
import '../../../core/network/paged_result.dart';
import '../../../core/providers/core_providers.dart';
import '../domain/catalog_filter.dart';
import '../domain/category.dart';
import '../domain/product.dart';
import '../domain/product_attribute.dart';

/// Paged catalog state for one filter.
class CatalogState {
  const CatalogState({
    required this.page,
    this.isLoadingMore = false,
  });

  /// Accumulated pages, not just the latest one.
  final PagedResult<Product> page;

  /// True while an additional page is in flight. Distinct from the outer
  /// `AsyncLoading`, which means the FIRST page is loading — the two need
  /// different UI (skeleton grid vs. a footer spinner).
  final bool isLoadingMore;

  List<Product> get products => page.items;
  bool get hasMore => page.hasNext;
  int get totalCount => page.totalCount;
  bool get isEmpty => page.items.isEmpty;

  CatalogState copyWith({
    PagedResult<Product>? page,
    bool? isLoadingMore,
  }) =>
      CatalogState(
        page: page ?? this.page,
        isLoadingMore: isLoadingMore ?? this.isLoadingMore,
      );
}

/// Drives an infinite-scroll product list for a given [CatalogFilter].
///
/// Keyed by the filter (a value object with proper equality), so navigating
/// back to a category reuses its already-loaded pages instead of refetching,
/// while changing any facet produces a new provider that starts at page 1.
class CatalogController
    extends FamilyAsyncNotifier<CatalogState, CatalogFilter> {
  CancelToken? _cancelToken;

  @override
  Future<CatalogState> build(CatalogFilter arg) async {
    ref.watch(languageCodeProvider);

    // Cancel any request still running for a previous build of this provider.
    // Without this, a slow first page can resolve after a faster subsequent
    // one and overwrite newer results.
    _cancelToken?.cancel();
    final token = CancelToken();
    _cancelToken = token;
    ref.onDispose(() => token.cancel());

    final page = await ref.read(catalogRepositoryProvider).getProducts(
          filter: arg,
          page: 1,
          cancelToken: token,
        );

    return CatalogState(page: page);
  }

  /// Loads the next page and appends it.
  Future<void> loadMore() async {
    final current = state.valueOrNull;
    if (current == null || !current.hasMore || current.isLoadingMore) return;

    state = AsyncData(current.copyWith(isLoadingMore: true));

    try {
      final next = await ref.read(catalogRepositoryProvider).getProducts(
            filter: arg,
            page: current.page.page + 1,
            cancelToken: _cancelToken,
          );

      state = AsyncData(
        current.copyWith(
          // De-duplicated by id: stable server ordering does not prevent an
          // item inserted between two fetches from appearing twice.
          page: next.appendTo(current.page, keyOf: (p) => p.id),
          isLoadingMore: false,
        ),
      );
    } catch (_) {
      // A failed page-append must not discard the pages already shown. The
      // flag is cleared so the user can pull to retry.
      state = AsyncData(current.copyWith(isLoadingMore: false));
    }
  }

  Future<void> refresh() async {
    ref.invalidateSelf();
    await future;
  }
}

final catalogControllerProvider = AsyncNotifierProvider.family<
    CatalogController, CatalogState, CatalogFilter>(CatalogController.new);

// ── Reference data ──────────────────────────────────────────────────────────

/// Categories. Cached for the session — they change rarely and are needed by
/// the home screen, the filter sheet and the catalog tab.
final categoriesProvider = FutureProvider<List<Category>>((ref) async {
  ref.watch(languageCodeProvider);
  return ref.read(catalogRepositoryProvider).getCategories();
});

final brandsProvider = FutureProvider<List<Brand>>((ref) async {
  ref.watch(languageCodeProvider);
  return ref.read(catalogRepositoryProvider).getBrands();
});

/// Filterable attributes for one category.
///
/// Per-category rather than global: the full attribute set across every
/// category is large and mostly irrelevant to the page being viewed.
final categoryAttributesProvider =
    FutureProvider.family<List<ProductAttribute>, int>((ref, categoryId) async {
  return ref.read(catalogRepositoryProvider).getCategoryAttributes(categoryId);
});

/// Full detail for one product, by slug.
final productDetailProvider =
    FutureProvider.family<ProductDetail, String>((ref, slug) async {
  ref.watch(languageCodeProvider);
  return ref.read(catalogRepositoryProvider).getProductBySlug(slug);
});

/// Related products for the detail page.
final relatedProductsProvider =
    FutureProvider.family<List<Product>, int>((ref, productId) async {
  ref.watch(languageCodeProvider);
  return ref.read(catalogRepositoryProvider).getRelatedProducts(productId);
});
