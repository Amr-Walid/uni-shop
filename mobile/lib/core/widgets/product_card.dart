import 'package:flutter/material.dart';

import '../../features/catalog/domain/product.dart';
import '../l10n/app_localizations.dart';
import '../theme/app_colors.dart';
import '../theme/app_spacing.dart';
import 'app_network_image.dart';
import 'press_scale.dart';
import 'price_tag.dart';

/// Product tile used in the catalog grid and every home carousel.
///
/// One widget for both contexts on purpose: a separate carousel card would
/// inevitably drift from the grid card, and shoppers compare prices across the
/// two. Width is controlled by the parent (grid delegate or SizedBox) so the
/// card itself never assumes a layout.
class ProductCard extends StatelessWidget {
  const ProductCard({
    super.key,
    required this.product,
    this.onTap,
    this.onAddToCart,
    this.onToggleWishlist,
    this.isInWishlist = false,
    this.quantityInCart = 0,
  });

  final Product product;
  final VoidCallback? onTap;
  final VoidCallback? onAddToCart;
  final VoidCallback? onToggleWishlist;
  final bool isInWishlist;

  /// Shown as a badge on the add button so the shopper can see what is already
  /// in the cart without opening it.
  final int quantityInCart;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = context.l10n;
    final outOfStock = product.availability == ProductAvailability.outOfStock;

