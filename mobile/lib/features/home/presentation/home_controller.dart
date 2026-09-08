import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/l10n/locale_controller.dart';
import '../../../core/providers/core_providers.dart';
import '../../cart/presentation/cart_controller.dart';
import '../domain/home_data.dart';

/// Loads the home screen.
///
/// One request for the whole screen (`GET /api/v1/home`): fetching banners,
/// categories, brands, featured, new arrivals, sale items and settings
/// separately is seven sequential round-trips, which on a 200-400 ms mobile
/// RTT is over a second of blank screen.
class HomeController extends AsyncNotifier<HomeData> {
  @override
  Future<HomeData> build() async {
    // Re-fetch when the language changes: product and category names are
    // resolved server-side from Accept-Language, so a cached payload is in the
    // wrong language after a switch.
    ref.watch(languageCodeProvider);

    final data = await ref.read(catalogRepositoryProvider).getHomeData();

    // Adopt the store's shipping rules as soon as they are known, so the cart
    // shows a correct total without a second request. The fees live in
    // SiteSettings and can change without an app release.
    ref.read(cartControllerProvider.notifier).applyShippingRules(
          shippingFee: data.settings.shippingFee,
          freeShippingAbove: data.settings.freeShippingAbove,
        );

    return data;
  }

  /// Pull-to-refresh.
  ///
  /// The existing data is retained under the loading state so the screen does
  /// not blank out while refreshing.
  Future<void> refresh() async {
    state = AsyncLoading<HomeData>().copyWithPrevious(state);
    ref.invalidateSelf();
    await future;
  }
}

final homeControllerProvider =
    AsyncNotifierProvider<HomeController, HomeData>(HomeController.new);

/// Store settings, read from whatever the home payload last returned.
///
/// Exposed separately so screens that only need the currency symbol or the
/// shipping threshold do not depend on the whole home payload — and so they
/// still get sensible defaults before it has loaded.
final storeSettingsProvider = Provider<StoreSettings>((ref) {
  final home = ref.watch(homeControllerProvider).valueOrNull;
  return home?.settings ?? const StoreSettings();
});
