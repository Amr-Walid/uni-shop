import 'package:equatable/equatable.dart';

import '../../../core/utils/json.dart';

/// One selectable value in a filter group. Mirrors `AttributeFilterOptionDto`.
class AttributeOption extends Equatable {
  const AttributeOption({
    required this.valueId,
    required this.valueAr,
    required this.valueEn,
    required this.count,
  });

  final int valueId;
  final String valueAr;
  final String valueEn;

  /// Number of matching products, so the sheet can show "أحمر (12)" and the
  /// shopper knows a facet is not a dead end before tapping it.
  final int count;

  /// Note: this group of DTOs is the ONE exception to the rule that the API
  /// pre-localizes text. `AttributeFilterGroupDto` predates the mobile
  /// endpoints (it is shared with the admin dashboard) and ships both columns,
  /// so the choice is made here instead.
  String label(String lang) {
    if (lang == 'en') {
      return valueEn.trim().isNotEmpty ? valueEn : valueAr;
    }
    return valueAr.trim().isNotEmpty ? valueAr : valueEn;
  }

  factory AttributeOption.fromJson(Map<String, dynamic> json) =>
      AttributeOption(
        valueId: Json.integer(json, 'valueId'),
        valueAr: Json.str(json, 'valueAr'),
        valueEn: Json.str(json, 'valueEn'),
        count: Json.integer(json, 'count'),
      );

  @override
  List<Object?> get props => [valueId, count];
}

/// A filterable attribute and its available values.
/// Mirrors `AttributeFilterGroupDto`.
///
/// The server already drops attributes whose values match no product, so an
/// empty group never reaches the client and the sheet needs no filtering.
class ProductAttribute extends Equatable {
  const ProductAttribute({
    required this.attributeId,
    required this.nameAr,
    required this.nameEn,
    this.options = const [],
  });

  final int attributeId;
  final String nameAr;
  final String nameEn;
  final List<AttributeOption> options;

  String label(String lang) {
    if (lang == 'en') {
      return nameEn.trim().isNotEmpty ? nameEn : nameAr;
    }
    return nameAr.trim().isNotEmpty ? nameAr : nameEn;
  }

  factory ProductAttribute.fromJson(Map<String, dynamic> json) =>
      ProductAttribute(
        attributeId: Json.integer(json, 'attributeId'),
        nameAr: Json.str(json, 'nameAr'),
        nameEn: Json.str(json, 'nameEn'),
        options: Json.list(json, 'options', AttributeOption.fromJson),
      );

  @override
  List<Object?> get props => [attributeId, options];
}
