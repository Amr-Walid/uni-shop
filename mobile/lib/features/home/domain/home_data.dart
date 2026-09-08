import 'package:decimal/decimal.dart';
import 'package:equatable/equatable.dart';
import 'package:flutter/material.dart';

import '../../../core/theme/app_colors.dart';
import '../../../core/utils/json.dart';
import '../../catalog/domain/category.dart';
import '../../catalog/domain/product.dart';

/// Non-sensitive store configuration. Mirrors `PublicSettingsDto`.
///
/// This is what lets the admin change the theme colour and shipping rules
/// without shipping a new build to the app stores.
class StoreSettings extends Equatable {
  const StoreSettings({
    this.siteName = 'Uni-Shop',
    this.taglineAr,
    this.taglineEn,
    this.primaryColorHex = '#1A6BFF',
    Decimal? shippingFee,
    Decimal? freeShippingAbove,
    this.currency = 'EGP',
    this.currencySymbolAr = 'ج.م',
    this.currencySymbolEn = 'EGP',
  })  : _shippingFee = shippingFee,
        _freeShippingAbove = freeShippingAbove;

  final String siteName;
  final String? taglineAr;
  final String? taglineEn;
  final String primaryColorHex;

  final Decimal? _shippingFee;
  final Decimal? _freeShippingAbove;

  final String currency;
  final String currencySymbolAr;
  final String currencySymbolEn;

  Decimal get shippingFee => _shippingFee ?? Decimal.zero;
  Decimal get freeShippingAbove =>
      _freeShippingAbove ?? Decimal.fromInt(5000);

  /// Admin-configured brand colour, used to re-theme the app at runtime.
  Color get primaryColor => AppColors.fromHex(primaryColorHex);

  String tagline(String lang) =>
      (lang == 'en' ? taglineEn : taglineAr) ?? taglineEn ?? taglineAr ?? '';

  String currencySymbol(String lang) =>
      lang == 'en' ? currencySymbolEn : currencySymbolAr;

  factory StoreSettings.fromJson(Map<String, dynamic> json) => StoreSettings(
        siteName: Json.str(json, 'siteName', fallback: 'Uni-Shop'),
        taglineAr: Json.strOrNull(json, 'taglineAr'),
        taglineEn: Json.strOrNull(json, 'taglineEn'),
        primaryColorHex:
            Json.str(json, 'primaryColor', fallback: '#1A6BFF'),
        shippingFee: Json.moneyOrNull(json, 'shippingFee'),
        freeShippingAbove: Json.moneyOrNull(json, 'freeShippingAbove'),
        currency: Json.str(json, 'currency', fallback: 'EGP'),
        currencySymbolAr:
            Json.str(json, 'currencySymbolAr', fallback: 'ج.م'),
        currencySymbolEn:
            Json.str(json, 'currencySymbolEn', fallback: 'EGP'),
      );

  @override
  List<Object?> get props => [
        siteName,
        primaryColorHex,
        _shippingFee,
        _freeShippingAbove,
      ];
}

/// Everything the home screen needs, from one request.
/// Mirrors `HomeDataDto`.
///
/// The backend aggregates this deliberately: fetching banners, categories,
/// brands, featured, new arrivals, sale items and settings separately is seven
/// sequential round-trips, which on a 200-400 ms mobile RTT is over a second
/// before anything paints.
class HomeData extends Equatable {
  const HomeData({
    this.categories = const [],
    this.brands = const [],
    this.heroProducts = const [],
    this.featuredProducts = const [],
    this.newArrivals = const [],
    this.onSale = const [],
    this.settings = const StoreSettings(),
  });

  final List<Category> categories;
  final List<Brand> brands;
  final List<Product> heroProducts;
  final List<Product> featuredProducts;
  final List<Product> newArrivals;
  final List<Product> onSale;
  final StoreSettings settings;

  /// True when the store has no content at all — a fresh install pointed at an
  /// unseeded database, which should show an empty state rather than a page of
  /// blank carousels.
  bool get isEmpty =>
      categories.isEmpty &&
      heroProducts.isEmpty &&
      featuredProducts.isEmpty &&
      newArrivals.isEmpty &&
      onSale.isEmpty;

