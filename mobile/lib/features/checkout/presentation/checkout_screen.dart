import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

import '../../../core/l10n/app_localizations.dart';
import '../../../core/theme/app_colors.dart';
import '../../../core/theme/app_spacing.dart';
import '../../../core/utils/formatters.dart';
import '../../../core/utils/validators.dart';
import '../../../core/widgets/state_views.dart';
import '../../../routing/routes.dart';
import '../../auth/presentation/auth_controller.dart';
import '../../cart/domain/cart_validation.dart';
import '../../cart/presentation/cart_controller.dart';
import 'checkout_controller.dart';

class CheckoutScreen extends ConsumerStatefulWidget {
  const CheckoutScreen({super.key});

  @override
  ConsumerState<CheckoutScreen> createState() => _CheckoutScreenState();
}

class _CheckoutScreenState extends ConsumerState<CheckoutScreen> {
  final _formKey = GlobalKey<FormState>();

  final _nameController = TextEditingController();
  final _phoneController = TextEditingController();
  final _emailController = TextEditingController();
  final _addressController = TextEditingController();
  final _cityController = TextEditingController();
  final _governorateController = TextEditingController();
  final _notesController = TextEditingController();

  bool _prefilled = false;

  @override
  void dispose() {
    _nameController.dispose();
    _phoneController.dispose();
    _emailController.dispose();
    _addressController.dispose();
    _cityController.dispose();
    _governorateController.dispose();
    _notesController.dispose();
    super.dispose();
  }

  /// Prefills from the signed-in profile.
  ///
  /// Done once, and only into empty fields, so a re-render (or the profile
  /// loading late) never overwrites something the user has already typed.
  void _prefillFromProfile() {
    if (_prefilled) return;

    final user = ref.read(currentUserProvider);
    if (user == null) return;

    _nameController.text = user.fullName ?? '';
    _phoneController.text = user.phone ?? '';
    _emailController.text = user.email;
    _addressController.text = user.defaultAddress ?? '';
    _cityController.text = user.city ?? '';
    _governorateController.text = user.governorate ?? '';
    _prefilled = true;
  }

  @override
  Widget build(BuildContext context) {
    final l10n = context.l10n;
    final cart = ref.watch(cartControllerProvider);
    final checkout = ref.watch(checkoutControllerProvider);

    _prefillFromProfile();

    // React to terminal states. Done in a listener rather than in build so
    // navigation is not triggered during a rebuild.
    ref.listen<CheckoutState>(checkoutControllerProvider, (previous, next) {
      switch (next) {
        case CheckoutSuccess(:final result):
          context.go(Routes.orderConfirmation(result.orderNumber));
        case CheckoutNeedsReview(:final validation):
          _showReviewSheet(validation);
        case CheckoutFailure(:final error):
          _showError(error);
        default:
          break;
      }
    });

    if (cart.isEmpty && checkout is! CheckoutSubmitting) {
      return Scaffold(
        appBar: AppBar(title: Text(l10n.t('checkoutTitle'))),
        body: EmptyState(
          emoji: '🛒',
          title: l10n.t('cartEmpty'),
          actionLabel: l10n.t('cartStartShopping'),
          onAction: () => context.go(Routes.catalog),
        ),
      );
    }

    final isSubmitting = checkout is CheckoutSubmitting;

    return Scaffold(
      appBar: AppBar(title: Text(l10n.t('checkoutTitle'))),
      body: AbsorbPointer(
        // Blocks the whole form while in flight: editing a field mid-submit
        // would send data that does not match what was validated.
        absorbing: isSubmitting,
        child: Form(
          key: _formKey,
          child: ListView(
            padding: const EdgeInsets.all(AppSpacing.lg),
            children: [
              _SectionTitle(l10n.t('checkoutContactInfo')),
              TextFormField(
                controller: _nameController,
                textInputAction: TextInputAction.next,
                decoration: InputDecoration(
                  labelText: l10n.t('fieldFullName'),
                ),
                validator: (value) =>
                    Validators.minLength(value, 3, l10n),
              ),
              const SizedBox(height: AppSpacing.md),
              TextFormField(
                controller: _phoneController,
                keyboardType: TextInputType.phone,
                textInputAction: TextInputAction.next,
                textDirection: TextDirection.ltr,
                inputFormatters: [
                  FilteringTextInputFormatter.allow(RegExp(r'[0-9+ ]')),
                ],
                decoration: InputDecoration(
                  labelText: l10n.t('fieldPhone'),
                  hintText: '01xxxxxxxxx',
                ),
                validator: (value) => Validators.phone(value, l10n),
              ),
              const SizedBox(height: AppSpacing.md),
              TextFormField(
                controller: _emailController,
                keyboardType: TextInputType.emailAddress,
                textInputAction: TextInputAction.next,
                textDirection: TextDirection.ltr,
                decoration: InputDecoration(
                  labelText:
                      '${l10n.t('fieldEmail')} ${l10n.t('optional')}',
                ),
                // Optional: the store contacts customers by phone, so an
                // email is a convenience rather than a requirement.
                validator: (value) =>
                    Validators.email(value, l10n, isRequired: false),
              ),

              const SizedBox(height: AppSpacing.xl),

              _SectionTitle(l10n.t('checkoutShippingInfo')),
              TextFormField(
                controller: _addressController,
                textInputAction: TextInputAction.next,
                maxLines: 2,
                decoration: InputDecoration(
                  labelText: l10n.t('fieldAddress'),
                ),
                validator: (value) =>
                    Validators.minLength(value, 10, l10n),
              ),
              const SizedBox(height: AppSpacing.md),
              Row(
                children: [
                  Expanded(
                    child: TextFormField(
                      controller: _cityController,
                      textInputAction: TextInputAction.next,
                      decoration: InputDecoration(
                        labelText: l10n.t('fieldCity'),
                      ),
                    ),
                  ),
                  const SizedBox(width: AppSpacing.md),
                  Expanded(
                    child: TextFormField(
                      controller: _governorateController,
                      textInputAction: TextInputAction.next,
                      decoration: InputDecoration(
                        labelText: l10n.t('fieldGovernorate'),
                      ),
                    ),
                  ),
                ],
              ),
              const SizedBox(height: AppSpacing.md),
              TextFormField(
                controller: _notesController,
                maxLines: 3,
                decoration: InputDecoration(
                  labelText:
                      '${l10n.t('fieldNotes')} ${l10n.t('optional')}',
                ),
              ),

              const SizedBox(height: AppSpacing.xl),

              _SectionTitle(l10n.t('checkoutOrderSummary')),
              _Summary(),

              const SizedBox(height: AppSpacing.lg),

              Container(
                padding: const EdgeInsets.all(AppSpacing.md),
                decoration: BoxDecoration(
                  color: AppColors.primarySurface,
                  borderRadius: AppRadius.cardRadius,
                ),
                child: Row(
                  children: [
                    const Icon(
                      Icons.payments_outlined,
                      color: AppColors.primaryDark,
                      size: 20,
                    ),
                    const SizedBox(width: AppSpacing.sm),
                    Expanded(
                      child: Text(
                        l10n.t('checkoutPaymentCod'),
                        style: Theme.of(context)
                            .textTheme
                            .labelMedium
                            ?.copyWith(color: AppColors.primaryDark),
                      ),
                    ),
                  ],
                ),
              ),

              const SizedBox(height: AppSpacing.xxxl),
            ],
          ),
        ),
      ),
      bottomNavigationBar: _SubmitBar(
        isSubmitting: isSubmitting,
        onSubmit: () => _submit(),
      ),
    );
  }

