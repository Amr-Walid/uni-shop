import 'package:decimal/decimal.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/l10n/app_localizations.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_spacing.dart';
import '../../../core/utils/formatters.dart';
import '../../../core/widgets/app_network_image.dart';
import '../../../core/widgets/state_views.dart';
import '../../../routing/routes.dart';
import '../domain/cart_item.dart';
import 'cart_controller.dart';

class CartScreen extends ConsumerWidget {
  const CartScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = context.l10n;
    final cart = ref.watch(cartControllerProvider);

    return Scaffold(
      appBar: AppBar(
        title: Text(l10n.t('cartTitle')),
        actions: [
          if (cart.isNotEmpty)
            IconButton(
              onPressed: () => _confirmClear(context, ref),
              icon: const Icon(Icons.delete_outline),
              tooltip: l10n.t('clear'),
            ),
        ],
      ),
      body: cart.isEmpty
          ? EmptyState(
              emoji: '🛒',
              title: l10n.t('cartEmpty'),
              message: l10n.t('cartEmptyHint'),
              actionLabel: l10n.t('cartStartShopping'),
              onAction: () => context.go(Routes.catalog),
            )
          : ListView(
              padding: AppSpacing.pageWithBottomNav,
              children: [
                if (cart.remainingForFreeShipping != null)
                  _FreeShippingNudge(
                    remaining: cart.remainingForFreeShipping!,
                    threshold: cart.freeShippingAbove,
                    subTotal: cart.subTotal,
                  ),
                for (final item in cart.items)
                  Padding(
                    padding: const EdgeInsets.only(bottom: AppSpacing.md),
                    child: _CartLine(item: item),
                  ),
                const SizedBox(height: AppSpacing.sm),
                _Totals(cart: cart),
              ],
            ),
      bottomNavigationBar:
          cart.isEmpty ? null : _CheckoutBar(cart: cart),
    );
  }

  Future<void> _confirmClear(BuildContext context, WidgetRef ref) async {
    final l10n = context.l10n;

    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(l10n.t('cartTitle')),
        content: Text(l10n.t('cartClearConfirm')),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: Text(l10n.t('cancel')),
          ),
          FilledButton(
            onPressed: () => Navigator.of(context).pop(true),
            child: Text(l10n.t('delete')),
          ),
        ],
      ),
    );

    if (confirmed == true) {
      ref.read(cartControllerProvider.notifier).clear();
    }
  }
}

/// "أضف ٥٠٠ ج.م للحصول على شحن مجاني" with a progress bar.
///
/// Shown because a free-shipping threshold only changes behaviour if the
/// shopper knows how close they are to it.
class _FreeShippingNudge extends StatelessWidget {
  const _FreeShippingNudge({
    required this.remaining,
    required this.threshold,
    required this.subTotal,
  });

  final Decimal remaining;
  final Decimal threshold;
  final Decimal subTotal;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = context.l10n;

    final progress = threshold <= Decimal.zero
        ? 0.0
        : (subTotal / threshold).toDouble().clamp(0.0, 1.0);

    return Container(
      margin: const EdgeInsets.only(bottom: AppSpacing.lg),
      padding: const EdgeInsets.all(AppSpacing.md),
      decoration: BoxDecoration(
        color: AppColors.primarySurface,
        borderRadius: AppRadius.cardRadius,
      ),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Row(
            children: [
              const Icon(
                Icons.local_shipping_outlined,
                size: 18,
                color: AppColors.primaryDark,
              ),
              const SizedBox(width: AppSpacing.sm),
              Expanded(
                child: Text(
                  l10n.tf('cartFreeShippingHint', {
                    'amount': Formatters.price(
                      remaining,
                      lang: l10n.languageCode,
                    ),
                  }),
                  style: theme.textTheme.labelMedium?.copyWith(
                    color: AppColors.primaryDark,
                  ),
                ),
              ),
            ],
          ),
          const SizedBox(height: AppSpacing.sm),
          ClipRRect(
            borderRadius: BorderRadius.circular(AppRadius.pill),
            child: LinearProgressIndicator(
              value: progress,
              backgroundColor: Colors.white,
              minHeight: 5,
            ),
          ),
        ],
      ),
    );
  }
}

class _CartLine extends ConsumerWidget {
  const _CartLine({required this.item});

  final CartItem item;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final l10n = context.l10n;

