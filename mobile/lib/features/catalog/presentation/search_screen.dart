import 'dart:async';

import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/l10n/app_localizations.dart';
import '../../../core/providers/core_providers.dart';
import '../../../core/theme/app_spacing.dart';
import '../../../core/widgets/product_card.dart';
import '../../../core/widgets/shimmer_box.dart';
import '../../../core/widgets/state_views.dart';
import '../../../routing/routes.dart';
import '../../cart/presentation/cart_controller.dart';
import '../domain/catalog_filter.dart';
import '../domain/product.dart';
import 'catalog_controller.dart';

class SearchScreen extends ConsumerStatefulWidget {
  const SearchScreen({super.key, this.initialQuery});

  final String? initialQuery;

  @override
  ConsumerState<SearchScreen> createState() => _SearchScreenState();
}

class _SearchScreenState extends ConsumerState<SearchScreen> {
  late final TextEditingController _controller;
  final _focusNode = FocusNode();

  /// Debounce so typing does not fire one request per keystroke.
  ///
  /// 350 ms is chosen to sit just above normal inter-key latency: shorter and
  /// a fast typist still triggers several requests, longer and the results
  /// feel laggy. The backend also rate-limits this endpoint.
  static const _debounce = Duration(milliseconds: 350);
  Timer? _debounceTimer;

  /// The committed term the grid is showing, as opposed to the raw field text.
  String _submittedQuery = '';

  /// Live suggestions for the current field text.
  List<String> _suggestions = const [];
  bool _showSuggestions = false;

  @override
  void initState() {
    super.initState();
    _controller = TextEditingController(text: widget.initialQuery ?? '');
    _submittedQuery = widget.initialQuery?.trim() ?? '';

    // Autofocus only when arriving with no query: if a term was passed in, the
    // user wants to see results, not a keyboard covering them.
    if (_submittedQuery.isEmpty) {
      WidgetsBinding.instance.addPostFrameCallback((_) {
        if (mounted) _focusNode.requestFocus();
      });
    }
  }

  @override
  void dispose() {
    _debounceTimer?.cancel();
    _controller.dispose();
    _focusNode.dispose();
    super.dispose();
  }

  void _onChanged(String value) {
    _debounceTimer?.cancel();

    final trimmed = value.trim();
    // The backend ignores anything shorter than two characters, since the
    // result set would be meaningless and expensive.
    if (trimmed.length < 2) {
      setState(() {
        _suggestions = const [];
        _showSuggestions = false;
      });
      return;
    }

    _debounceTimer = Timer(_debounce, () => _fetchSuggestions(trimmed));
  }

  Future<void> _fetchSuggestions(String query) async {
    try {
      final results = await ref
          .read(catalogRepositoryProvider)
          .searchSuggestions(query);

      if (!mounted) return;
      setState(() {
        _suggestions = results;
        _showSuggestions = results.isNotEmpty;
      });
    } catch (_) {
      // Suggestions are a convenience; a failure must not interrupt typing or
      // surface an error over the field.
      if (!mounted) return;
      setState(() {
        _suggestions = const [];
        _showSuggestions = false;
      });
    }
  }

  void _submit(String value) {
    final trimmed = value.trim();
    if (trimmed.isEmpty) return;

    _debounceTimer?.cancel();
    _focusNode.unfocus();

    setState(() {
      _submittedQuery = trimmed;
      _showSuggestions = false;
    });
  }

  void _pickSuggestion(String suggestion) {
    _controller.text = suggestion;
    _submit(suggestion);
  }

  @override
  Widget build(BuildContext context) {
    final l10n = context.l10n;

    return Scaffold(
      appBar: AppBar(
        titleSpacing: 0,
        title: TextField(
          controller: _controller,
          focusNode: _focusNode,
          textInputAction: TextInputAction.search,
          onChanged: _onChanged,
          onSubmitted: _submit,
          decoration: InputDecoration(
            hintText: l10n.t('homeSearchHint'),
            filled: false,
            border: InputBorder.none,
            enabledBorder: InputBorder.none,
            focusedBorder: InputBorder.none,
            suffixIcon: _controller.text.isEmpty
                ? null
                : IconButton(
                    onPressed: () {
                      _controller.clear();
                      setState(() {
                        _submittedQuery = '';
                        _suggestions = const [];
                        _showSuggestions = false;
                      });
                      _focusNode.requestFocus();
                    },
                    icon: const Icon(Icons.close),
                  ),
          ),
        ),
      ),
      body: _showSuggestions
          ? _SuggestionList(
              suggestions: _suggestions,
              onSelected: _pickSuggestion,
            )
          : _submittedQuery.isEmpty
              ? EmptyState(
                  icon: Icons.search,
                  title: l10n.t('search'),
                  message: l10n.t('homeSearchHint'),
                )
              : _Results(query: _submittedQuery),
    );
  }
}

