import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/l10n/app_localizations.dart';
import '../../../core/theme/app_spacing.dart';
import '../../../core/widgets/app_logo.dart';
import '../../../core/widgets/fade_in_up.dart';
import '../../../core/widgets/app_network_image.dart';
import '../../../core/widgets/product_card.dart';
import '../../../core/widgets/section_header.dart';
import '../../../core/widgets/shimmer_box.dart';
import '../../../core/widgets/state_views.dart';
import '../../../routing/routes.dart';
import '../../cart/presentation/cart_controller.dart';
import '../../catalog/domain/category.dart';
import '../../catalog/domain/product.dart';
import '../domain/home_data.dart';
import 'home_controller.dart';

class HomeScreen extends ConsumerWidget {
  const HomeScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = context.l10n;
    final home = ref.watch(homeControllerProvider);

    return Scaffold(
      body: SafeArea(
        bottom: false,
        child: RefreshIndicator(
          onRefresh: () =>
              ref.read(homeControllerProvider.notifier).refresh(),
          child: home.when(
            // A retained previous value means this is a pull-to-refresh, not a
            // first load — keep showing the content under the indicator.
            loading: () => home.hasValue
                ? _Content(data: home.requireValue)
                : const _HomeSkeleton(),
            error: (error, _) => home.hasValue
                ? _Content(data: home.requireValue)
                : ListView(
                    // Must stay scrollable or RefreshIndicator cannot be
                    // triggered to retry.
                    children: [
                      SizedBox(
                        height: MediaQuery.sizeOf(context).height * 0.7,
                        child: ErrorView(
                          error: error,
                          onRetry: () =>
                              ref.invalidate(homeControllerProvider),
                        ),
                      ),
                    ],
                  ),
            data: (data) => data.isEmpty
                ? ListView(
                    children: [
                      SizedBox(
                        height: MediaQuery.sizeOf(context).height * 0.7,
                        child: EmptyState(
                          emoji: '🏪',
                          title: l10n.t('catalogNoResults'),
                          message: l10n.t('errorNetworkHint'),
                        ),
                      ),
                    ],
                  )
                : _Content(data: data),
          ),
        ),
      ),
    );
  }
}

class _Content extends ConsumerWidget {
  const _Content({required this.data});

  final HomeData data;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = context.l10n;

    return CustomScrollView(
      slivers: [
        // Staggered entrance, mirroring the site's fadeInUp on its sections.
        // The delays are short and only cover the first screenful: content
        // further down is already animated by scrolling into view, and a
        // longer ladder would make the page feel slow to settle.
        SliverToBoxAdapter(
          child: FadeInUp(child: _Header(settings: data.settings)),
        ),

        if (data.categories.isNotEmpty) ...[
          SliverToBoxAdapter(
            child: FadeInUp(
              delay: const Duration(milliseconds: 60),
              child: SectionHeader(title: l10n.t('homeCategories')),
            ),
          ),
          SliverToBoxAdapter(
            child: FadeInUp(
              delay: const Duration(milliseconds: 100),
              child: _CategoryStrip(categories: data.categories),
            ),
          ),
        ],

        if (data.onSale.isNotEmpty)
          _ProductSection(
            title: l10n.t('homeOffers'),
            products: data.onSale,
            onSeeAll: () => context.push(
              Uri(
                path: Routes.catalog,
                queryParameters: {
                  'onSale': 'true',
                  'title': l10n.t('homeOffers'),
                },
              ).toString(),
            ),
          ),

        if (data.featuredProducts.isNotEmpty)
          _ProductSection(
            title: l10n.t('homeFeatured'),
            products: data.featuredProducts,
          ),

        if (data.newArrivals.isNotEmpty)
          _ProductSection(
            title: l10n.t('homeNewArrivals'),
            products: data.newArrivals,
          ),

        if (data.brands.isNotEmpty) ...[
          SliverToBoxAdapter(
            child: SectionHeader(title: l10n.t('homeBrands')),
          ),
          SliverToBoxAdapter(child: _BrandStrip(brands: data.brands)),
        ],

        // Clears the bottom navigation bar so the last row is fully visible.
        const SliverToBoxAdapter(child: SizedBox(height: AppSpacing.xxxl * 2)),
      ],
    );
  }
}

class _Header extends StatelessWidget {
  const _Header({required this.settings});

  final StoreSettings settings;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = context.l10n;
    final tagline = settings.tagline(l10n.languageCode);

    return Padding(
      padding: const EdgeInsets.fromLTRB(
        AppSpacing.lg,
        AppSpacing.lg,
        AppSpacing.lg,
        AppSpacing.sm,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    // The brand lockup, not `settings.siteName` as plain type.
                    // The website never renders its name as bare text — it
                    // shows the "US" cart mark — so a text-only header was the
                    // most immediately visible way the app failed to look like
                    // the same product.
                    const AppLogo(height: 30),
                    if (tagline.isNotEmpty)
                      Padding(
                        padding: const EdgeInsets.only(top: AppSpacing.xxs),
                        child: Text(
                          tagline,
                          style: theme.textTheme.bodySmall,
                          maxLines: 1,
                          overflow: TextOverflow.ellipsis,
                        ),
                      ),
                  ],
                ),
              ),
            ],
          ),
          const SizedBox(height: AppSpacing.lg),
          _SearchBar(),
        ],
      ),
    );
  }
}

