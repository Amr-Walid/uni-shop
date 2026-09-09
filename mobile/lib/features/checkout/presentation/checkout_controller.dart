import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/providers/core_providers.dart';
import '../../cart/domain/cart_validation.dart';
import '../../cart/presentation/cart_controller.dart';
import '../../orders/data/orders_repository.dart';
import '../../orders/domain/order.dart';

/// Checkout progress.
sealed class CheckoutState {
  const CheckoutState();
}

class CheckoutIdle extends CheckoutState {
  const CheckoutIdle();
}

class CheckoutSubmitting extends CheckoutState {
  const CheckoutSubmitting();
}

/// The server rejected the cart. The user must see and resolve the issues
/// before the order can be placed.
class CheckoutNeedsReview extends CheckoutState {
  const CheckoutNeedsReview(this.validation);

  final CartValidation validation;
}

class CheckoutSuccess extends CheckoutState {
  const CheckoutSuccess(this.result);

  final CheckoutResult result;
}

class CheckoutFailure extends CheckoutState {
  const CheckoutFailure(this.error);

  final Object error;
}

/// Drives the checkout submission.
///
/// The two things this class exists to guarantee:
///   1. the cart is validated against live stock and prices before an order is
///      created, and
///   2. one checkout attempt uses ONE idempotency key across every retry, so a
///      timeout followed by a retry can never produce two orders.
class CheckoutController extends Notifier<CheckoutState> {
  /// Generated once per attempt and deliberately NOT regenerated on retry.
  ///
  /// This is the whole point of the key: if the first request actually reached
  /// the server but the response was lost, the retry carries the same key and
  /// the server replays the original result instead of charging the customer
  /// twice.
  String? _idempotencyKey;

  OrdersRepository get _repository => ref.read(ordersRepositoryProvider);

  @override
  CheckoutState build() => const CheckoutIdle();

  /// Validates the cart, then places the order.
  ///
  /// [acceptChanges] must be true to proceed when validation reported fixable
  /// issues — the caller sets it only after the user has been shown, and has
  /// accepted, the new prices or reduced quantities.
  Future<void> submit({
    required String customerName,
    required String customerPhone,
    required String address,
    String? customerEmail,
    String? city,
    String? governorate,
    String? notes,
    bool acceptChanges = false,
  }) async {
    if (state is CheckoutSubmitting) return;

    state = const CheckoutSubmitting();
    _idempotencyKey ??= OrdersRepository.newIdempotencyKey();

    final cartController = ref.read(cartControllerProvider.notifier);

    try {
      final cart = ref.read(cartControllerProvider);
      if (cart.isEmpty) {
        state = const CheckoutIdle();
        return;
      }

      // ── Step 1: validate ──────────────────────────────────────────────────
      final validation = await _repository.validateCart(cart);

      if (!validation.isValid || validation.hasIssues) {
        if (!acceptChanges) {
          // Stop and surface the issues. The cart is NOT modified yet, so the
          // totals the user is looking at do not change under them.
          state = CheckoutNeedsReview(validation);
          return;
        }

        // The user accepted: repair the cart, then continue with whatever
        // remains.
        cartController.applyValidation(validation);

        if (ref.read(cartControllerProvider).isEmpty) {
          state = CheckoutNeedsReview(validation);
          return;
        }
      }

      // ── Step 2: place the order ───────────────────────────────────────────
      final result = await _repository.checkout(
        cart: ref.read(cartControllerProvider),
        customerName: customerName,
        customerPhone: customerPhone,
        address: address,
        customerEmail: customerEmail,
        city: city,
        governorate: governorate,
        notes: notes,
        idempotencyKey: _idempotencyKey!,
      );

      // The order exists server-side; the local cart has served its purpose.
      cartController.clear();

      // Retire the key only after a confirmed success, so the NEXT checkout
      // starts a fresh idempotency scope.
      _idempotencyKey = null;

      state = CheckoutSuccess(result);
    } catch (error) {
      // The key is intentionally retained here: this failure may have been a
      // lost response to a request the server did process, and reusing the key
      // is what makes the retry safe.
      state = CheckoutFailure(error);
    }
  }

  /// Returns to the form after an error or a review prompt.
  void reset() => state = const CheckoutIdle();
}

final checkoutControllerProvider =
    NotifierProvider<CheckoutController, CheckoutState>(
  CheckoutController.new,
);
