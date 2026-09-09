import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';

import '../../features/auth/data/auth_repository.dart';
import '../../features/cart/data/cart_repository.dart';
import '../../features/cart/data/cart_storage.dart';
import '../../features/catalog/data/catalog_repository.dart';
import '../../features/content/data/content_repository.dart';
import '../../features/orders/data/orders_repository.dart';
import '../l10n/locale_controller.dart';
import '../network/api_client.dart';
import '../storage/token_storage.dart';

/// Infrastructure wiring.
///
/// Every dependency is exposed as a provider so a test can override it with a
/// fake in one line. That is the specific reason Riverpod was chosen over
/// singletons or a service locator: `ProviderScope(overrides: [...])` is
/// compile-checked, whereas a locator fails at runtime when a registration is
/// forgotten.

/// Overridden in `main()` once `SharedPreferences.getInstance()` resolves.
///
/// Resolved eagerly rather than exposed as a `FutureProvider` so no widget has
/// to handle a loading state for something that is ready before first paint.
final sharedPreferencesProvider = Provider<SharedPreferences>((ref) {
  throw UnimplementedError(
    'sharedPreferencesProvider must be overridden in ProviderScope.',
  );
});

final tokenStorageProvider = Provider<TokenStorage>((ref) {
  return TokenStorage();
});

/// Signals that the session is unrecoverable (refresh reuse detected, account
/// locked or deleted), so the app can drop to an unauthenticated state.
///
/// A plain notifier rather than a callback because the event originates deep
/// in an interceptor and must reach the auth controller without the network
/// layer holding a reference to it.
class SessionExpiryNotifier extends Notifier<int> {
  @override
  int build() => 0;

  /// Incremented rather than set to a bool: two expiries in a row must both be
  /// observable, and a bool would already be true the second time.
  void notifyExpired() => state = state + 1;
}

final sessionExpiryProvider =
    NotifierProvider<SessionExpiryNotifier, int>(SessionExpiryNotifier.new);

/// The single HTTP client for the whole app.
final apiClientProvider = Provider<ApiClient>((ref) {
  final client = ApiClient(
    tokenStorage: ref.watch(tokenStorageProvider),
    onSessionExpired: () =>
        ref.read(sessionExpiryProvider.notifier).notifyExpired(),
  );

  // Keep Accept-Language in step with the UI language. `listen` with
  // fireImmediately covers the initial value too, so the very first request
  // already carries the right language.
  ref.listen<String>(
    languageCodeProvider,
    (_, next) => client.language = next,
    fireImmediately: true,
  );

  return client;
});

// ── Repositories ────────────────────────────────────────────────────────────

final catalogRepositoryProvider = Provider<CatalogRepository>((ref) {
  return CatalogRepository(ref.watch(apiClientProvider));
});

final authRepositoryProvider = Provider<AuthRepository>((ref) {
  return AuthRepository(
    client: ref.watch(apiClientProvider),
    tokenStorage: ref.watch(tokenStorageProvider),
  );
});

final ordersRepositoryProvider = Provider<OrdersRepository>((ref) {
  return OrdersRepository(ref.watch(apiClientProvider));
});

final cartRepositoryProvider = Provider<CartRepository>((ref) {
  return CartRepository(ref.watch(apiClientProvider));
});

final contentRepositoryProvider = Provider<ContentRepository>((ref) {
  return ContentRepository(ref.watch(apiClientProvider));
});

final cartStorageProvider = Provider<CartStorage>((ref) {
  return CartStorage(ref.watch(sharedPreferencesProvider));
});