/// Tappable search affordance.
///
/// Not a real TextField: tapping navigates to the dedicated search screen,
/// which owns the debounce, suggestions and result paging. An inline field
/// here would need all of that duplicated.
class _SearchBar extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = context.l10n;

    return Material(
      color: theme.inputDecorationTheme.fillColor,
      borderRadius: AppRadius.pillRadius,
      child: InkWell(
        onTap: () => context.push(Routes.search),
        borderRadius: AppRadius.pillRadius,
        child: Padding(
          padding: const EdgeInsets.symmetric(
            horizontal: AppSpacing.lg,
            vertical: AppSpacing.md,
          ),
          child: Row(
            children: [
              Icon(
                Icons.search,
                size: 20,
                color: theme.textTheme.bodySmall?.color,
              ),
              const SizedBox(width: AppSpacing.sm),
              Expanded(
                child: Text(
                  l10n.t('homeSearchHint'),
                  style: theme.textTheme.bodyMedium?.copyWith(
                    color: theme.textTheme.bodySmall?.color,
                  ),
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class _CategoryStrip extends StatelessWidget {
  const _CategoryStrip({required this.categories});

  final List<Category> categories;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return SizedBox(
      height: 104,
      child: ListView.separated(
        scrollDirection: Axis.horizontal,
        padding: AppSpacing.page,
        itemCount: categories.length,
        separatorBuilder: (_, __) => const SizedBox(width: AppSpacing.md),
        itemBuilder: (context, index) {
          final category = categories[index];

          return SizedBox(
            width: 76,
            child: InkWell(
              onTap: () => context.push(
                Routes.categoryCatalog(category.slug, title: category.name),
              ),
              borderRadius: BorderRadius.circular(AppRadius.sm),
              child: Column(
                children: [
                  AppNetworkImage(
                    url: category.imageUrl,
                    width: 60,
                    height: 60,
                    placeholderEmoji: category.emoji,
                    borderRadius: BorderRadius.circular(AppRadius.pill),
                  ),
                  const SizedBox(height: AppSpacing.xs),
                  Text(
                    category.name,
                    style: theme.textTheme.labelSmall,
                    maxLines: 2,
                    textAlign: TextAlign.center,
                    overflow: TextOverflow.ellipsis,
                  ),
                ],
              ),
            ),
          );
        },
      ),
    );
  }
}

class _BrandStrip extends StatelessWidget {
  const _BrandStrip({required this.brands});

  final List<Brand> brands;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return SizedBox(
      height: 72,
      child: ListView.separated(
        scrollDirection: Axis.horizontal,
        padding: AppSpacing.page,
        itemCount: brands.length,
        separatorBuilder: (_, __) => const SizedBox(width: AppSpacing.md),
        itemBuilder: (context, index) {
          final brand = brands[index];

          return InkWell(
            onTap: () => context.push(
              Routes.brandCatalog(brand.slug, title: brand.name),
            ),
            borderRadius: AppRadius.cardRadius,
            child: Container(
              width: 108,
              padding: const EdgeInsets.all(AppSpacing.sm),
              decoration: BoxDecoration(
                color: theme.cardColor,
                borderRadius: AppRadius.cardRadius,
                border: Border.all(color: theme.dividerColor),
              ),
              alignment: Alignment.center,
              child: brand.logoUrl != null
                  ? AppNetworkImage(
                      url: brand.logoUrl,
                      height: 40,
                      fit: BoxFit.contain,
                    )
                  : Text(
                      brand.name,
                      style: theme.textTheme.labelMedium,
                      maxLines: 2,
                      textAlign: TextAlign.center,
                      overflow: TextOverflow.ellipsis,
                    ),
            ),
          );
        },
      ),
    );
  }
}

class _ProductSection extends ConsumerWidget {
  const _ProductSection({
    required this.title,
    required this.products,
    this.onSeeAll,
  });

  final String title;
  final List<Product> products;
  final VoidCallback? onSeeAll;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    return SliverToBoxAdapter(
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          SectionHeader(title: title, onSeeAll: onSeeAll),
          HorizontalCardList(
            itemCount: products.length,
            itemBuilder: (context, index) {
              final product = products[index];

              return ProductCard(
                product: product,
                quantityInCart: ref.watch(cartQuantityProvider(product.id)),
                onTap: () => context.push(Routes.product(product.slug)),
                onAddToCart: () => _addToCart(context, ref, product),
              );
            },
          ),
          const SizedBox(height: AppSpacing.xl),
        ],
      ),
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
          action: SnackBarAction(
            label: l10n.t('navCart'),
            onPressed: () => context.go(Routes.cart),
          ),
        ),
      );
  }
}

class _HomeSkeleton extends StatelessWidget {
  const _HomeSkeleton();

  @override
  Widget build(BuildContext context) {
    return ListView(
      padding: const EdgeInsets.only(top: AppSpacing.lg),
      children: const [
        Padding(
          padding: AppSpacing.page,
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              ShimmerBox(height: 24, width: 140),
              SizedBox(height: AppSpacing.sm),
              ShimmerBox(height: 14, width: 200),
              SizedBox(height: AppSpacing.lg),
              ShimmerBox(height: 48, borderRadius: AppRadius.pillRadius),
            ],
          ),
        ),
        SizedBox(height: AppSpacing.xl),
        Padding(
          padding: AppSpacing.page,
          child: ShimmerBox(height: 20, width: 100),
        ),
        SizedBox(height: AppSpacing.md),
        ProductCarouselSkeleton(),
      ],
    );
  }
}
