import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:package_info_plus/package_info_plus.dart';

import '../../../core/l10n/app_localizations.dart';
import '../../../core/l10n/locale_controller.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_spacing.dart';
import '../../../core/widgets/state_views.dart';
import '../../../routing/routes.dart';
import '../domain/user_profile.dart';
import 'auth_controller.dart';

/// Account tab.
///
/// Reachable while signed out too: language and theme are device preferences,
/// not account settings, so hiding the whole tab behind a login would strip a
/// guest of any way to switch to English or dark mode.
class AccountScreen extends ConsumerWidget {
  const AccountScreen({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = context.l10n;
    final user = ref.watch(currentUserProvider);

    return Scaffold(
      appBar: AppBar(title: Text(l10n.t('accountTitle'))),
      body: ListView(
        padding: const EdgeInsets.only(bottom: AppSpacing.xxxl * 2),
        children: [
          if (user != null)
            _ProfileHeader(user: user)
          else
            _SignInPrompt(),

          const SizedBox(height: AppSpacing.lg),

          _SectionLabel(l10n.t('accountSettings')),
          const _LanguageTile(),
          const _ThemeTile(),

          const Divider(height: AppSpacing.xl),

          _SectionLabel(l10n.t('accountAbout')),
          ListTile(
            leading: const Icon(Icons.local_shipping_outlined),
            title: Text(l10n.t('orderTrackTitle')),
            trailing: const Icon(Icons.chevron_right),
            onTap: () => context.push(Routes.trackOrder),
          ),
          const _VersionTile(),

          if (user != null) ...[
            const Divider(height: AppSpacing.xl),
            ListTile(
              leading: const Icon(
                Icons.logout,
                color: AppColors.danger,
              ),
              title: Text(
                l10n.t('authLogout'),
                style: const TextStyle(color: AppColors.danger),
              ),
              onTap: () => _confirmLogout(context, ref),
            ),
          ],
        ],
      ),
    );
  }

  Future<void> _confirmLogout(BuildContext context, WidgetRef ref) async {
    final l10n = context.l10n;

    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(l10n.t('authLogout')),
        content: Text(l10n.t('authLogoutConfirm')),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(false),
            child: Text(l10n.t('cancel')),
          ),
          FilledButton(
            onPressed: () => Navigator.of(context).pop(true),
            style: FilledButton.styleFrom(
              backgroundColor: AppColors.danger,
            ),
            child: Text(l10n.t('authLogout')),
          ),
        ],
      ),
    );

    if (confirmed != true) return;

    await ref.read(authControllerProvider.notifier).logout();

    if (context.mounted) context.go(Routes.home);
  }
}

class _ProfileHeader extends StatelessWidget {
  const _ProfileHeader({required this.user});

  final UserProfile user;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.all(AppSpacing.lg),
      child: Row(
        children: [
          Container(
            width: 56,
            height: 56,
            decoration: BoxDecoration(
              color: theme.colorScheme.primary.withOpacity(0.12),
              shape: BoxShape.circle,
            ),
            alignment: Alignment.center,
            child: Text(
              user.initial,
              style: theme.textTheme.headlineSmall?.copyWith(
                color: theme.colorScheme.primary,
              ),
            ),
          ),
          const SizedBox(width: AppSpacing.md),
          Expanded(
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  user.displayName,
                  style: theme.textTheme.titleMedium,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
                Text(
                  user.email,
                  style: theme.textTheme.bodySmall,
                  textDirection: TextDirection.ltr,
                  maxLines: 1,
                  overflow: TextOverflow.ellipsis,
                ),
              ],
            ),
          ),
        ],
      ),
    );
  }
}

class _SignInPrompt extends StatelessWidget {
  @override
  Widget build(BuildContext context) {
    final l10n = context.l10n;

    return Padding(
      padding: const EdgeInsets.all(AppSpacing.lg),
      child: EmptyState(
        icon: Icons.person_outline,
        title: l10n.t('authLogin'),
        message: l10n.t('authGuestPrompt'),
        actionLabel: l10n.t('authLogin'),
        onAction: () => context.push(Routes.login),
        compact: true,
      ),
    );
  }
}

