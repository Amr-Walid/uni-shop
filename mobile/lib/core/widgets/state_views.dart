import 'package:flutter/material.dart';

import '../error/app_exception.dart';
import '../l10n/app_localizations.dart';
import '../theme/app_colors.dart';
import '../theme/app_spacing.dart';

/// Empty state: a successful request that returned nothing.
///
/// Kept strictly separate from [ErrorView] because the two mean opposite
/// things — "there is nothing here" needs a call to action, while "we could
/// not load it" needs a retry. Conflating them is how apps end up offering
/// "Retry" on an empty cart.
class EmptyState extends StatelessWidget {
  const EmptyState({
    super.key,
    required this.title,
    this.message,
    this.icon,
    this.emoji,
    this.actionLabel,
    this.onAction,
    this.compact = false,
  });

  final String title;
  final String? message;
  final IconData? icon;
  final String? emoji;
  final String? actionLabel;
  final VoidCallback? onAction;

  /// Tighter layout for use inside a card or sheet rather than a full page.
  final bool compact;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Center(
      child: Padding(
        padding: EdgeInsets.symmetric(
          horizontal: AppSpacing.xxl,
          vertical: compact ? AppSpacing.xl : AppSpacing.xxxl,
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            if (emoji != null)
              Text(emoji!, style: TextStyle(fontSize: compact ? 40 : 56))
            else
              Icon(
                icon ?? Icons.inbox_outlined,
                size: compact ? 40 : 56,
                color: theme.brightness == Brightness.dark
                    ? AppColors.textTertiaryDark
                    : AppColors.textTertiary,
              ),
            SizedBox(height: compact ? AppSpacing.md : AppSpacing.lg),
            Text(
              title,
              style: compact
                  ? theme.textTheme.titleSmall
                  : theme.textTheme.titleMedium,
              textAlign: TextAlign.center,
            ),
            if (message != null) ...[
              const SizedBox(height: AppSpacing.sm),
              Text(
                message!,
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: theme.textTheme.bodySmall?.color,
                ),
                textAlign: TextAlign.center,
              ),
            ],
            if (onAction != null && actionLabel != null) ...[
              SizedBox(height: compact ? AppSpacing.lg : AppSpacing.xxl),
              // Constrained rather than full-width: a stretched button on an
              // empty page draws more attention than the message.
              ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 260),
                child: FilledButton(
                  onPressed: onAction,
                  child: Text(actionLabel!),
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }
}

/// Error state with a retry affordance.
///
/// Takes the typed [AppException] rather than a string so the copy can be
/// chosen from the stable error code — a network failure and a server fault
/// need different wording and only one of them is worth retrying immediately.
class ErrorView extends StatelessWidget {
  const ErrorView({
    super.key,
    required this.error,
    this.onRetry,
    this.compact = false,
  });

  final Object error;
  final VoidCallback? onRetry;
  final bool compact;

  @override
  Widget build(BuildContext context) {
    final l10n = context.l10n;
    final theme = Theme.of(context);

    final (icon, title, message) = _describe(l10n);

    return Center(
      child: Padding(
        padding: EdgeInsets.symmetric(
          horizontal: AppSpacing.xxl,
          vertical: compact ? AppSpacing.xl : AppSpacing.xxxl,
        ),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Icon(
              icon,
              size: compact ? 40 : 56,
              color: AppColors.danger.withOpacity(0.8),
            ),
            SizedBox(height: compact ? AppSpacing.md : AppSpacing.lg),
            Text(
              title,
              style: compact
                  ? theme.textTheme.titleSmall
                  : theme.textTheme.titleMedium,
              textAlign: TextAlign.center,
            ),
            if (message != null) ...[
              const SizedBox(height: AppSpacing.sm),
              Text(
                message,
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: theme.textTheme.bodySmall?.color,
                ),
                textAlign: TextAlign.center,
              ),
            ],
            if (onRetry != null) ...[
              SizedBox(height: compact ? AppSpacing.lg : AppSpacing.xxl),
              ConstrainedBox(
                constraints: const BoxConstraints(maxWidth: 260),
                child: OutlinedButton.icon(
                  onPressed: onRetry,
                  icon: const Icon(Icons.refresh, size: 20),
                  label: Text(l10n.t('retry')),
                ),
              ),
            ],
            if (_traceId != null) ...[
              const SizedBox(height: AppSpacing.lg),
              // The server's traceId correlates this failure with its log
              // entry, which is the only way to diagnose a report from a real
              // user. Shown small and muted so it never looks like an
              // instruction.
              SelectableText(
                _traceId!,
                style: theme.textTheme.labelSmall?.copyWith(
                  color: theme.brightness == Brightness.dark
                      ? AppColors.textTertiaryDark
                      : AppColors.textTertiary,
                ),
              ),
            ],
          ],
        ),
      ),
    );
  }

  String? get _traceId =>
      error is AppException ? (error as AppException).traceId : null;

  (IconData, String, String?) _describe(AppLocalizations l10n) {
    if (error is! AppException) {
      return (
        Icons.error_outline,
        l10n.t('errorGeneric'),
        null,
      );
    }

    final exception = error as AppException;

    return switch (exception.code) {
      'NETWORK_ERROR' => (
          Icons.wifi_off_outlined,
          l10n.t('errorNetwork'),
          l10n.t('errorNetworkHint'),
        ),
      'TIMEOUT' => (
          Icons.timer_off_outlined,
          l10n.t('errorTimeout'),
          l10n.t('errorNetworkHint'),
        ),
      'RATE_LIMITED' => (
          Icons.hourglass_empty,
          l10n.t('errorRateLimited'),
          null,
        ),
      'MAINTENANCE' => (
          Icons.construction_outlined,
          l10n.t('errorMaintenance'),
          exception.message,
        ),
      'SERVER_ERROR' => (
          Icons.cloud_off_outlined,
          l10n.t('errorServer'),
          l10n.t('errorGeneric'),
        ),
      // For every other code the server already sent a localized, specific
      // message; showing it beats any generic copy the client could invent.
      _ => (
          Icons.error_outline,
          exception.message,
          null,
        ),
    };
  }
}

/// Full-page loading indicator, for the rare case where no skeleton shape is
/// known (e.g. a modal action in progress).
class LoadingView extends StatelessWidget {
  const LoadingView({super.key, this.message});

  final String? message;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);

    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          const CircularProgressIndicator(strokeWidth: 2.5),
          if (message != null) ...[
            const SizedBox(height: AppSpacing.lg),
            Text(
              message!,
              style: theme.textTheme.bodyMedium?.copyWith(
                color: theme.textTheme.bodySmall?.color,
              ),
            ),
          ],
        ],
      ),
    );
  }
}
