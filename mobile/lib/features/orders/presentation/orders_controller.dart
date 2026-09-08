import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/l10n/locale_controller.dart';
import '../../../core/network/paged_result.dart';
import '../../../core/providers/core_providers.dart';
import '../../auth/presentation/auth_controller.dart';
import '../domain/order.dart';

/// The signed-in user's order history, paged.
class OrdersController extends AsyncNotifier<PagedResult<OrderListItem>> {
  @override
  Future<PagedResult<OrderListItem>> build() async {
    ref.watch(languageCodeProvider);

    // Re-fetch when the session changes: signing in must load that account's
    // orders, and signing out must not leave the previous user's history on
    // screen.
    ref.watch(isAuthenticatedProvider);

    if (!ref.read(isAuthenticatedProvider)) {
      return PagedResult.empty<OrderListItem>();
    }

    return ref.read(ordersRepositoryProvider).getMyOrders();
  }

  Future<void> loadMore() async {
    final current = state.valueOrNull;
    if (current == null || !current.hasNext) return;

    try {
      final next = await ref.read(ordersRepositoryProvider).getMyOrders(
            page: current.page + 1,
          );

      state = AsyncData(
        next.appendTo(current, keyOf: (order) => order.id),
      );
    } catch (_) {
      // Keep the pages already shown; the user can pull to retry.
    }
  }

  Future<void> refresh() async {
    ref.invalidateSelf();
    await future;
  }

  /// Cancels an order and restores its stock server-side.
  ///
  /// The list is refreshed rather than patched locally: cancelling changes the
  /// status, `canCancel` and the timeline, and re-reading is the only way to
  /// be sure the app agrees with the server on all three.
  Future<void> cancel(int orderId) async {
    await ref.read(ordersRepositoryProvider).cancelOrder(orderId);
    ref.invalidateSelf();
    ref.invalidate(orderDetailProvider(orderId));
    await future;
  }
}

final ordersControllerProvider =
    AsyncNotifierProvider<OrdersController, PagedResult<OrderListItem>>(
  OrdersController.new,
);

/// One order's full detail.
final orderDetailProvider =
    FutureProvider.family<OrderDetail, int>((ref, orderId) async {
  ref.watch(languageCodeProvider);
  return ref.read(ordersRepositoryProvider).getOrder(orderId);
});

/// Guest order lookup.
///
/// A `family` keyed on both the number and the phone digits, so a wrong-digits
/// attempt is a distinct cache entry and correcting them refetches instead of
/// replaying the cached 404.
final trackedOrderProvider = FutureProvider.family<OrderDetail,
    ({String orderNumber, String phoneLast4})>((ref, args) async {
  return ref.read(ordersRepositoryProvider).trackOrder(
        orderNumber: args.orderNumber,
        phoneLast4: args.phoneLast4,
      );
});
