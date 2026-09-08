import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:go_router/go_router.dart';

import '../../../core/l10n/app_localizations.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_spacing.dart';
import '../../../routing/routes.dart';

/// Post-checkout confirmation.
///
/// Takes only the order number, not the whole result object: this route is
/// reachable by deep link, so depending on in-memory state would break it on a
/// cold start from a notification.
class OrderConfirmationScreen extends StatelessWidget {
  const OrderConfirmationScreen({super.key, required this.orderNumber});

  final String orderNumber;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final l10n = context.l10n;

    return Scaffold(
      // No back button: the cart has been cleared and the order placed, so
      // navigating "back" into checkout would show an empty, meaningless form.
      appBar: AppBar(automaticallyImplyLeading: false),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(AppSpacing.xxl),
          child: Column(
            children: [
              Container(
                width: 88,
                height: 88,
                decoration: BoxDecoration(
                  color: AppColors.success.withOpacity(0.12),
                  shape: BoxShape.circle,
                ),
                child: const Icon(
                  Icons.check_rounded,
                  size: 48,
                  color: AppColors.success,
                ),
              ),

              const SizedBox(height: AppSpacing.xl),

              Text(
                l10n.t('checkoutSuccessTitle'),
                style: theme.textTheme.headlineSmall,
                textAlign: TextAlign.center,
              ),

              const SizedBox(height: AppSpacing.sm),

              Text(
                l10n.t('checkoutSuccessBody'),
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: theme.textTheme.bodySmall?.color,
                ),
                textAlign: TextAlign.center,
              ),

              const SizedBox(height: AppSpacing.xxl),

              // The order number is a guest's ONLY handle on this order, so it
              // is shown prominently and made copyable.
              Container(
                padding: const EdgeInsets.all(AppSpacing.lg),
                decoration: BoxDecoration(
                  color: theme.cardColor,
                  borderRadius: AppRadius.cardRadius,
                  border: Border.all(color: theme.dividerColor),
                ),
                child: Column(
                  children: [
                    Text(
                      l10n.t('checkoutOrderNumber'),
                      style: theme.textTheme.labelMedium?.copyWith(
                        color: theme.textTheme.bodySmall?.color,
                      ),
                    ),
                    const SizedBox(height: AppSpacing.xs),
                    Row(
                      mainAxisAlignment: MainAxisAlignment.center,
                      children: [
                        Flexible(
                          child: SelectableText(
                            orderNumber,
                            style: theme.textTheme.titleLarge?.copyWith(
                              color: theme.colorScheme.primary,
                              fontFeatures: const [
                                FontFeature.tabularFigures(),
                              ],
                            ),
                            // Order numbers are alphanumeric identifiers and
                            // read LTR even inside an Arabic layout.
                            textDirection: TextDirection.ltr,
                          ),
                        ),
                        const SizedBox(width: AppSpacing.sm),
                        IconButton(
                          onPressed: () => _copy(context),
                          icon: const Icon(Icons.copy, size: 18),
                          tooltip: l10n.t('checkoutCopyNumber'),
                        ),
                      ],
                    ),
                    const SizedBox(height: AppSpacing.sm),
                    Text(
                      l10n.t('checkoutSaveNumberHint'),
                      style: theme.textTheme.bodySmall,
                      textAlign: TextAlign.center,
                    ),
                  ],
                ),
              ),

              const SizedBox(height: AppSpacing.xxl),

              FilledButton(
                onPressed: () => context.go(
                  Uri(
                    path: Routes.trackOrder,
                    queryParameters: {'number': orderNumber},
                  ).toString(),
                ),
                child: Text(l10n.t('orderTrackTitle')),
              ),

              const SizedBox(height: AppSpacing.md),

              OutlinedButton(
                onPressed: () => context.go(Routes.home),
                child: Text(l10n.t('checkoutContinueShopping')),
              ),
            ],
          ),
        ),
      ),
    );
  }

  void _copy(BuildContext context) {
    final l10n = context.l10n;

    Clipboard.setData(ClipboardData(text: orderNumber));

    ScaffoldMessenger.of(context)
      ..hideCurrentSnackBar()
      ..showSnackBar(
        SnackBar(content: Text(l10n.t('checkoutNumberCopied'))),
      );
  }
}