    return Dismissible(
      key: ValueKey('cart-${item.productId}'),
      // End-to-start only: a two-way swipe on a row that also contains a
      // horizontal stepper makes accidental deletions likely.
      direction: DismissDirection.endToStart,
      background: Container(
        alignment: AlignmentDirectional.centerEnd,
        padding: const EdgeInsets.symmetric(horizontal: AppSpacing.lg),
        decoration: BoxDecoration(
          color: AppColors.danger,
          borderRadius: AppRadius.cardRadius,
        ),
        child: const Icon(Icons.delete_outline, color: Colors.white),
      ),
      onDismissed: (_) {
        ref.read(cartControllerProvider.notifier).removeProduct(item.productId);
        ScaffoldMessenger.of(context)
          ..hideCurrentSnackBar()
          ..showSnackBar(
            SnackBar(content: Text(l10n.t('cartItemRemoved'))),
          );
      },
      child: Container(
        padding: const EdgeInsets.all(AppSpacing.sm),
        decoration: BoxDecoration(
          color: theme.cardColor,
          borderRadius: AppRadius.cardRadius,
          border: Border.all(color: theme.dividerColor),
        ),
        child: Row(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            SizedBox(
              width: 72,
              child: InkWell(
                onTap: item.slug == null
                    ? null
                    : () => context.push(Routes.product(item.slug!)),
                child: AppNetworkImage(
                  url: item.imageUrl,
                  width: 72,
                  height: 72,
                  placeholderEmoji: item.emoji,
                  borderRadius: BorderRadius.circular(AppRadius.sm),
                ),
              ),
            ),
            const SizedBox(width: AppSpacing.md),
            Expanded(
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  if (item.brandName != null)
                    Text(
                      item.brandName!,
                      style: theme.textTheme.labelSmall?.copyWith(
                        color: theme.textTheme.bodySmall?.color,
                      ),
                    ),
                  Text(
                    item.productName,
                    style: theme.textTheme.titleSmall,
                    maxLines: 2,
                    overflow: TextOverflow.ellipsis,
                  ),
                  const SizedBox(height: AppSpacing.sm),
                  Row(
                    children: [
                      _LineStepper(item: item),
                      const Spacer(),
                      Text(
                        Formatters.price(
                          item.subTotal,
                          lang: l10n.languageCode,
                        ),
                        style: theme.textTheme.titleSmall?.copyWith(
                          color: theme.colorScheme.primary,
                        ),
                      ),
                    ],
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class _LineStepper extends ConsumerWidget {
  const _LineStepper({required this.item});

  final CartItem item;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final controller = ref.read(cartControllerProvider.notifier);

    return Container(
      decoration: BoxDecoration(
        borderRadius: AppRadius.pillRadius,
        border: Border.all(color: theme.dividerColor),
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          InkWell(
            onTap: () => controller.decrement(item.productId),
            customBorder: const CircleBorder(),
            child: Padding(
              padding: const EdgeInsets.all(AppSpacing.xs),
              child: Icon(
                // At quantity 1 the next decrement removes the line, so the
                // icon becomes a bin to make that outcome explicit.
                item.quantity > 1 ? Icons.remove : Icons.delete_outline,
                size: 16,
              ),
            ),
          ),
          Padding(
            padding: const EdgeInsets.symmetric(horizontal: AppSpacing.sm),
            child: Text(
              '${item.quantity}',
              style: theme.textTheme.labelLarge,
            ),
          ),
          InkWell(
            onTap: () => controller.increment(item.productId),
            customBorder: const CircleBorder(),
            child: const Padding(
              padding: EdgeInsets.all(AppSpacing.xs),
              child: Icon(Icons.add, size: 16),
            ),
          ),
        ],
      ),
    );
  }
}

class _Totals extends StatelessWidget {
  const _Totals({required this.cart});

  final Cart cart;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = context.l10n;
    final lang = l10n.languageCode;

    return Container(
      padding: const EdgeInsets.all(AppSpacing.lg),
      decoration: BoxDecoration(
        color: theme.cardColor,
        borderRadius: AppRadius.cardRadius,
        border: Border.all(color: theme.dividerColor),
      ),
      child: Column(
        children: [
          _row(
            theme,
            l10n.t('cartSubtotal'),
            Formatters.price(cart.subTotal, lang: lang),
          ),
          const SizedBox(height: AppSpacing.sm),
          _row(
            theme,
            l10n.t('cartShipping'),
            cart.hasFreeShipping
                ? l10n.t('cartShippingFree')
                : Formatters.price(cart.shippingFee, lang: lang),
            valueColor: cart.hasFreeShipping ? AppColors.success : null,
          ),
          const Padding(
            padding: EdgeInsets.symmetric(vertical: AppSpacing.md),
            child: Divider(height: 1),
          ),
          _row(
            theme,
            l10n.t('cartTotal'),
            Formatters.price(cart.total, lang: lang),
            isTotal: true,
          ),
        ],
      ),
    );
  }

  Widget _row(
    ThemeData theme,
    String label,
    String value, {
    bool isTotal = false,
    Color? valueColor,
  }) {
    return Row(
      children: [
        Expanded(
          child: Text(
            label,
            style: isTotal
                ? theme.textTheme.titleMedium
                : theme.textTheme.bodyMedium,
          ),
        ),
        Text(
          value,
          style: (isTotal
                  ? theme.textTheme.titleMedium
                  : theme.textTheme.bodyMedium)
              ?.copyWith(
            color: valueColor ??
                (isTotal ? theme.colorScheme.primary : null),
            fontWeight: isTotal ? FontWeight.w700 : null,
          ),
        ),
      ],
    );
  }
}

class _CheckoutBar extends StatelessWidget {
  const _CheckoutBar({required this.cart});

  final Cart cart;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = context.l10n;

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
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                mainAxisSize: MainAxisSize.min,
                children: [
                  Text(
                    l10n.t('cartTotal'),
                    style: theme.textTheme.labelSmall,
                  ),
                  Text(
                    Formatters.price(cart.total, lang: l10n.languageCode),
                    style: theme.textTheme.titleMedium?.copyWith(
                      color: theme.colorScheme.primary,
                    ),
                  ),
                ],
              ),
              const SizedBox(width: AppSpacing.lg),
              Expanded(
                child: FilledButton(
                  onPressed: () => context.push(Routes.checkout),
                  child: Text(l10n.t('cartCheckout')),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
