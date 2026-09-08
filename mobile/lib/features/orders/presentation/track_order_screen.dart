import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/l10n/app_localizations.dart';
import '../../../core/theme/app_spacing.dart';
import '../../../core/utils/validators.dart';
import '../../../core/widgets/state_views.dart';
import 'order_detail_screen.dart';
import 'orders_controller.dart';

/// Guest order tracking.
///
/// Public by design — a customer who checked out without an account must still
/// be able to follow their order. The phone-digits field is a mandatory second
/// factor: an order number alone is guessable, and without it this screen
/// would expose any customer's name, address and purchases.
class TrackOrderScreen extends ConsumerStatefulWidget {
  const TrackOrderScreen({super.key, this.initialOrderNumber});

  final String? initialOrderNumber;

  @override
  ConsumerState<TrackOrderScreen> createState() => _TrackOrderScreenState();
}

class _TrackOrderScreenState extends ConsumerState<TrackOrderScreen> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _numberController;
  final _phoneController = TextEditingController();

  /// The submitted pair. Null until the form has been submitted, so the screen
  /// does not fire a lookup on every keystroke.
  ({String orderNumber, String phoneLast4})? _query;

  @override
  void initState() {
    super.initState();
    _numberController =
        TextEditingController(text: widget.initialOrderNumber ?? '');
  }

  @override
  void dispose() {
    _numberController.dispose();
    _phoneController.dispose();
    super.dispose();
  }

  void _submit() {
    if (!(_formKey.currentState?.validate() ?? false)) return;

    FocusScope.of(context).unfocus();

    setState(() {
      _query = (
        orderNumber: _numberController.text.trim(),
        phoneLast4: _phoneController.text.trim(),
      );
    });
  }

  @override
  Widget build(BuildContext context) {
    final l10n = context.l10n;
    final theme = Theme.of(context);

    return Scaffold(
      appBar: AppBar(title: Text(l10n.t('orderTrackTitle'))),
      body: ListView(
        padding: const EdgeInsets.all(AppSpacing.lg),
        children: [
          Text(
            l10n.t('orderTrackHint'),
            style: theme.textTheme.bodyMedium?.copyWith(
              color: theme.textTheme.bodySmall?.color,
            ),
          ),
          const SizedBox(height: AppSpacing.lg),

          Form(
            key: _formKey,
            child: Column(
              children: [
                TextFormField(
                  controller: _numberController,
                  textInputAction: TextInputAction.next,
                  // Order numbers are alphanumeric identifiers and read LTR
                  // even in an Arabic layout.
                  textDirection: TextDirection.ltr,
                  decoration: InputDecoration(
                    labelText: l10n.t('orderTrackNumberField'),
                    prefixIcon: const Icon(Icons.receipt_long_outlined),
                  ),
                  validator: (value) =>
                      Validators.minLength(value, 4, l10n),
                ),
                const SizedBox(height: AppSpacing.md),
                TextFormField(
                  controller: _phoneController,
                  keyboardType: TextInputType.number,
                  textInputAction: TextInputAction.done,
                  textDirection: TextDirection.ltr,
                  maxLength: 4,
                  inputFormatters: [
                    FilteringTextInputFormatter.digitsOnly,
                  ],
                  decoration: InputDecoration(
                    labelText: l10n.t('orderTrackPhoneField'),
                    prefixIcon: const Icon(Icons.phone_outlined),
                    counterText: '',
                  ),
                  validator: (value) =>
                      Validators.phoneLast4(value, l10n),
                  onFieldSubmitted: (_) => _submit(),
                ),
                const SizedBox(height: AppSpacing.lg),
                FilledButton(
                  onPressed: _submit,
                  child: Text(l10n.t('orderTrackSubmit')),
                ),
              ],
            ),
          ),

          const SizedBox(height: AppSpacing.xl),

          if (_query != null) _Result(query: _query!),
        ],
      ),
    );
  }
}

class _Result extends ConsumerWidget {
  const _Result({required this.query});

  final ({String orderNumber, String phoneLast4}) query;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = context.l10n;
    final order = ref.watch(trackedOrderProvider(query));

    return order.when(
      loading: () => const Padding(
        padding: EdgeInsets.all(AppSpacing.xxl),
        child: LoadingView(),
      ),
      error: (error, _) => EmptyState(
        icon: Icons.search_off,
        // "Not found" is shown for both a wrong number and wrong digits: the
        // server returns an identical 404 for each so it cannot be used to
        // confirm which order numbers are real.
        title: l10n.t('orderTrackNotFound'),
        compact: true,
      ),
      data: (data) => Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          const Divider(),
          const SizedBox(height: AppSpacing.md),
          // Reuses the signed-in detail view, with cancelling disabled: the
          // phone-digits factor is not strong enough to authorise destroying
          // an order.
          Flexible(
            child: OrderDetailView(order: data, canCancel: false),
          ),
        ],
      ),
    );
  }
}
