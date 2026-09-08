import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/l10n/app_localizations.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_spacing.dart';
import '../../../core/widgets/app_network_image.dart';
import '../../../core/widgets/price_tag.dart';
import '../../../core/widgets/product_card.dart';
import '../../../core/widgets/section_header.dart';
import '../../../core/widgets/shimmer_box.dart';
import '../../../core/widgets/state_views.dart';
import '../../../routing/routes.dart';
import '../../cart/presentation/cart_controller.dart';
import '../../catalog/domain/product.dart';
import '../../catalog/presentation/catalog_controller.dart';

class ProductDetailScreen extends ConsumerStatefulWidget {
  const ProductDetailScreen({super.key, required this.slug});

  final String slug;

  @override
  ConsumerState<ProductDetailScreen> createState() =>
      _ProductDetailScreenState();
}

class _ProductDetailScreenState extends ConsumerState<ProductDetailScreen> {
  int _quantity = 1;
  int _galleryIndex = 0;

  @override
  Widget build(BuildContext context) {
    final detail = ref.watch(productDetailProvider(widget.slug));

    return Scaffold(
      body: detail.when(
        loading: () => const _DetailSkeleton(),
        error: (error, _) => Scaffold(
          appBar: AppBar(),
          body: ErrorView(
            error: error,
            onRetry: () =>
                ref.invalidate(productDetailProvider(widget.slug)),
          ),
        ),
        data: (product) => CustomScrollView(
          slivers: [
            SliverAppBar(
              expandedHeight: MediaQuery.sizeOf(context).width,
              pinned: true,
              flexibleSpace: FlexibleSpaceBar(
                background: _Gallery(
                  images: product.images,
                  emoji: product.emoji,
                  currentIndex: _galleryIndex,
                  onPageChanged: (index) =>
                      setState(() => _galleryIndex = index),
                ),
              ),
            ),
            SliverToBoxAdapter(child: _Body(product: product)),
            _RelatedSection(productId: product.id),
            const SliverToBoxAdapter(
              child: SizedBox(height: AppSpacing.xxxl),
            ),
          ],
        ),
      ),
      bottomNavigationBar: detail.hasValue
          ? _AddToCartBar(
              product: detail.requireValue,
              quantity: _quantity,
              onQuantityChanged: (value) => setState(() => _quantity = value),
            )
          : null,
    );
  }
}

class _Gallery extends StatelessWidget {
  const _Gallery({
    required this.images,
    required this.currentIndex,
    required this.onPageChanged,
    this.emoji,
  });

  final List<String> images;
  final String? emoji;
  final int currentIndex;
  final ValueChanged<int> onPageChanged;

  @override
  Widget build(BuildContext context) {
    if (images.isEmpty) {
      return AppNetworkImage(
        url: null,
        placeholderEmoji: emoji,
        fit: BoxFit.cover,
      );
    }

    return Stack(
      fit: StackFit.expand,
      children: [
        PageView.builder(
          itemCount: images.length,
          onPageChanged: onPageChanged,
          itemBuilder: (context, index) => AppNetworkImage(
            url: images[index],
            fit: BoxFit.cover,
            placeholderEmoji: emoji,
          ),
        ),
        if (images.length > 1)
          PositionedDirectional(
            bottom: AppSpacing.lg,
            start: 0,
            end: 0,
            child: Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                for (var i = 0; i < images.length; i++)
                  Container(
                    margin: const EdgeInsets.symmetric(
                      horizontal: AppSpacing.xxs,
                    ),
                    width: i == currentIndex ? 20 : 6,
                    height: 6,
                    decoration: BoxDecoration(
                      // The active dot is widened rather than recoloured:
                      // colour alone is unreliable over an uncontrolled
                      // product photo.
                      color: i == currentIndex
                          ? Colors.white
                          : Colors.white.withOpacity(0.5),
                      borderRadius: BorderRadius.circular(AppRadius.pill),
                    ),
                  ),
              ],
            ),
          ),
      ],
    );
  }
}

class _Body extends StatelessWidget {
  const _Body({required this.product});

  final ProductDetail product;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = context.l10n;

