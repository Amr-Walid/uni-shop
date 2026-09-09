import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../core/l10n/app_localizations.dart';
import '../core/theme/app_colors.dart';
import '../core/theme/app_spacing.dart';
import '../features/cart/presentation/cart_controller.dart';
import 'routes.dart';

/// Bottom-navigation shell.
///
/// Wraps a [StatefulNavigationShell], so each tab keeps its own navigation
/// stack and scroll position across switches.
class AppShell extends ConsumerWidget {
  const AppShell({super.key, required this.navigationShell});

  final StatefulNavigationShell navigationShell;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = context.l10n;
    final cartCount = ref.watch(cartItemsCountProvider);

    return Scaffold(
      body: navigationShell,
      bottomNavigationBar: NavigationBar(
        selectedIndex: navigationShell.currentIndex,
        onDestinationSelected: (index) => _onTap(context, index),
        destinations: [
          NavigationDestination(
            icon: const Icon(Icons.home_outlined),
            selectedIcon: const Icon(Icons.home),
            label: l10n.t('navHome'),
          ),
          NavigationDestination(
            icon: const Icon(Icons.grid_view_outlined),
            selectedIcon: const Icon(Icons.grid_view),
            label: l10n.t('navCatalog'),
          ),
          NavigationDestination(
            icon: _CartIcon(count: cartCount, selected: false),
            selectedIcon: _CartIcon(count: cartCount, selected: true),
            label: l10n.t('navCart'),
          ),
          NavigationDestination(
            icon: const Icon(Icons.receipt_long_outlined),
            selectedIcon: const Icon(Icons.receipt_long),
            label: l10n.t('navOrders'),
          ),
          NavigationDestination(
            icon: const Icon(Icons.person_outline),
            selectedIcon: const Icon(Icons.person),
            label: l10n.t('navAccount'),
          ),
        ],
      ),
    );
  }

  void _onTap(BuildContext context, int index) {
    // Tapping the active tab pops it to its root, which is the standard
    // bottom-nav behaviour and the only way back out of a deep stack within
    // a tab without using the system back gesture.
    navigationShell.goBranch(
      index,
      initialLocation: index == navigationShell.currentIndex,
    );
  }
}

/// Cart icon with an item-count badge.
class _CartIcon extends StatelessWidget {
  const _CartIcon({required this.count, required this.selected});

  final int count;
  final bool selected;

  @override
  Widget build(BuildContext context) {
    final icon = Icon(
      selected ? Icons.shopping_cart : Icons.shopping_cart_outlined,
    );

    if (count == 0) return icon;

    return Badge(
      // Capped at "99+": a three-digit count widens the badge enough to
      // overlap the neighbouring tab.
      label: Text(count > 99 ? '99+' : '$count'),
      backgroundColor: AppColors.sale,
      textColor: Colors.white,
      child: icon,
    );
  }
}

/// Fallback for an unmatched deep link.
///
/// Reachable in practice: a push notification or a shared link can point at a
/// route this build does not know, e.g. after a server-side URL change.
class RouteNotFoundScreen extends StatelessWidget {
  const RouteNotFoundScreen({super.key, required this.location});

  final String location;

  @override
  Widget build(BuildContext context) {
    final l10n = context.l10n;
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(title: Text(l10n.t('appName'))),
      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.xxl),
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              const Text('🧭', style: TextStyle(fontSize: 56)),
              const SizedBox(height: AppSpacing.lg),
              Text(
                l10n.t('errorGeneric'),
                style: theme.textTheme.titleMedium,
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: AppSpacing.sm),
              Text(
                location,
                style: theme.textTheme.bodySmall,
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: AppSpacing.xxl),
              FilledButton(
                onPressed: () => context.go(Routes.home),
                child: Text(l10n.t('navHome')),
              ),
            ],
          ),
        ),
      ),
    );
  }
}
