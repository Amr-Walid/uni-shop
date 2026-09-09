import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/l10n/app_localizations.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_spacing.dart';
import '../../../core/utils/formatters.dart';
import '../../../core/widgets/app_network_image.dart';
import '../../../core/widgets/shimmer_box.dart';
import '../../../core/widgets/state_views.dart';
import '../../../routing/routes.dart';
import '../domain/order.dart';
import 'orders_controller.dart';
import 'orders_screen.dart' show OrderStatusChip;

class OrderDetailScreen extends ConsumerWidget {
  const OrderDetailScreen({super.key, required this.orderId});

  final int orderId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = context.l10n;
    final order = ref.watch(orderDetailProvider(orderId));

    return Scaffold(
      appBar: AppBar(title: Text(l10n.t('orderDetailTitle'))),
      body: order.when(
        loading: () => const ListSkeleton(itemCount: 3, itemHeight: 120),
        error: (error, _) => ErrorView(
          error: error,
          onRetry: () => ref.invalidate(orderDetailProvider(orderId)),
        ),
        data: (data) => OrderDetailView(order: data),
      ),
    );
  }
}

/// Order detail body.
///
/// Shared with the guest tracking screen: the API returns the same
/// `OrderDetailDto` from `GET /orders/{id}` and `GET /orders/track/{number}`,
/// so one widget serves both and they cannot drift apart.
class OrderDetailView extends ConsumerWidget {
  const OrderDetailView({super.key, required this.order, this.canCancel});

  final OrderDetail order;

  /// Overrides the order's own flag. Guest tracking passes false: cancelling
  /// is an account action, and the phone-digits factor is not strong enough to
  /// authorise destroying an order.
  final bool? canCancel;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final l10n = context.l10n;
    final lang = l10n.languageCode;
    final showCancel = canCancel ?? order.canCancel;

