import 'package:uuid/uuid.dart';

import '../../../core/network/api_client.dart';
import '../../../core/network/paged_result.dart';
import '../../cart/domain/cart_item.dart';
import '../../cart/domain/cart_validation.dart';
import '../domain/order.dart';

/// Checkout, order history and guest tracking.
class OrdersRepository {
  const OrdersRepository(this._client);

  final ApiClient _client;

  static const _uuid = Uuid();

  /// Validates the cart against live prices and stock.
  ///
  /// Called immediately before checkout. This is the only defence against the
  /// window between adding an item and paying for it, in which stock can sell
  /// out and an admin can change a price.
  Future<CartValidation> validateCart(Cart cart) async {
    final json = await _client.post<Map<String, dynamic>>(
      'cart/validate',
      body: {'items': cart.toRequestItems()},
    );
    return CartValidation.fromJson(json);
  }

  /// Places the order.
  ///
  /// [idempotencyKey] is required, not optional. Checkout is the one request
  /// that must never execute twice: a retry after a timeout — or a user
  /// double-tapping the button — would otherwise create a second order and
  /// deduct stock again. The caller generates the key ONCE per checkout
  /// attempt and reuses it across retries; the server replays the original
  /// response instead of re-running the operation.
  Future<CheckoutResult> checkout({
    required Cart cart,
    required String customerName,
    required String customerPhone,
    required String address,
    required String idempotencyKey,
    String? customerEmail,
    String? city,
    String? governorate,
    String? notes,
    String paymentMethod = 'cod',
  }) async {
    final json = await _client.post<Map<String, dynamic>>(
      'orders/checkout',
      headers: {'Idempotency-Key': idempotencyKey},
      body: {
        'customerName': customerName.trim(),
        'customerPhone': customerPhone.trim(),
        'customerEmail': customerEmail?.trim(),
        'address': address.trim(),
        'city': city?.trim(),
        'governorate': governorate?.trim(),
        'notes': notes?.trim(),
        'paymentMethod': paymentMethod,
        'items': cart.toRequestItems(),
      },
    );
    return CheckoutResult.fromJson(json);
  }

  /// Generates a checkout idempotency key.
  ///
  /// Held by the checkout controller for the lifetime of one attempt and
  /// regenerated only after a *successful* order, so every retry of a failed
  /// or timed-out submission carries the same key.
  static String newIdempotencyKey() => _uuid.v4();

  /// The signed-in user's orders. Backed by `GET /orders/my`.
  Future<PagedResult<OrderListItem>> getMyOrders({
    int page = 1,
    int pageSize = PagedResult.defaultPageSize,
  }) async {
    final json = await _client.get<Map<String, dynamic>>(
      'orders/my',
      query: {'page': page, 'pageSize': pageSize},
    );
    return PagedResult.fromJson(json, OrderListItem.fromJson);
  }

  /// One order by id. Server-side this is scoped to the caller's `UserId`, so
  /// another user's id returns 404 rather than their data.
  Future<OrderDetail> getOrder(int orderId) async {
    final json = await _client.get<Map<String, dynamic>>('orders/$orderId');
    return OrderDetail.fromJson(json);
  }

  /// Guest tracking by order number.
  ///
  /// [phoneLast4] is a mandatory second factor: an order number alone is
  /// guessable, and without it the endpoint would expose any customer's name,
  /// address and purchases. The server returns an identical 404 for "no such
  /// order" and "wrong digits" so it cannot confirm which numbers are real.
  Future<OrderDetail> trackOrder({
    required String orderNumber,
    required String phoneLast4,
  }) async {
    final json = await _client.get<Map<String, dynamic>>(
      'orders/track/${orderNumber.trim()}',
      query: {'phone4': phoneLast4.trim()},
    );
    return OrderDetail.fromJson(json);
  }

  /// Cancels an order and restores its stock in the same transaction.
  ///
  /// Only permitted while the order is in a cancellable status; the server
  /// owns that rule, which is why `canCancel` is read from the order rather
  /// than derived here.
  Future<OrderDetail> cancelOrder(int orderId) async {
    final json = await _client.post<Map<String, dynamic>>(
      'orders/$orderId/cancel',
    );
    return OrderDetail.fromJson(json);
  }

  /// The status ladder, for rendering a tracker before an order is loaded.
  Future<List<OrderStatus>> getStatuses() async {
    final json = await _client.get<List<dynamic>>('orders/statuses');

    final result = <OrderStatus>[];
    for (final entry in json) {
      if (entry is! Map) continue;
      try {
        result.add(OrderStatus.fromJson(Map<String, dynamic>.from(entry)));
      } catch (_) {
        continue;
      }
    }
    return result;
  }
}
