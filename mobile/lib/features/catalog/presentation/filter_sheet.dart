import 'package:decimal/decimal.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../../core/l10n/app_localizations.dart';
import '../../../core/theme/app_spacing.dart';
import '../domain/catalog_filter.dart';
import 'catalog_controller.dart';

/// Filter bottom sheet.
///
/// Edits a LOCAL copy of the filter and returns it only on "apply". Applying
/// each toggle immediately would fire a network request per tap and make the
/// grid flicker underneath the sheet.
class FilterSheet extends ConsumerStatefulWidget {
  const FilterSheet({super.key, required this.initialFilter});

  final CatalogFilter initialFilter;

  @override
  ConsumerState<FilterSheet> createState() => _FilterSheetState();
}

class _FilterSheetState extends ConsumerState<FilterSheet> {
  late CatalogFilter _draft;
  late final TextEditingController _minController;
  late final TextEditingController _maxController;

  @override
  void initState() {
    super.initState();
    _draft = widget.initialFilter;
    _minController = TextEditingController(
      text: _draft.minPrice?.toString() ?? '',
    );
    _maxController = TextEditingController(
      text: _draft.maxPrice?.toString() ?? '',
    );
  }

  @override
  void dispose() {
    _minController.dispose();
    _maxController.dispose();
    super.dispose();
  }

  void _commitPriceRange() {
    final min = Decimal.tryParse(_minController.text.trim());
    final max = Decimal.tryParse(_maxController.text.trim());

    // Swap inverted bounds rather than rejecting them: a user who types
    // 500–100 clearly means 100–500, and an error message here is friction
    // for no benefit.
    final (low, high) = (min != null && max != null && min > max)
        ? (max, min)
        : (min, max);

    _draft = _draft.copyWith(
      minPrice: low,
      maxPrice: high,
      clearPriceRange: low == null && high == null,
    );
  }

  @override
  Widget build(BuildContext context) {
    final l10n = context.l10n;
    final theme = Theme.of(context);

    // Attributes are only offered when a single category is in scope: values
    // like "شاشة 6.5 بوصة" are meaningless across the whole catalogue.
    final categoryId = _draft.categoryId;
    final attributes = categoryId == null
        ? null
        : ref.watch(categoryAttributesProvider(categoryId));

    return DraggableScrollableSheet(
      initialChildSize: 0.75,
      minChildSize: 0.4,
      maxChildSize: 0.95,
      expand: false,
      builder: (context, scrollController) => Column(
        children: [
          Padding(
            padding: const EdgeInsets.fromLTRB(
              AppSpacing.lg,
              AppSpacing.sm,
              AppSpacing.lg,
              AppSpacing.sm,
            ),
            child: Row(
              children: [
                Expanded(
                  child: Text(
                    l10n.t('catalogFilters'),
                    style: theme.textTheme.titleLarge,
                  ),
                ),
                if (!_draft.isEmpty)
                  TextButton(
                    onPressed: () {
                      setState(() {
                        _draft = _draft.clearFacets();
                        _minController.clear();
                        _maxController.clear();
                      });
                    },
                    child: Text(l10n.t('reset')),
                  ),
              ],
            ),
          ),
          const Divider(height: 1),

          Expanded(
            child: ListView(
              controller: scrollController,
              padding: AppSpacing.sheet,
              children: [
                _SectionTitle(l10n.t('filterPriceRange')),
                Row(
                  children: [
                    Expanded(
                      child: _PriceField(
                        controller: _minController,
                        label: l10n.t('filterMin'),
                      ),
                    ),
                    const SizedBox(width: AppSpacing.md),
                    Expanded(
                      child: _PriceField(
                        controller: _maxController,
                        label: l10n.t('filterMax'),
                      ),
                    ),
                  ],
                ),

                const SizedBox(height: AppSpacing.xl),

                SwitchListTile(
                  value: _draft.inStockOnly,
                  onChanged: (value) => setState(
                    () => _draft = _draft.copyWith(inStockOnly: value),
                  ),
                  title: Text(l10n.t('filterInStockOnly')),
                  contentPadding: EdgeInsets.zero,
                ),
                SwitchListTile(
                  value: _draft.onSaleOnly,
                  onChanged: (value) => setState(
                    () => _draft = _draft.copyWith(onSaleOnly: value),
                  ),
                  title: Text(l10n.t('filterOnSaleOnly')),
                  contentPadding: EdgeInsets.zero,
                ),

                if (attributes != null)
                  attributes.when(
                    // Attributes are secondary: a failure to load them must
                    // not block the rest of the sheet, so both loading and
                    // error render as nothing.
                    loading: () => const Padding(
                      padding: EdgeInsets.symmetric(vertical: AppSpacing.lg),
                      child: Center(
                        child: SizedBox(
                          width: 20,
                          height: 20,
                          child: CircularProgressIndicator(strokeWidth: 2),
                        ),
                      ),
                    ),
                    error: (_, __) => const SizedBox.shrink(),
                    data: (groups) => Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        for (final group in groups) ...[
                          const SizedBox(height: AppSpacing.lg),
                          _SectionTitle(group.label(l10n.languageCode)),
                          Wrap(
                            spacing: AppSpacing.sm,
                            runSpacing: AppSpacing.sm,
                            children: [
                              for (final option in group.options)
                                FilterChip(
                                  selected: _draft.attributeValueIds
                                      .contains(option.valueId),
                                  onSelected: (_) => setState(() {
                                    _draft = _draft
                                        .toggleAttributeValue(option.valueId);
                                  }),
                                  label: Text(
                                    '${option.label(l10n.languageCode)}'
                                    ' (${option.count})',
                                  ),
                                ),
                            ],
                          ),
                        ],
                      ],
                    ),
                  ),
              ],
            ),
          ),

          // Sticky footer so "apply" is always reachable without scrolling to
          // the bottom of a long attribute list.
          SafeArea(
            child: Padding(
              padding: const EdgeInsets.all(AppSpacing.lg),
              child: FilledButton(
                onPressed: () {
                  _commitPriceRange();
                  Navigator.of(context).pop(_draft);
                },
                child: Text(l10n.t('apply')),
              ),
            ),
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
      padding: const EdgeInsets.only(bottom: AppSpacing.sm),
      child: Text(title, style: Theme.of(context).textTheme.titleSmall),
    );
  }
}

class _PriceField extends StatelessWidget {
  const _PriceField({required this.controller, required this.label});

  final TextEditingController controller;
  final String label;

  @override
  Widget build(BuildContext context) {
    return TextField(
      controller: controller,
      keyboardType: const TextInputType.numberWithOptions(decimal: true),
      // Only digits and a single separator reach the field, so
      // Decimal.tryParse cannot fail on stray characters.
      inputFormatters: [
        FilteringTextInputFormatter.allow(RegExp(r'[0-9.]')),
      ],
      textDirection: TextDirection.ltr,
      decoration: InputDecoration(labelText: label),
    );
  }
}
