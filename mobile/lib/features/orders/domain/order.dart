import 'package:decimal/decimal.dart';
import 'package:equatable/equatable.dart';
import 'package:flutter/material.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/utils/json.dart';

/// An order status. Mirrors `OrderStatusSummaryDto`.
///
/// The status ladder lives in the database (`OrderStatus` table) and an admin
/// can add to it, so the app never hardcodes the workflow — it renders whatever
/// the server sends, including [colorHex] and [icon].
class OrderStatus extends Equatable {
  const OrderStatus({
    required this.id,
    required this.name,
    required this.colorHex,
    required this.sortOrder,
    this.icon,
  });

  final int id;

  /// Already localized by the server from the paired NameAr/NameEn columns.
  final String name;

  final String colorHex;
  final String? icon;
  final int sortOrder;

  /// Falls back to the seeded palette when the stored hex is malformed, so an
  /// admin typo cannot render a colourless chip.
  Color get color => AppColors.fromHex(
        colorHex,
        fallback: AppColors.orderStatus[id] ?? AppColors.primary,
      );

  /// Seed id 5 = تم التسليم / Delivered.
  bool get isDelivered => id == 5;

  /// Seed id 7 = ملغي / Cancelled.
  bool get isCancelled => id == 7;

  /// Seed id 6 = مرتجع / Returned.
  bool get isReturned => id == 6;

  /// A terminal state has no further steps, so the progress tracker stops.
  bool get isTerminal => isDelivered || isCancelled || isReturned;

  factory OrderStatus.fromJson(Map<String, dynamic> json) => OrderStatus(
        id: Json.integer(json, 'id'),
        name: Json.str(json, 'name'),
        colorHex: Json.str(json, 'colorHex'),
        icon: Json.strOrNull(json, 'icon'),
        sortOrder: Json.integer(json, 'sortOrder'),
      );

  @override
  List<Object?> get props => [id, name, colorHex];
}

/// Order row in the "my orders" list. Mirrors `OrderListItemDto`.
class OrderListItem extends Equatable {
  const OrderListItem({
    required this.id,
    required this.orderNumber,
    required this.total,
    required this.itemsCount,
    required this.createdAt,
    required this.canCancel,
    this.status,
    this.firstItemImageUrl,
  });

  final int id;
  final String orderNumber;
  final Decimal total;
  final int itemsCount;
  final DateTime createdAt;

  final OrderStatus? status;
  final String? firstItemImageUrl;

  /// Precomputed server-side. The app must NOT re-derive it from the status id:
  /// the cancellable set is business policy (`CancellableStatusIds = {1, 2}`)
  /// and could change without an app release.
  final bool canCancel;

  factory OrderListItem.fromJson(Map<String, dynamic> json) => OrderListItem(
        id: Json.integer(json, 'id'),
        orderNumber: Json.str(json, 'orderNumber'),
        total: Json.money(json, 'total'),
        itemsCount: Json.integer(json, 'itemsCount'),
        createdAt: Json.dateTime(json, 'createdAt') ?? DateTime.now().toUtc(),
        status: Json.object(json, 'status') != null
            ? OrderStatus.fromJson(Json.object(json, 'status')!)
            : null,
        firstItemImageUrl: Json.strOrNull(json, 'firstItemImageUrl'),
        canCancel: Json.boolean(json, 'canCancel'),
      );

  @override
  List<Object?> get props => [id, orderNumber, status, canCancel];
}

/// One purchased line. Mirrors `OrderLineDto`.
class OrderLine extends Equatable {
  const OrderLine({
    required this.productId,
    required this.productName,
    required this.quantity,
    required this.unitPrice,
    required this.subTotal,
    this.imageUrl,
    this.productSlug,
  });

  final int productId;

  /// Historical snapshot taken at purchase time — deliberately NOT the current
  /// catalogue name, so a later rename does not rewrite past invoices.
  final String productName;

  final int quantity;
  final Decimal unitPrice;
  final Decimal subTotal;
  final String? imageUrl;

  /// Null when the product has since been removed from the catalogue; the row
  /// must then render without a tappable link.
  final String? productSlug;

  bool get isLinkable => productSlug != null && productSlug!.isNotEmpty;

  factory OrderLine.fromJson(Map<String, dynamic> json) => OrderLine(
        productId: Json.integer(json, 'productId'),
        productName: Json.str(json, 'productName'),
        quantity: Json.integer(json, 'quantity', fallback: 1),
        unitPrice: Json.money(json, 'unitPrice'),
        subTotal: Json.money(json, 'subTotal'),
        imageUrl: Json.strOrNull(json, 'imageUrl'),
        productSlug: Json.strOrNull(json, 'productSlug'),
      );

  @override
  List<Object?> get props => [productId, quantity, unitPrice];
}

/// One rung of the status ladder. Mirrors `OrderTimelineStepDto`.
class OrderTimelineStep extends Equatable {
  const OrderTimelineStep({
    required this.statusId,
    required this.name,
    required this.colorHex,
    required this.isReached,
    required this.isCurrent,
    this.icon,
  });

  final int statusId;
  final String name;
  final String colorHex;
  final String? icon;

  /// The order has passed through (or is at) this step.
  final bool isReached;