    return ListView(
      padding: const EdgeInsets.all(AppSpacing.lg),
      children: [
        // ── Header ────────────────────────────────────────────────────────
        Container(
          padding: const EdgeInsets.all(AppSpacing.lg),
          decoration: BoxDecoration(
            color: theme.cardColor,
            borderRadius: AppRadius.cardRadius,
            border: Border.all(color: theme.dividerColor),
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
                        Text(
                          l10n.t('checkoutOrderNumber'),
                          style: theme.textTheme.labelSmall,
                        ),
                        Text(
                          order.orderNumber,
                          style: theme.textTheme.titleMedium,
                          textDirection: TextDirection.ltr,
                        ),
                      ],
                    ),
                  ),
                  if (order.status != null)
                    OrderStatusChip(status: order.status!),
                ],
              ),
              const SizedBox(height: AppSpacing.md),
              Text(
                '${l10n.t('orderPlacedAt')}: '
                '${Formatters.dateTime(order.createdAt, lang: lang)}',
                style: theme.textTheme.bodySmall,
              ),
            ],
          ),
        ),

        // ── Timeline ──────────────────────────────────────────────────────
        if (order.timeline.isNotEmpty) ...[
          const SizedBox(height: AppSpacing.xl),
          _SectionTitle(l10n.t('orderTimeline')),
          _Timeline(steps: order.timeline),
        ],

        // ── Items ─────────────────────────────────────────────────────────
        const SizedBox(height: AppSpacing.xl),
        _SectionTitle(l10n.t('orderItems')),
        for (final line in order.items)
          Padding(
            padding: const EdgeInsets.only(bottom: AppSpacing.sm),
            child: _OrderLineRow(line: line),
          ),

        // ── Totals ────────────────────────────────────────────────────────
        const SizedBox(height: AppSpacing.lg),
        Container(
          padding: const EdgeInsets.all(AppSpacing.lg),
          decoration: BoxDecoration(
            color: theme.cardColor,
            borderRadius: AppRadius.cardRadius,
            border: Border.all(color: theme.dividerColor),
          ),
          child: Column(
            children: [
              _totalRow(
                theme,
                l10n.t('cartSubtotal'),
                Formatters.price(order.subTotal, lang: lang),
              ),
              const SizedBox(height: AppSpacing.sm),
              _totalRow(
                theme,
                l10n.t('cartShipping'),
                order.hasFreeShipping
                    ? l10n.t('cartShippingFree')
                    : Formatters.price(order.shippingFee, lang: lang),
                valueColor:
                    order.hasFreeShipping ? AppColors.success : null,
              ),
              const Padding(
                padding: EdgeInsets.symmetric(vertical: AppSpacing.md),
                child: Divider(height: 1),
              ),
              _totalRow(
                theme,
                l10n.t('cartTotal'),
                Formatters.price(order.total, lang: lang),
                isTotal: true,
              ),
            ],
          ),
        ),

        // ── Shipping details ──────────────────────────────────────────────
        const SizedBox(height: AppSpacing.xl),
        _SectionTitle(l10n.t('checkoutShippingInfo')),
        Container(
          padding: const EdgeInsets.all(AppSpacing.lg),
          decoration: BoxDecoration(
            color: theme.cardColor,
            borderRadius: AppRadius.cardRadius,
            border: Border.all(color: theme.dividerColor),
          ),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              _detailRow(theme, Icons.person_outline, order.customerName),
              const SizedBox(height: AppSpacing.sm),
              _detailRow(
                theme,
                Icons.phone_outlined,
                // Masked: this screen is the one most likely to be
                // screenshotted and shared with support.
                Formatters.maskPhone(order.customerPhone),
              ),
              if (order.formattedAddress.isNotEmpty) ...[
                const SizedBox(height: AppSpacing.sm),
                _detailRow(
                  theme,
                  Icons.location_on_outlined,
                  order.formattedAddress,
                ),
              ],
              if (order.notes != null) ...[
                const SizedBox(height: AppSpacing.sm),
                _detailRow(theme, Icons.notes_outlined, order.notes!),
              ],
            ],
          ),
        ),

        if (showCancel) ...[
          const SizedBox(height: AppSpacing.xxl),
          OutlinedButton.icon(
            onPressed: () => _confirmCancel(context, ref),
            icon: const Icon(Icons.close, size: 20),
            label: Text(l10n.t('orderCancel')),
            style: OutlinedButton.styleFrom(
              foregroundColor: AppColors.danger,
              side: const BorderSide(color: AppColors.danger),
            ),
          ),
        ],

        const SizedBox(height: AppSpacing.xxxl),
      ],
    );
  }

  Widget _totalRow(
    ThemeData theme,
    String label,
    String value, {
    bool isTotal = false,
    Color? valueColor,
  }) {
    final style =
        isTotal ? theme.textTheme.titleMedium : theme.textTheme.bodyMedium;

    return Row(
      children: [
        Expanded(child: Text(label, style: style)),
        Text(
          value,
          style: style?.copyWith(
            color: valueColor ?? (isTotal ? theme.colorScheme.primary : null),
            fontWeight: isTotal ? FontWeight.w700 : null,
          ),
        ),
      ],
    );
  }

  Widget _detailRow(ThemeData theme, IconData icon, String value) {
    return Row(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Icon(icon, size: 18, color: theme.textTheme.bodySmall?.color),
        const SizedBox(width: AppSpacing.sm),
        Expanded(child: Text(value, style: theme.textTheme.bodyMedium)),
      ],
    );
  }

  Future<void> _confirmCancel(BuildContext context, WidgetRef ref) async {
    final l10n = context.l10n;

    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(l10n.t('orderCancel')),
        content: Text(l10n.t('orderCancelConfirm')),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: Text(l10n.t('cancel')),
          ),
          FilledButton(
            onPressed: () => Navigator.of(context).pop(true),
            style: FilledButton.styleFrom(
              backgroundColor: AppColors.danger,
            ),
            child: Text(l10n.t('confirm')),
          ),
        ],
      ),
    );

    if (confirmed != true || !context.mounted) return;

    try {
      await ref.read(ordersControllerProvider.notifier).cancel(order.id);
      if (!context.mounted) return;

      ScaffoldMessenger.of(context)
        ..hideCurrentSnackBar()
        ..showSnackBar(SnackBar(content: Text(l10n.t('orderCancelled'))));
    } catch (error) {
      if (!context.mounted) return;

      ScaffoldMessenger.of(context)
        ..hideCurrentSnackBar()
        ..showSnackBar(
          SnackBar(
            content: Text(l10n.t('orderCancelNotAllowed')),
            backgroundColor: AppColors.danger,
          ),
        );
    }
  }
}

