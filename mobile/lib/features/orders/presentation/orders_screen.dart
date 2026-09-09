import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/l10n/app_localizations.dart';
import '../../../core/theme/app_spacing.dart';
import '../../../core/utils/formatters.dart';
import '../../../core/widgets/app_network_image.dart';
import '../../../core/widgets/shimmer_box.dart';
import '../../../core/widgets/state_views.dart';
import '../../../routing/routes.dart';
import '../domain/order.dart';
import 'orders_controller.dart';

class OrdersScreen extends ConsumerStatefulWidget {
  const OrdersScreen({super.key});

  @override
  ConsumerState<OrdersScreen> createState() => _OrdersScreenState();
}

class _OrdersScreenState extends ConsumerState<OrdersScreen> {
  final _scrollController = ScrollController();

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
    if (position.pixels >= position.maxScrollExtent - 300) {
      ref.read(ordersControllerProvider.notifier).loadMore();
    }
  }

  @override
  Widget build(BuildContext context) {
    final l10n = context.l10n;
    final orders = ref.watch(ordersControllerProvider);

    return Scaffold(
      appBar: AppBar(
        title: Text(l10n.t('ordersTitle')),
        actions: [
          // Guest tracking stays reachable from here: a customer who ordered
          // without an account still needs a way in.
          IconButton(
            onPressed: () => context.push(Routes.trackOrder),
            icon: const Icon(Icons.local_shipping_outlined),
            tooltip: l10n.t('orderTrackTitle'),
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: () => ref.read(ordersControllerProvider.notifier).refresh(),
        child: orders.when(
          loading: () => orders.hasValue
              ? _List(
                  orders: orders.requireValue.items,
                  controller: _scrollController,
                  hasMore: orders.requireValue.hasNext,
                )
              : const ListSkeleton(),
          error: (error, _) => ListView(
            children: [
              SizedBox(
                height: MediaQuery.sizeOf(context).height * 0.6,
                child: ErrorView(
                  error: error,
                  onRetry: () => ref.invalidate(ordersControllerProvider),
                ),
              ),
            ],
          ),
          data: (page) => page.isEmpty
              ? ListView(
                  children: [
                    SizedBox(
                      height: MediaQuery.sizeOf(context).height * 0.6,
                      child: EmptyState(
                        emoji: '📦',
                        title: l10n.t('ordersEmpty'),
                        message: l10n.t('ordersEmptyHint'),
                        actionLabel: l10n.t('cartStartShopping'),
                        onAction: () => context.go(Routes.catalog),
                      ),
                    ),
                  ],
                )
              : _List(
                  orders: page.items,
                  controller: _scrollController,
                  hasMore: page.hasNext,
                ),
        ),
      ),
    );
  }
}

class _List extends StatelessWidget {
  const _List({
    required this.orders,
    required this.controller,
    required this.hasMore,
  });

  final List<OrderListItem> orders;
  final ScrollController controller;
  final bool hasMore;

  @override
  Widget build(BuildContext context) {
    return ListView.separated(
      controller: controller,
      padding: AppSpacing.pageWithBottomNav,
      itemCount: orders.length + (hasMore ? 1 : 0),
      separatorBuilder: (_, __) => const SizedBox(height: AppSpacing.md),
      itemBuilder: (context, index) {
        if (index >= orders.length) {
          return const Padding(
            padding: EdgeInsets.all(AppSpacing.lg),
            child: Center(
              child: SizedBox(
                width: 24,
                height: 24,
                child: CircularProgressIndicator(strokeWidth: 2),
              ),
            ),
          );
        }

        return OrderCard(order: orders[index]);
      },
    );
  }
}

/// One row in the order list.
class OrderCard extends StatelessWidget {
  const OrderCard({super.key, required this.order});

  final OrderListItem order;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = context.l10n;

    return Material(
      color: theme.cardColor,
      borderRadius: AppRadius.cardRadius,
      child: InkWell(
        onTap: () => context.push(Routes.orderDetail(order.id)),
        borderRadius: AppRadius.cardRadius,
        child: Container(
          padding: const EdgeInsets.all(AppSpacing.md),
          decoration: BoxDecoration(
            borderRadius: AppRadius.cardRadius,
            border: Border.all(color: theme.dividerColor),
          ),
          child: Row(
            children: [
              AppNetworkImage(
                url: order.firstItemImageUrl,
                width: 56,
                height: 56,
                borderRadius: BorderRadius.circular(AppRadius.sm),
              ),
              const SizedBox(width: AppSpacing.md),
              Expanded(
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Row(
                      children: [
                        Expanded(
                          child: Text(
                            order.orderNumber,
                            style: theme.textTheme.titleSmall,
                            textDirection: TextDirection.ltr,
                            maxLines: 1,
                            overflow: TextOverflow.ellipsis,
                          ),
                        ),
                        if (order.status != null)
                          OrderStatusChip(status: order.status!),
                      ],
                    ),
                    const SizedBox(height: AppSpacing.xs),
                    Text(
                      // Relative time is more useful than an absolute date for
                      // recent orders, which is most of this list.
                      Formatters.relative(
                        order.createdAt,
                        lang: l10n.languageCode,
                      ),
                      style: theme.textTheme.bodySmall,
                    ),
                    const SizedBox(height: AppSpacing.sm),
                    Row(
                      children: [
                        Text(
                          l10n.tf('catalogResultsCount', {
                            'n': order.itemsCount,
                          }),
                          style: theme.textTheme.bodySmall,
                        ),
                        const Spacer(),
                        Text(
                          Formatters.price(
                            order.total,
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
      ),
    );
  }
}

/// Status pill, coloured from the server-supplied hex.
class OrderStatusChip extends StatelessWidget {
  const OrderStatusChip({super.key, required this.status});

  final OrderStatus status;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final color = status.color;

    return Container(
      padding: const EdgeInsets.symmetric(
        horizontal: AppSpacing.sm,
        vertical: AppSpacing.xxs,
      ),
      decoration: BoxDecoration(
        color: color.withOpacity(0.12),
        borderRadius: AppRadius.pillRadius,
      ),
      child: Row(
        mainAxisSize: MainAxisSize.min,
        children: [
          if (status.icon != null) ...[
            Text(status.icon!, style: const TextStyle(fontSize: 11)),
            const SizedBox(width: AppSpacing.xxs),
          ],
          Text(
            status.name,
            style: theme.textTheme.labelSmall?.copyWith(color: color),
          ),
        ],
      ),
    );
  }
}
