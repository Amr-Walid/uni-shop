import 'package:decimal/decimal.dart';
import 'package:equatable/equatable.dart';

import '../../../core/utils/json.dart';
import 'cart_item.dart';

/// Why a cart line could not be honoured.
///
/// The server sends a stable string code alongside a localized message; the
/// code is what the app branches on so behaviour never depends on wording.
enum CartIssueKind {
  /// Product was deleted or hidden since it was added.
  unavailable,

  /// Requested quantity exceeds remaining stock.
  insufficientStock,

  /// Price changed since it was added — the customer must be shown the new one
  /// before being charged.
  priceChanged,

  unknown;

  static CartIssueKind fromCode(String code) => switch (code.toUpperCase()) {
        'PRODUCT_UNAVAILABLE' ||
        'PRODUCT_NOT_FOUND' =>
          CartIssueKind.unavailable,
        'INSUFFICIENT_STOCK' ||
        'OUT_OF_STOCK' =>
          CartIssueKind.insufficientStock,
        'PRICE_CHANGED' => CartIssueKind.priceChanged,
        _ => CartIssueKind.unknown,
      };
}

/// One problem found while validating the cart. Mirrors `CartItemIssueDto`.
class CartIssue extends Equatable {
  const CartIssue({
    required this.productId,
    required this.productName,
    required this.kind,
    required this.message,
    this.oldPrice,
    this.newPrice,
    this.requestedQuantity,
    this.availableQuantity,
  });

  final int productId;
  final String productName;
  final CartIssueKind kind;

  /// Server-localized text. Shown as-is rather than re-derived locally, so a
  /// new issue type added server-side still reads correctly in an old build.
  final String message;

  final Decimal? oldPrice;
  final Decimal? newPrice;
  final int? requestedQuantity;

  /// Note: this is stock exposure limited to the specific item the customer
  /// already tried to buy — it is not the general catalogue stock leak the
  /// list DTOs avoid.
  final int? availableQuantity;

  /// True when the app can silently repair the line (clamp quantity / accept
  /// the new price) instead of blocking checkout.
  bool get isAutoFixable =>
      kind == CartIssueKind.insufficientStock ||
      kind == CartIssueKind.priceChanged;

  factory CartIssue.fromJson(Map<String, dynamic> json) => CartIssue(
        productId: Json.integer(json, 'productId'),
        productName: Json.str(json, 'productName'),
        kind: CartIssueKind.fromCode(Json.str(json, 'issueCode')),
        message: Json.str(json, 'message'),
        oldPrice: Json.moneyOrNull(json, 'oldPrice'),
        newPrice: Json.moneyOrNull(json, 'newPrice'),
        requestedQuantity: Json.intOrNull(json, 'requestedQuantity'),
        availableQuantity: Json.intOrNull(json, 'availableQuantity'),
      );

  @override
  List<Object?> get props => [productId, kind];
}

/// Result of `POST /api/v1/cart/validate`. Mirrors `CartValidationResultDto`.
///
/// This call is made immediately before checkout. It is the only defence
/// against the window between adding an item and paying for it, during which
/// stock can sell out and an admin can change a price.
class CartValidation extends Equatable {
  const CartValidation({
    required this.isValid,
    required this.cart,
    this.issues = const [],
  });

  final bool isValid;

  /// The cart as the *server* sees it, with authoritative prices and totals.
  final Cart cart;

  final List<CartIssue> issues;

  bool get hasIssues => issues.isNotEmpty;

  /// Issues the app can repair without user intervention.
  List<CartIssue> get autoFixable =>
      issues.where((i) => i.isAutoFixable).toList();

  /// Lines that must be removed — nothing can make an unavailable product
  /// purchasable.
  List<CartIssue> get blocking =>
      issues.where((i) => i.kind == CartIssueKind.unavailable).toList();

  factory CartValidation.fromJson(Map<String, dynamic> json) =>
      CartValidation(
        isValid: Json.boolean(json, 'isValid', fallback: true),
        cart: Cart.fromJson(json),
        issues: Json.list(json, 'issues', CartIssue.fromJson),
      );

  @override
  List<Object?> get props => [isValid, cart, issues];
}