  /// This is the order's current step.
  final bool isCurrent;

  Color get color => AppColors.fromHex(
        colorHex,
        fallback: AppColors.orderStatus[statusId] ?? AppColors.primary,
      );

  factory OrderTimelineStep.fromJson(Map<String, dynamic> json) =>
      OrderTimelineStep(
        statusId: Json.integer(json, 'statusId'),
        name: Json.str(json, 'name'),
        colorHex: Json.str(json, 'colorHex'),
        icon: Json.strOrNull(json, 'icon'),
        isReached: Json.boolean(json, 'isReached'),
        isCurrent: Json.boolean(json, 'isCurrent'),
      );

  @override
  List<Object?> get props => [statusId, isReached, isCurrent];
}

/// Full order detail. Mirrors `OrderDetailDto`.
///
/// Used by both the signed-in order page and the guest tracking screen — the
/// server returns the identical shape for `GET /orders/{id}` and
/// `POST /orders/track`, so there is one model and one widget tree.
class OrderDetail extends Equatable {
  const OrderDetail({
    required this.id,
    required this.orderNumber,
    required this.customerName,
    required this.customerPhone,
    required this.subTotal,
    required this.shippingFee,
    required this.total,
    required this.createdAt,
    required this.canCancel,
    this.customerEmail,
    this.address,
    this.city,
    this.governorate,
    this.notes,
    this.updatedAt,
    this.status,
    this.items = const [],
    this.timeline = const [],
  });

  final int id;
  final String orderNumber;

  final String customerName;
  final String customerPhone;
  final String? customerEmail;
  final String? address;
  final String? city;
  final String? governorate;
  final String? notes;

  final Decimal subTotal;
  final Decimal shippingFee;
  final Decimal total;

  final DateTime createdAt;
  final DateTime? updatedAt;

  final OrderStatus? status;
  final bool canCancel;

  final List<OrderLine> items;

  /// The full ladder with reached steps flagged, so the tracker can be drawn
  /// without the app knowing the workflow.
  final List<OrderTimelineStep> timeline;

  bool get hasFreeShipping => shippingFee <= Decimal.zero;

  int get itemsCount => items.fold(0, (sum, i) => sum + i.quantity);

  /// Single-line address for display, skipping the parts that are null.
  String get formattedAddress => [address, city, governorate]
      .where((part) => part != null && part.trim().isNotEmpty)
      .join('، ');

  OrderTimelineStep? get currentStep {
    for (final step in timeline) {
      if (step.isCurrent) return step;
    }
    return null;
  }

  factory OrderDetail.fromJson(Map<String, dynamic> json) => OrderDetail(
        id: Json.integer(json, 'id'),
        orderNumber: Json.str(json, 'orderNumber'),
        customerName: Json.str(json, 'customerName'),
        customerPhone: Json.str(json, 'customerPhone'),
        customerEmail: Json.strOrNull(json, 'customerEmail'),
        address: Json.strOrNull(json, 'address'),
        city: Json.strOrNull(json, 'city'),
        governorate: Json.strOrNull(json, 'governorate'),
        notes: Json.strOrNull(json, 'notes'),
        subTotal: Json.money(json, 'subTotal'),
        shippingFee: Json.money(json, 'shippingFee'),
        total: Json.money(json, 'total'),
        createdAt: Json.dateTime(json, 'createdAt') ?? DateTime.now().toUtc(),
        updatedAt: Json.dateTime(json, 'updatedAt'),
        status: Json.object(json, 'status') != null
            ? OrderStatus.fromJson(Json.object(json, 'status')!)
            : null,
        canCancel: Json.boolean(json, 'canCancel'),
        items: Json.list(json, 'items', OrderLine.fromJson),
        timeline: Json.list(json, 'timeline', OrderTimelineStep.fromJson),
      );

  @override
  List<Object?> get props => [id, orderNumber, status, canCancel, items];
}

/// Confirmation returned by `POST /api/v1/orders/checkout`.
/// Mirrors `CheckoutResponseDto`.
class CheckoutResult extends Equatable {
  const CheckoutResult({
    required this.orderId,
    required this.orderNumber,
    required this.subTotal,
    required this.shippingFee,
    required this.total,
    required this.createdAt,
    this.status,
  });

  final int orderId;

  /// The customer's only handle on the order until they sign in — the
  /// confirmation screen must make it copyable.
  final String orderNumber;

  final Decimal subTotal;
  final Decimal shippingFee;
  final Decimal total;
  final DateTime createdAt;
  final OrderStatus? status;

  factory CheckoutResult.fromJson(Map<String, dynamic> json) => CheckoutResult(
        orderId: Json.integer(json, 'orderId'),
        orderNumber: Json.str(json, 'orderNumber'),
        subTotal: Json.money(json, 'subTotal'),
        shippingFee: Json.money(json, 'shippingFee'),
        total: Json.money(json, 'total'),
        createdAt: Json.dateTime(json, 'createdAt') ?? DateTime.now().toUtc(),
        status: Json.object(json, 'status') != null
            ? OrderStatus.fromJson(Json.object(json, 'status')!)
            : null,
      );

  @override
  List<Object?> get props => [orderId, orderNumber, total];
}