    return Padding(
      padding: const EdgeInsets.all(AppSpacing.lg),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          if (product.brandName != null)
            Text(
              product.brandName!,
              style: theme.textTheme.labelMedium?.copyWith(
                color: theme.colorScheme.primary,
              ),
            ),

          const SizedBox(height: AppSpacing.xs),

          Text(product.name, style: theme.textTheme.headlineSmall),

          const SizedBox(height: AppSpacing.md),

          PriceTag(
            price: product.price,
            oldPrice: product.oldPrice,
            discountPercent: product.discountPercent,
            size: PriceTagSize.large,
          ),

          const SizedBox(height: AppSpacing.md),

          _AvailabilityChip(availability: product.availability),

          if (product.shortDesc != null) ...[
            const SizedBox(height: AppSpacing.lg),
            Text(
              product.shortDesc!,
              style: theme.textTheme.bodyMedium?.copyWith(
                color: theme.textTheme.bodySmall?.color,
              ),
            ),
          ],

          if (product.features.isNotEmpty) ...[
            const SizedBox(height: AppSpacing.xl),
            Text(
              l10n.t('productFeatures'),
              style: theme.textTheme.titleMedium,
            ),
            const SizedBox(height: AppSpacing.sm),
            for (final feature in product.features)
              Padding(
                padding: const EdgeInsets.only(bottom: AppSpacing.xs),
                child: Row(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Icon(
                      Icons.check_circle,
                      size: 18,
                      color: AppColors.success,
                    ),
                    const SizedBox(width: AppSpacing.sm),
                    Expanded(
                      child: Text(
                        feature,
                        style: theme.textTheme.bodyMedium,
                      ),
                    ),
                  ],
                ),
              ),
          ],

          if (product.description != null) ...[
            const SizedBox(height: AppSpacing.xl),
            Text(
              l10n.t('productDescription'),
              style: theme.textTheme.titleMedium,
            ),
            const SizedBox(height: AppSpacing.sm),
            // Rendered as plain text, not HTML: the description is
            // admin-authored and putting it in a WebView would let a script
            // tag execute with the app's privileges.
            Text(product.description!, style: theme.textTheme.bodyMedium),
          ],

          if (product.specs.isNotEmpty) ...[
            const SizedBox(height: AppSpacing.xl),
            Text(l10n.t('productSpecs'), style: theme.textTheme.titleMedium),
            const SizedBox(height: AppSpacing.sm),
            Container(
              decoration: BoxDecoration(
                borderRadius: AppRadius.cardRadius,
                border: Border.all(color: theme.dividerColor),
              ),
              child: Column(
                children: [
                  for (var i = 0; i < product.specs.length; i++) ...[
                    if (i > 0) Divider(height: 1, color: theme.dividerColor),
                    Padding(
                      padding: const EdgeInsets.all(AppSpacing.md),
                      child: Row(
                        children: [
                          Expanded(
                            flex: 2,
                            child: Text(
                              product.specs[i].name,
                              style: theme.textTheme.bodySmall,
                            ),
                          ),
                          Expanded(
                            flex: 3,
                            child: Text(
                              product.specs[i].value,
                              style: theme.textTheme.bodyMedium,
                            ),
                          ),
                        ],
                      ),
                    ),
                  ],
                ],
              ),
            ),
          ],
        ],
      ),
    );
  }
}

class _AvailabilityChip extends StatelessWidget {
  const _AvailabilityChip({required this.availability});

  final ProductAvailability availability;

  @override
  Widget build(BuildContext context) {
    final l10n = context.l10n;
    final theme = Theme.of(context);

    final (label, color, icon) = switch (availability) {
      ProductAvailability.outOfStock => (
          l10n.t('productOutOfStock'),
          AppColors.danger,
          Icons.remove_shopping_cart_outlined,
        ),
      ProductAvailability.lowStock => (
          l10n.t('productLowStock'),
          AppColors.warning,
          Icons.warning_amber_rounded,
        ),
      ProductAvailability.inStock => (
          l10n.t('productInStock'),
          AppColors.success,
          Icons.check_circle_outline,
        ),
    };

    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.md,
        vertical: AppSpacing.sm,
      ),
      decoration: BoxDecoration(
        color: color.withOpacity(0.12),
        borderRadius: AppRadius.pillRadius,
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          Icon(icon, size: 16, color: color),
          const SizedBox(width: AppSpacing.xs),
          Text(
            label,
            style: theme.textTheme.labelMedium?.copyWith(color: color),
          ),
        ],
      ),
    );
  }
}

class _RelatedSection extends ConsumerWidget {
  const _RelatedSection({required this.productId});

  final int productId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final related = ref.watch(relatedProductsProvider(productId));
    final l10n = context.l10n;