/// Vertical progress tracker.
///
/// Renders whatever ladder the server sent, including colours and icons, so an
/// admin-added status appears without an app release.
class _Timeline extends StatelessWidget {
  const _Timeline({required this.steps});

  final List<OrderTimelineStep> steps;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Container(
      padding: const EdgeInsets.all(AppSpacing.lg),
      decoration: BoxDecoration(
        color: theme.cardColor,
        borderRadius: AppRadius.cardRadius,
        border: Border.all(color: theme.dividerColor),
      ),
      child: Column(
        children: [
          for (var i = 0; i < steps.length; i++)
            _TimelineRow(
              step: steps[i],
              isFirst: i == 0,
              isLast: i == steps.length - 1,
            ),
        ],
      ),
    );
  }
}

class _TimelineRow extends StatelessWidget {
  const _TimelineRow({
    required this.step,
    required this.isFirst,
    required this.isLast,
  });

  final OrderTimelineStep step;
  final bool isFirst;
  final bool isLast;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final color = step.isReached
        ? step.color
        : theme.dividerColor;

    return IntrinsicHeight(
      child: Row(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          // Rail: dot plus connector.
          Column(
            children: [
              Container(
                width: step.isCurrent ? 16 : 12,
                height: step.isCurrent ? 16 : 12,
                decoration: BoxDecoration(
                  color: step.isReached ? color : theme.cardColor,
                  shape: BoxShape.circle,
                  border: Border.all(color: color, width: 2),
                ),
              ),
              if (!isLast)
                Expanded(
                  child: Container(
                    width: 2,
                    // The connector is coloured by the step ABOVE it, so the
                    // filled portion of the rail stops at the current step.
                    color: step.isReached ? color : theme.dividerColor,
                  ),
                ),
            ],
          ),
          const SizedBox(width: AppSpacing.md),
          Expanded(
            child: Padding(
              padding: EdgeInsets.only(
                bottom: isLast ? 0 : AppSpacing.lg,
              ),
              child: Row(
                children: [
                  if (step.icon != null) ...[
                    Opacity(
                      opacity: step.isReached ? 1 : 0.4,
                      child: Text(step.icon!),
                    ),
                    const SizedBox(width: AppSpacing.sm),
                  ],
                  Expanded(
                    child: Text(
                      step.name,
                      style: theme.textTheme.bodyMedium?.copyWith(
                        color: step.isReached
                            ? theme.textTheme.bodyMedium?.color
                            : theme.textTheme.bodySmall?.color,
                        fontWeight:
                            step.isCurrent ? FontWeight.w700 : null,
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}

class _OrderLineRow extends StatelessWidget {
  const _OrderLineRow({required this.line});

  final OrderLine line;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = context.l10n;

    return Container(
      padding: const EdgeInsets.all(AppSpacing.sm),
      decoration: BoxDecoration(
        color: theme.cardColor,
        borderRadius: AppRadius.cardRadius,
        border: Border.all(color: theme.dividerColor),
      ),
      child: Row(
        children: [
          InkWell(
            // Only linkable when the product still exists in the catalogue;
            // the server sends a null slug once it has been removed.
            onTap: line.isLinkable
                ? () => context.push(Routes.product(line.productSlug!))
                : null,
            child: AppNetworkImage(
              url: line.imageUrl,
              width: 56,
              height: 56,
              borderRadius: BorderRadius.circular(AppRadius.sm),
            ),
          ),
          const SizedBox(width: AppSpacing.md),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  line.productName,
                  style: theme.textTheme.titleSmall,
                  maxLines: 2,
                  overflow: TextOverflow.ellipsis,
                ),
                const SizedBox(height: AppSpacing.xs),
                Text(
                  '${line.quantity} × '
                  '${Formatters.price(line.unitPrice, lang: l10n.languageCode)}',
                  style: theme.textTheme.bodySmall,
                ),
              ],
            ),
          ),
          Text(
            Formatters.price(line.subTotal, lang: l10n.languageCode),
            style: theme.textTheme.titleSmall?.copyWith(
              color: theme.colorScheme.primary,
            ),
          ),
        ],
      ),
    );
  }
}

class _SectionTitle extends StatelessWidget {
  const _SectionTitle(this.title);

  final String title;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.md),
      child: Text(title, style: Theme.of(context).textTheme.titleMedium),
    );
  }
}
