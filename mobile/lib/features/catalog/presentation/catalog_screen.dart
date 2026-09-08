import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/l10n/app_localizations.dart';
import '../../../core/theme/app_spacing.dart';
import '../../../core/widgets/product_card.dart';
import '../../../core/widgets/shimmer_box.dart';
import '../../../core/widgets/state_views.dart';
import '../../../routing/routes.dart';
import '../../cart/presentation/cart_controller.dart';
import '../domain/catalog_filter.dart';
import '../domain/product.dart';
import 'catalog_controller.dart';
import 'filter_sheet.dart';

/// Paged product grid with filtering and sorting.
class CatalogScreen extends ConsumerStatefulWidget {
  const CatalogScreen({
    super.key,
    this.initialFilter = const CatalogFilter(),
    this.title,
  });

  final CatalogFilter initialFilter;
  final String? title;

  @override
  ConsumerState<CatalogScreen> createState() => _CatalogScreenState();
}

class _CatalogScreenState extends ConsumerState<CatalogScreen> {
  late CatalogFilter _filter;
  final _scrollController = ScrollController();

  @override
  void initState() {
    super.initState();
    _filter = widget.initialFilter;
    _scrollController.addListener(_onScroll);
  }

  @override
  void dispose() {
    _scrollController
      ..removeListener(_onScroll)
      ..dispose();
    super.dispose();
  }

  /// Prefetches the next page before the user reaches the end.
  ///
  /// The 400px lead-in means the next page is usually already resolved by the
  /// time it scrolls into view, so the grid does not visibly stall.
  void _onScroll() {
    if (!_scrollController.hasClients) return;

    final position = _scrollController.position;
    if (position.pixels >= position.maxScrollExtent - 400) {
      ref.read(catalogControllerProvider(_filter).notifier).loadMore();
    }
  }

  void _applyFilter(CatalogFilter next) {
    if (next == _filter) return;
    setState(() => _filter = next);
    // Jump to the top: results have changed, and leaving the user mid-scroll
    // in a different result set is disorienting.
    if (_scrollController.hasClients) {
      _scrollController.jumpTo(0);
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = context.l10n;
    final state = ref.watch(catalogControllerProvider(_filter));

    return Scaffold(
      appBar: AppBar(
        title: Text(widget.title ?? l10n.t('catalogTitle')),
        actions: [
          IconButton(
            onPressed: () => context.push(Routes.search),
            icon: const Icon(Icons.search),
            tooltip: l10n.t('search'),
          ),
        ],
      ),
      body: Column(
        children: [
          _FilterBar(
            filter: _filter,
            resultCount: state.valueOrNull?.totalCount,
            onFilterChanged: _applyFilter,
          ),
          const Divider(height: 1),
          Expanded(
            child: state.when(
              loading: () => state.hasValue
                  ? _Grid(
                      state: state.requireValue,
                      controller: _scrollController,
                      filter: _filter,
                    )
                  : const ProductGridSkeleton(itemCount: 6),
              error: (error, _) => state.hasValue
                  ? _Grid(
                      state: state.requireValue,
                      controller: _scrollController,
                      filter: _filter,
                    )
                  : ErrorView(
                      error: error,
                      onRetry: () =>
                          ref.invalidate(catalogControllerProvider(_filter)),
                    ),
              data: (data) => data.isEmpty
                  ? EmptyState(
                      emoji: '🔍',
                      title: l10n.t('catalogNoResults'),
                      message: l10n.t('catalogNoResultsHint'),
                      actionLabel:
                          _filter.isEmpty ? null : l10n.t('clear'),
                      onAction: _filter.isEmpty
                          ? null
                          : () => _applyFilter(_filter.clearFacets()),
                    )
                  : _Grid(
                      state: data,
                      controller: _scrollController,
                      filter: _filter,
                    ),
            ),
          ),
        ],
      ),
    );
  }
}

class _FilterBar extends ConsumerWidget {
  const _FilterBar({
    required this.filter,
    required this.onFilterChanged,
    this.resultCount,
  });

  final CatalogFilter filter;
  final ValueChanged<CatalogFilter> onFilterChanged;
  final int? resultCount;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final l10n = context.l10n;
    final activeCount = filter.activeCount;