  factory HomeData.fromJson(Map<String, dynamic> json) => HomeData(
        categories: Json.list(json, 'categories', Category.fromJson),
        brands: Json.list(json, 'brands', Brand.fromJson),
        heroProducts: Json.list(json, 'heroProducts', Product.fromJson),
        featuredProducts:
            Json.list(json, 'featuredProducts', Product.fromJson),
        newArrivals: Json.list(json, 'newArrivals', Product.fromJson),
        onSale: Json.list(json, 'onSale', Product.fromJson),
        settings: StoreSettings.fromJson(
          Json.object(json, 'settings') ?? const {},
        ),
      );

  @override
  List<Object?> get props => [
        categories,
        brands,
        heroProducts,
        featuredProducts,
        newArrivals,
        onSale,
        settings,
      ];
}

/// Remote kill-switch and force-update contract. Mirrors `AppConfigDto`.
///
/// This is the only way to react to a critical bug in a build that is already
/// on customers' phones. Fetched at startup, before the first screen renders.
class AppConfig extends Equatable {
  const AppConfig({
    this.minSupportedVersion = '1.0.0',
    this.latestVersion = '1.0.0',
    this.forceUpdate = false,
    this.updateMessageAr,
    this.updateMessageEn,
    this.androidStoreUrl,
    this.iosStoreUrl,
    this.maintenanceMode = false,
    this.maintenanceMessageAr,
    this.maintenanceMessageEn,
    this.settings = const StoreSettings(),
  });

  final String minSupportedVersion;
  final String latestVersion;
  final bool forceUpdate;
  final String? updateMessageAr;
  final String? updateMessageEn;
  final String? androidStoreUrl;
  final String? iosStoreUrl;

  final bool maintenanceMode;
  final String? maintenanceMessageAr;
  final String? maintenanceMessageEn;

  final StoreSettings settings;

  String? updateMessage(String lang) =>
      lang == 'en' ? updateMessageEn : updateMessageAr;

  String? maintenanceMessage(String lang) =>
      lang == 'en' ? maintenanceMessageEn : maintenanceMessageAr;

  /// Whether [currentVersion] is below [minSupportedVersion].
  ///
  /// Compared component-wise as integers, not lexicographically: string
  /// comparison puts "1.10.0" before "1.9.0" and would wrongly block a newer
  /// build. Unparseable input returns false — failing open is correct here,
  /// because a version-parsing bug must not lock every user out of the app.
  bool isVersionUnsupported(String currentVersion) {
    final current = _parseVersion(currentVersion);
    final minimum = _parseVersion(minSupportedVersion);
    if (current == null || minimum == null) return false;

    for (var i = 0; i < 3; i++) {
      if (current[i] < minimum[i]) return true;
      if (current[i] > minimum[i]) return false;
    }
    return false;
  }

  static List<int>? _parseVersion(String value) {
    // Strip any build metadata: package_info_plus reports "1.0.0" but a
    // semantic version may carry "+42" or "-beta".
    final core = value.split(RegExp(r'[+\-]')).first.trim();
    final parts = core.split('.');
    if (parts.isEmpty) return null;

    final numbers = <int>[0, 0, 0];
    for (var i = 0; i < 3 && i < parts.length; i++) {
      final parsed = int.tryParse(parts[i]);
      if (parsed == null) return null;
      numbers[i] = parsed;
    }
    return numbers;
  }

  factory AppConfig.fromJson(Map<String, dynamic> json) => AppConfig(
        minSupportedVersion:
            Json.str(json, 'minSupportedVersion', fallback: '1.0.0'),
        latestVersion: Json.str(json, 'latestVersion', fallback: '1.0.0'),
        forceUpdate: Json.boolean(json, 'forceUpdate'),
        updateMessageAr: Json.strOrNull(json, 'updateMessageAr'),
        updateMessageEn: Json.strOrNull(json, 'updateMessageEn'),
        androidStoreUrl: Json.strOrNull(json, 'androidStoreUrl'),
        iosStoreUrl: Json.strOrNull(json, 'iosStoreUrl'),
        maintenanceMode: Json.boolean(json, 'maintenanceMode'),
        maintenanceMessageAr: Json.strOrNull(json, 'maintenanceMessageAr'),
        maintenanceMessageEn: Json.strOrNull(json, 'maintenanceMessageEn'),
        settings: StoreSettings.fromJson(
          Json.object(json, 'settings') ?? const {},
        ),
      );

  @override
  List<Object?> get props => [
        minSupportedVersion,
        latestVersion,
        forceUpdate,
        maintenanceMode,
        settings,
      ];
}