    // Related products are supplementary — a failure or empty result renders
    // nothing rather than an error the user cannot act on.
    return related.maybeWhen(
      data: (products) => products.isEmpty
          ? const SliverToBoxAdapter(child: SizedBox.shrink())
          : SliverToBoxAdapter(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  SectionHeader(title: l10n.t('productRelated')),
                  HorizontalCardList(
                    itemCount: products.length,
                    itemBuilder: (context, index) {
                      final product = products[index];
                      return ProductCard(
                        product: product,
                        onTap: () => context.pushReplacement(
                          Routes.product(product.slug),
                        ),
                      );
                    },
                  ),
                ],
              ),
            ),
      orElse: () => const SliverToBoxAdapter(child: SizedBox.shrink()),
    );
  }
}

/// Sticky footer with a quantity stepper and the add-to-cart button.
class _AddToCartBar extends ConsumerWidget {
  const _AddToCartBar({
    required this.product,
    required this.quantity,
    required this.onQuantityChanged,
  });

  final ProductDetail product;
  final int quantity;
  final ValueChanged<int> onQuantityChanged;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final l10n = context.l10n;
    final outOfStock = product.availability == ProductAvailability.outOfStock;

    return Container(
      decoration: BoxDecoration(
        color: theme.cardColor,
        boxShadow: AppShadows.bottomBar,
      ),
      child: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: Row(
            children: [
              if (!outOfStock) ...[
                _QuantityStepper(
                  quantity: quantity,
                  onChanged: onQuantityChanged,
                ),
                const SizedBox(width: AppSpacing.md),
              ],
              Expanded(
                child: FilledButton.icon(
                  onPressed: outOfStock
                      ? null
                      : () => _add(context, ref),
                  icon: const Icon(Icons.add_shopping_cart, size: 20),
                  label: Text(
                    outOfStock
                        ? l10n.t('productOutOfStock')
                        : l10n.t('productAddToCart'),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  void _add(BuildContext context, WidgetRef ref) {
    ref.read(cartControllerProvider.notifier).addProduct(
          // The cart stores the list shape, so the detail is projected down
          // rather than the cart carrying a second product model.
          product.toListItem(),
          quantity: quantity,
        );

    final l10n = context.l10n;
    ScaffoldMessenger.of(context)
      ..hideCurrentSnackBar()
      ..showSnackBar(
        SnackBar(
          content: Text(l10n.t('productAddedToCart')),
          action: SnackBarAction(
            label: l10n.t('navCart'),
            onPressed: () => context.go(Routes.cart),
          ),
        ),
      );
  }
}

class _QuantityStepper extends StatelessWidget {
  const _QuantityStepper({required this.quantity, required this.onChanged});

  final int quantity;
  final ValueChanged<int> onChanged;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      decoration: BoxDecoration(
        borderRadius: AppRadius.fieldRadius,
        border: Border.all(color: theme.dividerColor),
      ),
      child: Row(
        children: [
          IconButton(
            // Disabled at 1: the stepper on this screen selects how many to
            // add, so zero is not a meaningful value here.
            onPressed: quantity > 1 ? () => onChanged(quantity - 1) : null,
            icon: const Icon(Icons.remove, size: 18),
            visualDensity: VisualDensity.compact,
          ),
          SizedBox(
            width: 24,
            child: Text(
              '$quantity',
              textAlign: TextAlign.center,
              style: theme.textTheme.titleSmall,
            ),
          ),
          IconButton(
            onPressed: () => onChanged(quantity + 1),
            icon: const Icon(Icons.add, size: 18),
            visualDensity: VisualDensity.compact,
          ),
        ],
      ),
    );
  }
}

class _DetailSkeleton extends StatelessWidget {
  const _DetailSkeleton();

  @override
  Widget build(BuildContext context) {
    final width = MediaQuery.sizeOf(context).width;

    return ListView(
      children: [
        ShimmerBox(height: width, borderRadius: BorderRadius.zero),
        const Padding(
          padding: EdgeInsets.all(AppSpacing.lg),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              ShimmerBox(height: 14, width: 80),
              SizedBox(height: AppSpacing.sm),
              ShimmerBox(height: 24),
              SizedBox(height: AppSpacing.xs),
              ShimmerBox(height: 24, width: 200),
              SizedBox(height: AppSpacing.lg),
              ShimmerBox(height: 28, width: 120),
              SizedBox(height: AppSpacing.lg),
              ShimmerBox(height: 32, width: 100),
            ],
          ),
        ),
      ],
    );
  }
}