class _SuggestionList extends StatelessWidget {
  const _SuggestionList({
    required this.suggestions,
    required this.onSelected,
  });

  final List<String> suggestions;
  final ValueChanged<String> onSelected;

  @override
  Widget build(BuildContext context) {
    return ListView.separated(
      itemCount: suggestions.length,
      separatorBuilder: (_, __) => const Divider(height: 1),
      itemBuilder: (context, index) {
        final suggestion = suggestions[index];
        return ListTile(
          leading: const Icon(Icons.search, size: 20),
          title: Text(suggestion),
          onTap: () => onSelected(suggestion),
        );
      },
    );
  }
}

/// Search results.
///
/// Reuses the catalog controller with a query-only filter, so paging,
/// de-duplication and cancellation behave exactly as they do in the shop tab
/// rather than being reimplemented here.
class _Results extends ConsumerStatefulWidget {
  const _Results({required this.query});

  final String query;

  @override
  ConsumerState<_Results> createState() => _ResultsState();
}

class _ResultsState extends ConsumerState<_Results> {
  final _scrollController = ScrollController();

  CatalogFilter get _filter => CatalogFilter(query: widget.query);

  @override
  void initState() {
    super.initState();
    _scrollController.addListener(_onScroll);
  }

  @override
  void dispose() {
    _scrollController
      ..removeListener(_onScroll)
      ..dispose();
    super.dispose();
  }

  void _onScroll() {
    if (!_scrollController.hasClients) return;
    final position = _scrollController.position;
    if (position.pixels >= position.maxScrollExtent - 400) {
      ref.read(catalogControllerProvider(_filter).notifier).loadMore();
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = context.l10n;
    final state = ref.watch(catalogControllerProvider(_filter));

    return state.when(
      loading: () => const ProductGridSkeleton(itemCount: 4),
      error: (error, _) => ErrorView(
        error: error,
        onRetry: () => ref.invalidate(catalogControllerProvider(_filter)),
      ),
      data: (data) {
        if (data.isEmpty) {
          return EmptyState(
            emoji: '🔍',
            title: l10n.t('catalogNoResults'),
            message: l10n.t('catalogNoResultsHint'),
          );
        }

        return Column(
          children: [
            Padding(
              padding: const EdgeInsets.symmetric(
                horizontal: AppSpacing.lg,
                vertical: AppSpacing.sm,
              ),
              child: Align(
                alignment: AlignmentDirectional.centerStart,
                child: Text(
                  l10n.tf('catalogResultsCount', {'n': data.totalCount}),
                  style: Theme.of(context).textTheme.bodySmall,
                ),
              ),
            ),
            Expanded(
              child: GridView.builder(
                controller: _scrollController,
                padding: AppSpacing.pageWithBottomNav,
                itemCount: data.products.length + (data.hasMore ? 1 : 0),
                // Fixed body height plus a square image — see
                // AppSizes.productCardHeight. A childAspectRatio here clipped
                // the second line of long product names.
                gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                  crossAxisCount: 2,
                  crossAxisSpacing: AppSpacing.md,
                  mainAxisSpacing: AppSpacing.md,
                  mainAxisExtent: AppSizes.productCardHeight(
                    (MediaQuery.sizeOf(context).width -
                            AppSpacing.lg * 2 -
                            AppSpacing.md) /
                        2,
                  ),
                ),
                itemBuilder: (context, index) {
                  if (index >= data.products.length) {
                    return const Center(
                      child: SizedBox(
                        width: 24,
                        height: 24,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      ),
                    );
                  }

                  final product = data.products[index];

                  return ProductCard(
                    product: product,
                    quantityInCart:
                        ref.watch(cartQuantityProvider(product.id)),
                    onTap: () => context.push(Routes.product(product.slug)),
                    onAddToCart: () => _addToCart(context, ref, product),
                  );
                },
              ),
            ),
          ],
        );
      },
    );
  }

  void _addToCart(BuildContext context, WidgetRef ref, Product product) {
    ref.read(cartControllerProvider.notifier).addProduct(product);
    final l10n = context.l10n;
    ScaffoldMessenger.of(context)
      ..hideCurrentSnackBar()
      ..showSnackBar(
        SnackBar(
          content: Text(l10n.t('productAddedToCart')),
          duration: const Duration(seconds: 2),
        ),
      );
  }
}
