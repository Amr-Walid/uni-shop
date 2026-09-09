import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../features/auth/domain/user_profile.dart';
import '../features/auth/presentation/account_screen.dart';
import '../features/auth/presentation/auth_controller.dart';
import '../features/auth/presentation/login_screen.dart';
import '../features/cart/presentation/cart_screen.dart';
import '../features/catalog/domain/catalog_filter.dart';
import '../features/catalog/presentation/catalog_screen.dart';
import '../features/catalog/presentation/search_screen.dart';
import '../features/checkout/presentation/checkout_screen.dart';
import '../features/checkout/presentation/order_confirmation_screen.dart';
import '../features/home/presentation/home_screen.dart';
import '../features/orders/presentation/order_detail_screen.dart';
import '../features/orders/presentation/orders_screen.dart';
import '../features/orders/presentation/track_order_screen.dart';
import '../features/product_detail/presentation/product_detail_screen.dart';
import 'app_shell.dart';
import 'routes.dart';

/// Application router.
///
/// go_router is used rather than Navigator 1.0 because deep links are a
/// requirement, not a nicety: a push notification about an order must open
/// that order's page directly, and a shared product URL from the website must
/// resolve to the same product in the app. Declarative routes make that a
/// path-matching problem instead of a stack-manipulation one.
final appRouterProvider = Provider<GoRouter>((ref) {
  // A ValueNotifier bridge so the router re-evaluates redirects when auth
  // state changes, without rebuilding (and therefore recreating) the router.
  final authNotifier = ValueNotifier<int>(0);
  ref.listen(authControllerProvider, (_, __) => authNotifier.value++);
  ref.onDispose(authNotifier.dispose);

  return GoRouter(
    initialLocation: Routes.home,
    refreshListenable: authNotifier,
    debugLogDiagnostics: false,

    redirect: (context, state) {
      final authState = ref.read(authControllerProvider).valueOrNull;

      // Do not redirect while the session is still being restored, or a user
      // who IS signed in would be bounced to the login screen on cold start.
      if (authState == null) return null;

      final isAuthenticated = authState is Authenticated;
      final isProtected = Routes.protectedPaths
          .any((path) => state.matchedLocation.startsWith(path));

      if (isProtected && !isAuthenticated) {
        // Carry the intended destination so login can return the user there
        // rather than dumping them on the home screen.
        return Uri(
          path: Routes.login,
          queryParameters: {'redirect': state.matchedLocation},
        ).toString();
      }

      // Already signed in and sitting on the login screen — send them on.
      if (isAuthenticated && state.matchedLocation == Routes.login) {
        return state.uri.queryParameters['redirect'] ?? Routes.home;
      }

      return null;
    },

    routes: [
      // Tab shell. A StatefulShellRoute keeps a separate Navigator per tab, so
      // switching tabs preserves each one's scroll position and page stack —
      // the behaviour users expect from a bottom nav.
      StatefulShellRoute.indexedStack(
        builder: (context, state, navigationShell) =>
            AppShell(navigationShell: navigationShell),
        branches: [
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: Routes.home,
                builder: (context, state) => const HomeScreen(),
              ),
            ],
          ),
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: Routes.catalog,
                builder: (context, state) {
                  // Category/brand scope arrives as query parameters so the
                  // same route serves the plain shop tab and a category tap.
                  final params = state.uri.queryParameters;
                  return CatalogScreen(
                    initialFilter: CatalogFilter(
                      categorySlug: params['category'],
                      brandSlug: params['brand'],
                      query: params['q'],
                    ),
                    title: params['title'],
                  );
                },
              ),
            ],
          ),
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: Routes.cart,
                builder: (context, state) => const CartScreen(),
              ),
            ],
          ),
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: Routes.orders,
                builder: (context, state) => const OrdersScreen(),
              ),
            ],
          ),
          StatefulShellBranch(
            routes: [
              GoRoute(
                path: Routes.account,
                builder: (context, state) => const AccountScreen(),
              ),
            ],
          ),
        ],
      ),

      // Full-screen routes, pushed above the shell so they hide the bottom nav.
      GoRoute(
        path: Routes.productPattern,
        builder: (context, state) => ProductDetailScreen(
          slug: state.pathParameters['slug'] ?? '',
        ),
      ),
      GoRoute(
        path: Routes.search,
        builder: (context, state) => SearchScreen(
          initialQuery: state.uri.queryParameters['q'],
        ),
      ),
      GoRoute(
        path: Routes.checkout,
        builder: (context, state) => const CheckoutScreen(),
      ),
      GoRoute(
        path: Routes.orderConfirmationPattern,
        builder: (context, state) => OrderConfirmationScreen(
          orderNumber: state.pathParameters['orderNumber'] ?? '',
        ),
      ),
      GoRoute(
        path: Routes.orderDetailPattern,
        builder: (context, state) => OrderDetailScreen(
          orderId: int.tryParse(state.pathParameters['id'] ?? '') ?? 0,
        ),
      ),
      GoRoute(
        path: Routes.trackOrder,
        builder: (context, state) => TrackOrderScreen(
          initialOrderNumber: state.uri.queryParameters['number'],
        ),
      ),
      GoRoute(
        path: Routes.login,
        builder: (context, state) => LoginScreen(
          redirectTo: state.uri.queryParameters['redirect'],
        ),
      ),
    ],

    errorBuilder: (context, state) => RouteNotFoundScreen(
      location: state.matchedLocation,
    ),
  );
});
