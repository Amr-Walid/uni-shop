import 'package:decimal/decimal.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/providers/core_providers.dart';
import '../../catalog/domain/product.dart';
import '../data/cart_storage.dart';
import '../domain/cart_item.dart';
import '../domain/cart_validation.dart';

/// Owns the shopping cart.
///
/// The cart is local-first: every mutation updates state and persists
/// immediately, with no network call. That is deliberate — adding to cart must
/// feel instant and must work with no connection, and the store is fully
/// shoppable without an account. The server is consulted only to *validate*
/// before checkout, which is where correctness actually matters.
class CartController extends Notifier<Cart> {
  CartStorage get _storage => ref.read(cartStorageProvider);

  @override
  Cart build() {
    // Read synchronously from SharedPreferences (already resolved in main) so
    // the cart badge is correct on first paint rather than popping in.
    return Cart(items: _storage.read());
  }

  /// Adds a product, or increases its quantity if already present.
  ///
  /// Returns the resulting quantity so the caller can show
  /// "تمت الإضافة (٢)" without re-reading state.
  int addProduct(Product product, {int quantity = 1}) {
    if (quantity < 1) return state.quantityOf(product.id);

    final existing = state.findItem(product.id);
    final items = List<CartItem>.from(state.items);

    if (existing == null) {
      items.add(CartItem.fromProduct(product, quantity: quantity));
    } else {
      final index = items.indexOf(existing);
      items[index] = existing.copyWith(
        quantity: existing.quantity + quantity,
        // Refresh the cached price: the product was just viewed, so this is
        // the freshest figure the app has seen.
        unitPrice: product.price,
      );
    }

    _apply(items);
    return state.quantityOf(product.id);
  }

  /// Sets an exact quantity. A quantity of zero removes the line, which is
  /// what a stepper decremented to zero should do.
  void setQuantity(int productId, int quantity) {
    if (quantity <= 0) {
      removeProduct(productId);
      return;
    }

    final existing = state.findItem(productId);
    if (existing == null) return;

    final items = List<CartItem>.from(state.items);
    items[items.indexOf(existing)] = existing.copyWith(quantity: quantity);
    _apply(items);
  }

  void increment(int productId) =>
      setQuantity(productId, state.quantityOf(productId) + 1);

  void decrement(int productId) =>
      setQuantity(productId, state.quantityOf(productId) - 1);

  void removeProduct(int productId) {
    final items =
        state.items.where((item) => item.productId != productId).toList();
    _apply(items);
  }

  void clear() => _apply(const []);

  /// Applies the server's verdict after validation.
  ///
  /// Prices are corrected and quantities clamped to what is actually
  /// available; lines the server rejected outright are dropped. This runs
  /// AFTER the user has been shown the issues, so it never silently changes a
  /// total they are about to approve.
  void applyValidation(CartValidation validation) {
    final byProduct = {
      for (final issue in validation.issues) issue.productId: issue,
    };

    final items = <CartItem>[];
    for (final item in state.items) {
      final issue = byProduct[item.productId];

      if (issue == null) {
        items.add(item);
        continue;
      }

      switch (issue.kind) {
        case CartIssueKind.unavailable:
          // Dropped entirely — nothing makes an unavailable product buyable.
          continue;

        case CartIssueKind.insufficientStock:
          final available = issue.availableQuantity ?? 0;
          if (available <= 0) continue;
          items.add(item.copyWith(quantity: available));

        case CartIssueKind.priceChanged:
          items.add(item.copyWith(unitPrice: issue.newPrice ?? item.unitPrice));

        case CartIssueKind.unknown:
          // An issue kind this build does not understand is left untouched:
          // the server will reject it again at checkout, which is safer than
          // guessing at a repair.
          items.add(item);
      }
    }

    state = validation.cart.copyWith(items: items);
    _persist();
  }

  /// Adopts the shipping rules from the store settings.
  ///
  /// Fees live in SiteSettings and can change without an app release, so they
  /// are never hardcoded into the cart.
  void applyShippingRules({
    required Decimal shippingFee,
    required Decimal freeShippingAbove,
  }) {
    state = state.copyWith(
      shippingFee: shippingFee,
      freeShippingAbove: freeShippingAbove,
    );
  }

  void _apply(List<CartItem> items) {
    state = state.copyWith(items: items);
    _persist();
  }

  /// Fire-and-forget persistence.
  ///
  /// Not awaited so a mutation never blocks the UI frame; a failed write costs
  /// at most the items added since the last successful one.
  void _persist() {
    _storage.write(state.items);
  }
}

final cartControllerProvider =
    NotifierProvider<CartController, Cart>(CartController.new);

/// Unit count for the bottom-nav badge.
///
/// A separate provider so the badge rebuilds only when the count changes, not
/// on every price or shipping-rule update.
final cartItemsCountProvider = Provider<int>((ref) {
  return ref.watch(cartControllerProvider).itemsCount;
});

/// Quantity of one product, for the stepper on a card or detail page.
final cartQuantityProvider = Provider.family<int, int>((ref, productId) {
  return ref.watch(cartControllerProvider).quantityOf(productId);
});