  Future<void> _submit({bool acceptChanges = false}) async {
    if (!(_formKey.currentState?.validate() ?? false)) return;

    FocusScope.of(context).unfocus();

    await ref.read(checkoutControllerProvider.notifier).submit(
          customerName: _nameController.text,
          // Normalised so a number typed as "+20 100…" matches the stored
          // format and guest tracking can find the order later.
          customerPhone: Validators.normalisePhone(_phoneController.text),
          address: _addressController.text,
          customerEmail: _emailController.text.trim().isEmpty
              ? null
              : _emailController.text,
          city: _cityController.text,
          governorate: _governorateController.text,
          notes: _notesController.text,
          acceptChanges: acceptChanges,
        );
  }

  /// Shows what changed and asks the user to accept before re-submitting.
  Future<void> _showReviewSheet(CartValidation validation) async {
    final l10n = context.l10n;

    final accepted = await showModalBottomSheet<bool>(
      context: context,
      isScrollControlled: true,
      builder: (context) => SafeArea(
        child: Padding(
          padding: AppSpacing.sheet,
          child: Column(
            mainAxisSize: MainAxisSize.min,
            crossAxisAlignment: CrossAxisAlignment.start,
            children: [
              Row(
                children: [
                  const Icon(
                    Icons.warning_amber_rounded,
                    color: AppColors.warning,
                  ),
                  const SizedBox(width: AppSpacing.sm),
                  Expanded(
                    child: Text(
                      l10n.t('cartStockAdjusted'),
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                  ),
                ],
              ),
              const SizedBox(height: AppSpacing.lg),
              // The server's message is shown verbatim: it is already
              // localized and states exactly what changed for that line.
              for (final issue in validation.issues)
                Padding(
                  padding: const EdgeInsets.only(bottom: AppSpacing.sm),
                  child: Row(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text('• '),
                      Expanded(
                        child: Text(
                          issue.message,
                          style: Theme.of(context).textTheme.bodyMedium,
                        ),
                      ),
                    ],
                  ),
                ),
              const SizedBox(height: AppSpacing.lg),
              Row(
                children: [
                  Expanded(
                    child: OutlinedButton(
                      onPressed: () => Navigator.of(context).pop(false),
                      child: Text(l10n.t('cancel')),
                    ),
                  ),
                  const SizedBox(width: AppSpacing.md),
                  Expanded(
                    child: FilledButton(
                      onPressed: () => Navigator.of(context).pop(true),
                      child: Text(l10n.t('confirm')),
                    ),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );

    if (!mounted) return;

    ref.read(checkoutControllerProvider.notifier).reset();

    if (accepted == true) {
      // Re-submit with the changes accepted; the same idempotency key is
      // reused by the controller.
      await _submit(acceptChanges: true);
    } else {
      // Declined — send them back to the cart to adjust it themselves.
      if (mounted) context.go(Routes.cart);
    }
  }

  void _showError(Object error) {
    final l10n = context.l10n;

    ref.read(checkoutControllerProvider.notifier).reset();

    showDialog<void>(
      context: context,
      builder: (context) => AlertDialog(
        title: Text(l10n.t('errorGeneric')),
        content: ErrorView(error: error, compact: true),
        actions: [
          TextButton(
            onPressed: () => Navigator.of(context).pop(),
            child: Text(l10n.t('ok')),
          ),
        ],
      ),
    );
  }
}

class _SectionTitle extends StatelessWidget {
  const _SectionTitle(this.title);

  final String title;

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.only(bottom: AppSpacing.md),
      child: Text(title, style: Theme.of(context).textTheme.titleMedium),
    );
  }
}

class _Summary extends ConsumerWidget {
  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final l10n = context.l10n;
    final cart = ref.watch(cartControllerProvider);
    final lang = l10n.languageCode;

    return Container(
      padding: const EdgeInsets.all(AppSpacing.lg),
      decoration: BoxDecoration(
        color: theme.cardColor,
        borderRadius: AppRadius.cardRadius,
        border: Border.all(color: theme.dividerColor),
      ),
      child: Column(
        children: [
          for (final item in cart.items)
            Padding(
              padding: const EdgeInsets.only(bottom: AppSpacing.sm),
              child: Row(
                children: [
                  Text(
                    '${item.quantity}×',
                    style: theme.textTheme.labelMedium?.copyWith(
                      color: theme.colorScheme.primary,
                    ),
                  ),
                  const SizedBox(width: AppSpacing.sm),
                  Expanded(
                    child: Text(
                      item.productName,
                      style: theme.textTheme.bodySmall,
                      maxLines: 1,
                      overflow: TextOverflow.ellipsis,
                    ),
                  ),
                  Text(
                    Formatters.price(item.subTotal, lang: lang),
                    style: theme.textTheme.bodySmall,
                  ),
                ],
              ),
            ),
          const Divider(height: AppSpacing.xl),
          Row(
            children: [
              Expanded(
                child: Text(
                  l10n.t('cartSubtotal'),
                  style: theme.textTheme.bodyMedium,
                ),
              ),
              Text(
                Formatters.price(cart.subTotal, lang: lang),
                style: theme.textTheme.bodyMedium,
              ),
            ],
          ),
          const SizedBox(height: AppSpacing.xs),
          Row(
            children: [
              Expanded(
                child: Text(
                  l10n.t('cartShipping'),
                  style: theme.textTheme.bodyMedium,
                ),
              ),
              Text(
                cart.hasFreeShipping
                    ? l10n.t('cartShippingFree')
                    : Formatters.price(cart.shippingFee, lang: lang),
                style: theme.textTheme.bodyMedium?.copyWith(
                  color: cart.hasFreeShipping ? AppColors.success : null,
                ),
              ),
            ],
          ),
          const Divider(height: AppSpacing.xl),
          Row(
            children: [
              Expanded(
                child: Text(
                  l10n.t('cartTotal'),
                  style: theme.textTheme.titleMedium,
                ),
              ),
              Text(
                Formatters.price(cart.total, lang: lang),
                style: theme.textTheme.titleMedium?.copyWith(
                  color: theme.colorScheme.primary,
                ),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class _SubmitBar extends ConsumerWidget {
  const _SubmitBar({required this.isSubmitting, required this.onSubmit});

  final bool isSubmitting;
  final VoidCallback onSubmit;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final theme = Theme.of(context);
    final l10n = context.l10n;
    final cart = ref.watch(cartControllerProvider);

    return Container(
      decoration: BoxDecoration(
        color: theme.cardColor,
        boxShadow: AppShadows.bottomBar,
      ),
      child: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(AppSpacing.lg),
          child: FilledButton(
            onPressed: isSubmitting ? null : onSubmit,
            child: isSubmitting
                ? const SizedBox(
                    width: 22,
                    height: 22,
                    child: CircularProgressIndicator(
                      strokeWidth: 2,
                      color: Colors.white,
                    ),
                  )
                : Text(
                    '${l10n.t('checkoutPlaceOrder')} · '
                    '${Formatters.price(cart.total, lang: l10n.languageCode)}',
                  ),
          ),
        ),
      ),
    );
  }
}
