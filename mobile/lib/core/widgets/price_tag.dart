import 'package:decimal/decimal.dart';
import 'package:flutter/material.dart';

import '../l10n/app_localizations.dart';
import '../theme/app_colors.dart';
import '../theme/app_spacing.dart';
import '../theme/app_typography.dart';
import '../utils/formatters.dart';

/// Price display with optional struck-through original and a discount badge.
///
/// Centralised because a price appears in six places (card, detail, cart line,
/// cart total, checkout summary, order line) and they must agree exactly —
/// including the currency position, which differs between Arabic and English.
class PriceTag extends StatelessWidget {
  const PriceTag({
    super.key,
    required this.price,
    this.oldPrice,
    this.discountPercent,
    this.size = PriceTagSize.medium,
    this.alignment = CrossAxisAlignment.start,
    this.showBadge = true,
  });

  final Decimal price;
  final Decimal? oldPrice;
  final int? discountPercent;
  final PriceTagSize size;
  final CrossAxisAlignment alignment;
  final bool showBadge;

  /// A struck-through price is only shown when it is genuinely higher and a
  /// discount was computed — never on a stale value left by an admin.
  bool get _hasDiscount =>
      oldPrice != null && oldPrice! > price && (discountPercent ?? 0) > 0;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = context.l10n;

    final priceStyle = AppTypography.price(
      switch (size) {
        PriceTagSize.small => theme.textTheme.titleSmall,
        PriceTagSize.medium => theme.textTheme.titleMedium,
        PriceTagSize.large => theme.textTheme.headlineSmall,
      },
    ).copyWith(color: theme.colorScheme.primary);

    final oldStyle = AppTypography.price(
      size == PriceTagSize.large
          ? theme.textTheme.bodyLarge
          : theme.textTheme.bodySmall,
    ).copyWith(
      color: theme.textTheme.bodySmall?.color?.withOpacity(0.7),
      decoration: TextDecoration.lineThrough,
      // A thinner strike reads as a strike rather than as an underline at
      // small sizes.
      decorationThickness: 1.5,
      fontWeight: FontWeight.w500,
    );

    final current = Text(
      Formatters.price(price, lang: l10n.languageCode),
      style: priceStyle,
      maxLines: 1,
      overflow: TextOverflow.ellipsis,
    );

    if (!_hasDiscount) return current;

    return Column(
      crossAxisAlignment: alignment,
      mainAxisSize: MainAxisSize.min,
      children: [
        current,
        const SizedBox(height: AppSpacing.xxs),
        // Wrap, not Row: at a narrow card width the old price plus badge
        // overflows, and Wrap moves the badge to a second line instead of
        // clipping it.
        Wrap(
          spacing: AppSpacing.sm,
          runSpacing: AppSpacing.xxs,
          crossAxisAlignment: WrapCrossAlignment.center,
          children: [
            Text(
              Formatters.price(
                oldPrice,
                lang: l10n.languageCode,
                withSymbol: false,
              ),
              style: oldStyle,
            ),
            if (showBadge)
              DiscountBadge(
                percent: discountPercent!,
                compact: size == PriceTagSize.small,
              ),
          ],
        ),
      ],
    );
  }
}

enum PriceTagSize { small, medium, large }

/// "خصم ٢٥٪" / "25% off" chip.
class DiscountBadge extends StatelessWidget {
  const DiscountBadge({
    super.key,
    required this.percent,
    this.compact = false,
  });

  final int percent;
  final bool compact;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = context.l10n;

    return Container(
      padding: EdgeInsets.symmetric(
        horizontal: compact ? AppSpacing.xs : AppSpacing.sm,
        vertical: compact ? 1 : AppSpacing.xxs,
      ),
      decoration: BoxDecoration(
        color: AppColors.sale,
        borderRadius: BorderRadius.circular(AppRadius.xs),
      ),
      child: Text(
        compact
            // Bare "-25%" fits a grid card where the full phrase would not.
            ? '-$percent%'
            : l10n.tf('productDiscountBadge', {'n': percent}),
        style: (compact
                ? theme.textTheme.labelSmall
                : theme.textTheme.labelMedium)
            ?.copyWith(
          color: Colors.white,
          fontWeight: FontWeight.w700,
          // Digits are Western in both languages, so force LTR inside the
          // badge or the minus sign jumps to the wrong side in an RTL layout.
          fontFeatures: const [FontFeature.tabularFigures()],
        ),
        textDirection: TextDirection.ltr,
      ),
    );
  }
}