class _LanguageTile extends ConsumerWidget {
  const _LanguageTile();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = context.l10n;
    final locale = ref.watch(localeControllerProvider);

    return ListTile(
      leading: const Icon(Icons.language),
      title: Text(l10n.t('accountLanguage')),
      trailing: SegmentedButton<String>(
        segments: const [
          ButtonSegment(value: 'ar', label: Text('العربية')),
          ButtonSegment(value: 'en', label: Text('English')),
        ],
        selected: {locale.languageCode},
        showSelectedIcon: false,
        onSelectionChanged: (selection) {
          // Also updates Accept-Language, so server-localized product and
          // category names switch with the UI.
          ref
              .read(localeControllerProvider.notifier)
              .setLocale(Locale(selection.first));
        },
      ),
    );
  }
}

class _ThemeTile extends ConsumerWidget {
  const _ThemeTile();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = context.l10n;
    final mode = ref.watch(themeModeControllerProvider);

    final label = switch (mode) {
      ThemeMode.light => l10n.t('themeLight'),
      ThemeMode.dark => l10n.t('themeDark'),
      ThemeMode.system => l10n.t('themeSystem'),
    };

    return ListTile(
      leading: const Icon(Icons.brightness_6_outlined),
      title: Text(l10n.t('accountTheme')),
      subtitle: Text(label),
      trailing: const Icon(Icons.chevron_right),
      onTap: () => _pick(context, ref, mode),
    );
  }

  Future<void> _pick(
    BuildContext context,
    WidgetRef ref,
    ThemeMode current,
  ) async {
    final l10n = context.l10n;

    final selected = await showModalBottomSheet<ThemeMode>(
      context: context,
      builder: (context) => SafeArea(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Padding(
              padding: const EdgeInsets.all(AppSpacing.lg),
              child: Text(
                l10n.t('accountTheme'),
                style: Theme.of(context).textTheme.titleMedium,
              ),
            ),
            for (final mode in ThemeMode.values)
              RadioListTile<ThemeMode>(
                value: mode,
                groupValue: current,
                onChanged: (value) => Navigator.of(context).pop(value),
                title: Text(
                  switch (mode) {
                    ThemeMode.light => l10n.t('themeLight'),
                    ThemeMode.dark => l10n.t('themeDark'),
                    ThemeMode.system => l10n.t('themeSystem'),
                  },
                ),
              ),
            const SizedBox(height: AppSpacing.sm),
          ],
        ),
      ),
    );

    if (selected != null) {
      await ref.read(themeModeControllerProvider.notifier).setMode(selected);
    }
  }
}

/// Shows the installed version.
///
/// Read from the platform bundle rather than a hardcoded constant, so it
/// cannot drift from what is actually installed — and it is the same value
/// compared against `AppConfig.minSupportedVersion` for force-update.
class _VersionTile extends StatelessWidget {
  const _VersionTile();

  @override
  Widget build(BuildContext context) {
    final l10n = context.l10n;

    return FutureBuilder<PackageInfo>(
      future: PackageInfo.fromPlatform(),
      builder: (context, snapshot) {
        final info = snapshot.data;

        return ListTile(
          leading: const Icon(Icons.info_outline),
          title: Text(l10n.t('accountVersion')),
          subtitle: Text(
            info == null
                ? '—'
                : '${info.version} (${info.buildNumber})',
            textDirection: TextDirection.ltr,
          ),
        );
      },
    );
  }
}

class _SectionLabel extends StatelessWidget {
  const _SectionLabel(this.label);

  final String label;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Padding(
      padding: const EdgeInsets.fromLTRB(
        AppSpacing.lg,
        AppSpacing.md,
        AppSpacing.lg,
        AppSpacing.xs,
      ),
      child: Text(
        label,
        style: theme.textTheme.labelMedium?.copyWith(
          color: theme.colorScheme.primary,
        ),
      ),
    );
  }
}
