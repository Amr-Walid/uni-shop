import 'package:flutter/material.dart';

import '../l10n/app_localizations.dart';
import '../theme/app_spacing.dart';

/// "منتجات مميزة    عرض الكل ›" row above a home-screen section.
class SectionHeader extends StatelessWidget {
  const SectionHeader({
    super.key,
    required this.title,
    this.subtitle,
    this.onSeeAll,
    this.padding = AppSpacing.page,
  });

  final String title;
  final String? subtitle;
  final VoidCallback? onSeeAll;
  final EdgeInsets padding;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = context.l10n;
    final isRtl = Directionality.of(context) == TextDirection.rtl;

    return Padding(
      padding: padding,
      child: Row(
        children: [
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(title, style: theme.textTheme.titleLarge),
                if (subtitle != null) ...[
                  const SizedBox(height: AppSpacing.xxs),
                  Text(
                    subtitle!,
                    style: theme.textTheme.bodySmall,
                    maxLines: 1,
                    overflow: TextOverflow.ellipsis,
                  ),
                ],
              ],
            ),
          ),
          if (onSeeAll != null)
            TextButton(
              onPressed: onSeeAll,
              child: Row(
                children: [
                  Text(l10n.t('seeAll')),
                  // The chevron must point in the direction of travel, which
                  // is leftwards in Arabic.
                  Icon(
                    isRtl ? Icons.chevron_left : Icons.chevron_right,
                    size: 18,
                  ),
                ],
              ),
            ),
        ],
      ),
    );
  }
}

/// Horizontally scrolling row of cards, used by every home section.
///
/// Extracted because the padding, spacing and clipping behaviour must match
/// across sections — a carousel whose first card is flush to the edge while
/// another is inset looks unfinished.
class HorizontalCardList extends StatelessWidget {
  const HorizontalCardList({
    super.key,
    required this.itemCount,
    required this.itemBuilder,
    this.itemWidth = 160,
    this.height,
    this.padding = AppSpacing.page,
  });

  final int itemCount;
  final Widget Function(BuildContext context, int index) itemBuilder;
  final double itemWidth;
  final double? height;
  final EdgeInsets padding;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      // Square image plus the card body's fixed height, so the row is exactly
      // as tall as its content needs.
      //
      // This used to be `itemWidth / 0.62`, which tied the height to the
      // width. That hurt this row MORE than the catalogue grid: a carousel
      // card is 160dp wide against the grid's ~183.5dp, so the same ratio left
      // it 25dp short of what the body needs and cut the second line of every
      // long product name — the clipping visible on the home screen.
      height: height ?? AppSizes.productCardHeight(itemWidth),
      child: ListView.separated(
        scrollDirection: Axis.horizontal,
        padding: padding,
        itemCount: itemCount,
        // clipBehavior none lets the card's shadow render outside the row
        // instead of being cut at its bounds.
        clipBehavior: Clip.none,
        separatorBuilder: (_, __) => const SizedBox(width: AppSpacing.md),
        itemBuilder: (context, index) => SizedBox(
          width: itemWidth,
          child: itemBuilder(context, index),
        ),
      ),
    );
  }
}