    return Padding(
      padding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.lg,
        vertical: AppSpacing.sm,
      ),
      child: Row(
        children: [
          if (resultCount != null)
            Expanded(
              child: Text(
                l10n.tf('catalogResultsCount', {'n': resultCount}),
                style: theme.textTheme.bodySmall,
              ),
            )
          else
            const Spacer(),

          TextButton.icon(
            onPressed: () => _openSort(context),
            icon: const Icon(Icons.swap_vert, size: 18),
            label: Text(l10n.t(filter.sort.labelKey)),
          ),

          const SizedBox(width: AppSpacing.xs),

          // The badge is what tells the user filters are active while the
          // sheet is closed; without it a narrow result set looks like a bug.
          Badge(
            isLabelVisible: activeCount > 0,
            label: Text('$activeCount'),
            child: TextButton.icon(
              onPressed: () => _openFilters(context, ref),
              icon: const Icon(Icons.tune, size: 18),
              label: Text(l10n.t('catalogFilters')),
            ),
          ),
        ],
      ),
    );
  }

  Future<void> _openSort(BuildContext context) async {
    final l10n = context.l10n;

    final selected = await showModalBottomSheet<CatalogSort>(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Padding(
              padding: const EdgeInsets.all(AppSpacing.lg),
              child: Text(
                l10n.t('catalogSort'),
                style: Theme.of(context).textTheme.titleMedium,
              ),
            ),
            for (final option in CatalogSort.values)
              RadioListTile<CatalogSort>(
                value: option,
                groupValue: filter.sort,
                onChanged: (value) => Navigator.of(context).pop(value),
                title: Text(l10n.t(option.labelKey)),
              ),
            const SizedBox(height: AppSpacing.sm),
          ],
        ),
      ),
    );

    if (selected != null) {
      onFilterChanged(filter.copyWith(sort: selected));
    }
  }

  Future<void> _openFilters(BuildContext context, WidgetRef ref) async {
    final result = await showModalBottomSheet<CatalogFilter>(
      context: context,
      isScrollControlled: true,
      builder: (context) => FilterSheet(initialFilter: filter),
    );

    if (result != null) onFilterChanged(result);
  }
}

class _Grid extends ConsumerWidget {
  const _Grid({
    required this.state,
    required this.controller,
    required this.filter,
  });

  final CatalogState state;
  final ScrollController controller;
  final CatalogFilter filter;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return RefreshIndicator(
      onRefresh: () =>
          ref.read(catalogControllerProvider(filter).notifier).refresh(),
      child: GridView.builder(
        controller: controller,
        padding: AppSpacing.pageWithBottomNav,
        // One extra cell holds the trailing loader, so it participates in the
        // grid's scroll extent instead of being a separate sliver.
        itemCount: state.products.length + (state.hasMore ? 1 : 0),
        // mainAxisExtent, not childAspectRatio: the card's body needs a FIXED
        // height (two lines of name, availability, price row, 36dp button)
        // while only the image scales with width. A ratio ties the whole
        // height to the width, which left the body ~11dp short on a normal
        // phone and silently clipped the second line of every long name.
        gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
          crossAxisCount: 2,
          crossAxisSpacing: AppSpacing.md,
          mainAxisSpacing: AppSpacing.md,
          mainAxisExtent: AppSizes.productCardHeight(
            // Tile width for 2 columns: viewport minus page padding and the
            // single gutter between them.
            (MediaQuery.sizeOf(context).width -
                    AppSpacing.lg * 2 -
                    AppSpacing.md) /
                2,
          ),
        ),
        itemBuilder: (context, index) {
          if (index >= state.products.length) {
            return const Center(
              child: SizedBox(
                width: 24,
                height: 24,
                child: CircularProgressIndicator(strokeWidth: 2),
              ),
            );
          }

          final product = state.products[index];

          return ProductCard(
            product: product,
            quantityInCart: ref.watch(cartQuantityProvider(product.id)),
            onTap: () => context.push(Routes.product(product.slug)),
            onAddToCart: () => _addToCart(context, ref, product),
          );
        },
      ),
    );
  }

  void _addToCart(BuildContext context, WidgetRef ref, Product product) {
    ref.read(cartControllerProvider.notifier).addProduct(product);

    final l10n = context.l10n;
    final router = GoRouter.of(context);
    ScaffoldMessenger.of(context)
      ..hideCurrentSnackBar()
      ..showSnackBar(
        SnackBar(
          content: Text(l10n.t('productAddedToCart')),
          duration: const Duration(seconds: 2),
          action: SnackBarAction(
            label: l10n.t('navCart'),
            onPressed: () => router.go(Routes.cart),
          ),
        ),
      );
  }
}