    return PressScale(
      child: Material(
        color: theme.cardColor,
        borderRadius: AppRadius.cardRadius,
        clipBehavior: Clip.antiAlias,
        child: InkWell(
          onTap: onTap,
          child: Container(
            decoration: BoxDecoration(
              borderRadius: AppRadius.cardRadius,
              border: Border.all(color: theme.dividerColor),
            ),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                _buildImage(context, outOfStock),
                Expanded(child: _buildBody(context, theme, l10n, outOfStock)),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _buildImage(BuildContext context, bool outOfStock) {
    return Stack(
      children: [
        // Greyed out when unavailable so the state is obvious at a glance in a
        // grid, not only from the label.
        Opacity(
          opacity: outOfStock ? 0.45 : 1,
          child: AspectRatio(
            aspectRatio: AppSizes.productImageAspect,
            child: DecoratedBox(
              // The website's `.product-card .media` treatment: a soft orange
              // radial wash at 70%/30% over a vertical violet-white gradient.
              // Product photos are transparent PNGs and emoji placeholders are
              // glyphs, so both sit ON this wash — it is the single most
              // recognisable part of the storefront's product styling and its
              // absence is why the app's tiles looked flat and generic.
              //
              // Two gradients would need two layers, so the radial is baked in
              // as the decoration and the linear sits beneath it: cheaper than
              // stacking two DecoratedBoxes, and gradients are shader-only
              // work with no per-frame cost.
              decoration: const BoxDecoration(
                gradient: LinearGradient(
                  begin: Alignment.topCenter,
                  end: Alignment.bottomCenter,
                  colors: [Color(0xFFF8FAFF), Color(0xFFEEF2FF)],
                ),
              ),
              child: DecoratedBox(
                decoration: const BoxDecoration(
                  gradient: RadialGradient(
                    // 70% across, 30% down, expressed in Alignment's -1..1
                    // space.
                    center: Alignment(0.4, -0.4),
                    radius: 0.9,
                    colors: [Color(0x14FF7A00), Color(0x00FF7A00)],
                  ),
                ),
                child: AppNetworkImage(
                  url: product.imageUrl,
                  placeholderEmoji: product.emoji,
                  fit: BoxFit.cover,
                ),
              ),
            ),
          ),
        ),

        // Discount badge, leading edge — flips side automatically in RTL
        // because PositionedDirectional resolves against text direction.
        if (product.isOnSale)
          PositionedDirectional(
            top: AppSpacing.sm,
            start: AppSpacing.sm,
            child: DiscountBadge(percent: product.discountPercent!, compact: true),
          ),

        if (onToggleWishlist != null)
          PositionedDirectional(
            top: AppSpacing.xs,
            end: AppSpacing.xs,
            child: _WishlistButton(
              isActive: isInWishlist,
              onPressed: onToggleWishlist!,
            ),
          ),

        if (product.badge != null && !product.isOnSale)
          PositionedDirectional(
            top: AppSpacing.sm,
            start: AppSpacing.sm,
            child: _Badge(
              label: product.badge!,
              color: AppColors.primary,
            ),
          ),
      ],
    );
  }

  Widget _buildBody(
    BuildContext context,
    ThemeData theme,
    AppLocalizations l10n,
    bool outOfStock,
  ) {
    return Padding(
      padding: const EdgeInsets.all(AppSpacing.sm),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          if (product.brandName != null)
            Text(
              product.brandName!,
              style: theme.textTheme.labelSmall?.copyWith(
                color: theme.textTheme.bodySmall?.color,
              ),
              maxLines: 1,
              overflow: TextOverflow.ellipsis,
            ),

          const SizedBox(height: AppSpacing.xxs),

          // Up to two lines: Arabic product names are long and a single line
          // truncates almost all of them, while three lines makes card heights
          // inconsistent across a row.
          //
          // This is the ONLY elastic child, and there is deliberately no
          // `Spacer` after it. An earlier version had both, and since
          // `Flexible` and `Spacer` each default to `flex: 1` they split the
          // leftover space evenly — the name got half of what two lines need
          // and was clipped through the middle of its glyphs, running into the
          // availability row. That is the collision visible on device.
          //
          // Replacing it with a fixed-height `SizedBox` was also wrong: a
          // reservation ignores whether the space exists, so short cards
          // overflowed by 3-8px instead.
          //
          // Giving the name the single flexible slot resolves both. It takes
          // what two lines need when the card is tall enough, shrinks and
          // ellipsizes when it is not, and can never overflow. The name block
          // absorbing the slack (rather than a Spacer) also keeps the price
          // row pinned to the bottom, so prices still align across a row.
          Expanded(
            child: Align(
              alignment: AlignmentDirectional.topStart,
              child: Text(
                product.name,
                style: theme.textTheme.titleSmall,
                maxLines: 2,
                overflow: TextOverflow.ellipsis,
              ),
            ),
          ),

          _buildAvailability(theme, l10n),

          const SizedBox(height: AppSpacing.xs),

          Row(
            crossAxisAlignment: CrossAxisAlignment.end,
            children: [
              Expanded(
                child: PriceTag(
                  price: product.price,
                  oldPrice: product.oldPrice,
                  discountPercent: product.discountPercent,
                  size: PriceTagSize.small,
                  // The badge is already shown over the image; repeating it
                  // here would crowd a narrow card.
                  showBadge: false,
                ),
              ),
              if (onAddToCart != null)
                _AddButton(
                  // Disabled rather than hidden: a missing button reads as a
                  // rendering bug, a disabled one communicates the reason.
                  onPressed: outOfStock ? null : onAddToCart,
                  quantity: quantityInCart,
                ),
            ],
          ),
        ],
      ),
    );
  }

  Widget _buildAvailability(ThemeData theme, AppLocalizations l10n) {
    final (label, color) = switch (product.availability) {
      ProductAvailability.outOfStock => (
          l10n.t('productOutOfStock'),
          AppColors.danger,
        ),
      ProductAvailability.lowStock => (
          l10n.t('productLowStock'),
          AppColors.warning,
        ),
      ProductAvailability.inStock => (
          l10n.t('productInStock'),
          AppColors.success,
        ),
    };

    return Row(
      children: [
        Container(
          width: 6,
          height: 6,
          decoration: BoxDecoration(color: color, shape: BoxShape.circle),
        ),
        const SizedBox(width: AppSpacing.xs),
        Text(
          label,
          style: theme.textTheme.labelSmall?.copyWith(color: color),
        ),
      ],
    );
  }
}

class _AddButton extends StatelessWidget {
  const _AddButton({required this.onPressed, this.quantity = 0});

  final VoidCallback? onPressed;
  final int quantity;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final enabled = onPressed != null;

    return SizedBox(
      width: 36,
      height: 36,
      child: Material(
        color: enabled
            ? theme.colorScheme.primary
            : theme.disabledColor.withOpacity(0.15),
        borderRadius: BorderRadius.circular(AppRadius.sm),
        child: InkWell(
          onTap: onPressed,
          borderRadius: BorderRadius.circular(AppRadius.sm),
          child: Center(
            child: quantity > 0
                ? Text(
                    '$quantity',
                    style: theme.textTheme.labelMedium?.copyWith(
                      color: theme.colorScheme.onPrimary,
                      fontWeight: FontWeight.w700,
                    ),
                  )
                : Icon(
                    Icons.add_shopping_cart_outlined,
                    size: 18,
                    color: enabled
                        ? theme.colorScheme.onPrimary
                        : theme.disabledColor,
                  ),
          ),
        ),
      ),
    );
  }
}

class _WishlistButton extends StatelessWidget {
  const _WishlistButton({required this.isActive, required this.onPressed});

  final bool isActive;
  final VoidCallback onPressed;

  @override
  Widget build(BuildContext context) {
    return Material(
      // Translucent chip: the heart sits over an uncontrolled product photo
      // and needs its own contrast to stay visible on a light image.
      color: Colors.black.withOpacity(0.28),
      shape: const CircleBorder(),
      child: InkWell(
        onTap: onPressed,
        customBorder: const CircleBorder(),
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xs),
          child: Icon(
            isActive ? Icons.favorite : Icons.favorite_border,
            size: 18,
            color: isActive ? AppColors.danger : Colors.white,
          ),
        ),
      ),
    );
  }
}

class _Badge extends StatelessWidget {
  const _Badge({required this.label, required this.color});

  final String label;
  final Color color;

  @override
  Widget build(BuildContext context) {
    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.sm,
        vertical: 1,
      ),
      decoration: BoxDecoration(
        color: color,
        borderRadius: BorderRadius.circular(AppRadius.xs),
      ),
      child: Text(
        label,
        style: Theme.of(context).textTheme.labelSmall?.copyWith(
              color: Colors.white,
              fontWeight: FontWeight.w700,
            ),
        maxLines: 1,
      ),
    );
  }
}
