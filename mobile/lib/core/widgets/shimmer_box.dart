import 'package:flutter/material.dart';
import 'package:shimmer/shimmer.dart';

import '../theme/app_colors.dart';
import '../theme/app_spacing.dart';

/// Skeleton placeholder.
///
/// Skeletons are used instead of a centred spinner for first loads because
/// they preserve the layout: the content appears in place rather than the
/// page jumping when the spinner is replaced.
class ShimmerBox extends StatelessWidget {
  const ShimmerBox({
    super.key,
    this.width,
    this.height = 16,
    this.borderRadius,
  });

  final double? width;
  final double height;
  final BorderRadius? borderRadius;

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return Shimmer.fromColors(
      baseColor: isDark ? AppColors.shimmerBaseDark : AppColors.shimmerBase,
      highlightColor:
          isDark ? AppColors.shimmerHighlightDark : AppColors.shimmerHighlight,
      // The sweep must run against the reading direction, otherwise in RTL it
      // travels the "wrong" way and reads as a glitch.
      direction: Directionality.of(context) == TextDirection.rtl
          ? ShimmerDirection.rtl
          : ShimmerDirection.ltr,
      child: Container(
        width: width,
        height: height,
        decoration: BoxDecoration(
          color: isDark ? AppColors.shimmerBaseDark : AppColors.shimmerBase,
          borderRadius: borderRadius ??
              BorderRadius.circular(AppRadius.xs),
        ),
      ),
    );
  }
}

/// Skeleton shaped like a [ProductCard], used to fill a loading grid.
///
/// The proportions intentionally match the real card so the grid does not
/// reflow when data arrives.
class ProductCardSkeleton extends StatelessWidget {
  const ProductCardSkeleton({super.key});

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      decoration: BoxDecoration(
        color: theme.cardColor,
        borderRadius: AppRadius.cardRadius,
        border: Border.all(color: theme.dividerColor),
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const AspectRatio(
            aspectRatio: AppSizes.productImageAspect,
            child: ShimmerBox(
              height: double.infinity,
              borderRadius: BorderRadius.vertical(
                top: Radius.circular(AppRadius.md),
              ),
            ),
          ),
          Padding(
            padding: const EdgeInsets.all(AppSpacing.sm),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                const ShimmerBox(height: 12, width: 60),
                const SizedBox(height: AppSpacing.sm),
                const ShimmerBox(height: 14),
                const SizedBox(height: AppSpacing.xs),
                const ShimmerBox(height: 14, width: 120),
                const SizedBox(height: AppSpacing.md),
                const ShimmerBox(height: 18, width: 90),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

/// Grid of product skeletons for a catalog first load.
class ProductGridSkeleton extends StatelessWidget {
  const ProductGridSkeleton({
    super.key,
    this.itemCount = 6,
    this.crossAxisCount = 2,
    this.padding = AppSpacing.page,
  });

  final int itemCount;
  final int crossAxisCount;
  final EdgeInsets padding;

  @override
  Widget build(BuildContext context) {
    return GridView.builder(
      padding: padding,
      // The skeleton is never interactive and must not steal the scroll
      // gesture from a parent CustomScrollView.
      physics: const NeverScrollableScrollPhysics(),
      shrinkWrap: true,
      itemCount: itemCount,
      // Must size identically to the real grid (see
      // AppSizes.productCardHeight); a skeleton of a different height makes
      // the layout visibly jump when the real cards arrive.
      gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
        crossAxisCount: crossAxisCount,
        crossAxisSpacing: AppSpacing.md,
        mainAxisSpacing: AppSpacing.md,
        mainAxisExtent: AppSizes.productCardHeight(
          (MediaQuery.sizeOf(context).width -
                  AppSpacing.lg * 2 -
                  AppSpacing.md * (crossAxisCount - 1)) /
              crossAxisCount,
        ),
      ),
      itemBuilder: (_, __) => const ProductCardSkeleton(),
    );
  }
}

/// Horizontal skeleton row for a home-screen carousel.
class ProductCarouselSkeleton extends StatelessWidget {
  const ProductCarouselSkeleton({
    super.key,
    this.itemCount = 3,
    this.itemWidth = 160,
  });

  final int itemCount;
  final double itemWidth;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      // Same height as the real carousel so nothing shifts on load.
      height: AppSizes.productCardHeight(itemWidth),
      child: ListView.separated(
        scrollDirection: Axis.horizontal,
        physics: const NeverScrollableScrollPhysics(),
        padding: AppSpacing.page,
        itemCount: itemCount,
        separatorBuilder: (_, __) => const SizedBox(width: AppSpacing.md),
        itemBuilder: (_, __) => SizedBox(
          width: itemWidth,
          child: const ProductCardSkeleton(),
        ),
      ),
    );
  }
}

/// Skeleton for a list of rows (orders, cart lines, addresses).
class ListSkeleton extends StatelessWidget {
  const ListSkeleton({
    super.key,
    this.itemCount = 4,
    this.itemHeight = 88,
  });

  final int itemCount;
  final double itemHeight;

  @override
  Widget build(BuildContext context) {
    return ListView.separated(
      padding: AppSpacing.page,
      physics: const NeverScrollableScrollPhysics(),
      shrinkWrap: true,
      itemCount: itemCount,
      separatorBuilder: (_, __) => const SizedBox(height: AppSpacing.md),
      itemBuilder: (_, __) => ShimmerBox(
        height: itemHeight,
        borderRadius: AppRadius.cardRadius,
      ),
    );
  }
}
