import 'package:decimal/decimal.dart';
import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:unishop_app/core/l10n/app_localizations.dart';
import 'package:unishop_app/core/theme/app_spacing.dart';
import 'package:unishop_app/core/theme/app_theme.dart';
import 'package:unishop_app/core/widgets/price_tag.dart';
import 'package:unishop_app/core/widgets/product_card.dart';
import 'package:unishop_app/core/widgets/state_views.dart';
import 'package:unishop_app/features/catalog/domain/product.dart';

/// Wraps a widget with everything it needs: theme, localization and — most
/// importantly — the locale that drives text direction.
Widget _wrap(Widget child, {String lang = 'ar'}) {
  return MaterialApp(
    locale: Locale(lang),
    supportedLocales: AppLocalizations.supportedLocales,
    localizationsDelegates: const [
      AppLocalizations.delegate,
      GlobalMaterialLocalizations.delegate,
      GlobalWidgetsLocalizations.delegate,
      GlobalCupertinoLocalizations.delegate,
    ],
    theme: AppTheme.light(),
    home: Scaffold(
      body: SizedBox(width: 180, height: 300, child: child),
    ),
  );
}

Product _product({
  bool inStock = true,
  bool lowStock = false,
  String price = '1500',
  String? oldPrice,
  int? discountPercent,
  String? badge,
  String name = 'آيفون 15 برو ماكس 256 جيجا',
}) {
  return Product(
    id: 1,
    slug: 'iphone-15',
    name: name,
    price: Decimal.parse(price),
    oldPrice: oldPrice == null ? null : Decimal.parse(oldPrice),
    discountPercent: discountPercent,
    badge: badge,
    brandName: 'Apple',
    inStock: inStock,
    lowStock: lowStock,
    isFeatured: false,
  );
}

void main() {
  group('ProductCard', () {
    testWidgets('renders name, brand and price', (tester) async {
      await tester.pumpWidget(_wrap(ProductCard(product: _product())));

      expect(find.text('آيفون 15 برو ماكس 256 جيجا'), findsOneWidget);
      expect(find.text('Apple'), findsOneWidget);
      expect(find.textContaining('1,500'), findsOneWidget);
    });

    testWidgets('shows the Arabic availability label', (tester) async {
      await tester.pumpWidget(_wrap(ProductCard(product: _product())));
      expect(find.text('متوفر'), findsOneWidget);

      await tester.pumpWidget(
        _wrap(ProductCard(product: _product(lowStock: true))),
      );
      expect(find.text('كمية محدودة'), findsOneWidget);

      await tester.pumpWidget(
        _wrap(ProductCard(product: _product(inStock: false))),
      );
      expect(find.text('غير متوفر'), findsOneWidget);
    });

    testWidgets('never exposes a raw stock number', (tester) async {
      // The list DTO deliberately omits Stock; this asserts the widget cannot
      // reintroduce it.
      await tester.pumpWidget(
        _wrap(ProductCard(product: _product(lowStock: true))),
      );

      expect(find.textContaining(RegExp(r'\b\d+ (في المخزون|left)\b')),
          findsNothing);
    });

    testWidgets('disables rather than hides add-to-cart when out of stock',
        (tester) async {
      // A missing button reads as a rendering bug; a disabled one explains.
      var tapped = false;

      await tester.pumpWidget(
        _wrap(
          ProductCard(
            product: _product(inStock: false),
            onAddToCart: () => tapped = true,
          ),
        ),
      );

      final button = find.byIcon(Icons.add_shopping_cart_outlined);
      expect(button, findsOneWidget);

      await tester.tap(button, warnIfMissed: false);
      await tester.pump();

      expect(tapped, isFalse);
    });

    testWidgets('invokes onAddToCart when in stock', (tester) async {
      var tapped = false;

      await tester.pumpWidget(
        _wrap(
          ProductCard(
            product: _product(),
            onAddToCart: () => tapped = true,
          ),
        ),
      );

      await tester.tap(find.byIcon(Icons.add_shopping_cart_outlined));
      await tester.pump();

      expect(tapped, isTrue);
    });

    testWidgets('shows the cart quantity instead of the add icon',
        (tester) async {
      await tester.pumpWidget(
        _wrap(
          ProductCard(
            product: _product(),
            onAddToCart: () {},
            quantityInCart: 3,
          ),
        ),
      );

      expect(find.text('3'), findsOneWidget);
      expect(find.byIcon(Icons.add_shopping_cart_outlined), findsNothing);
    });

    testWidgets('renders the discount badge only for a genuine sale',
        (tester) async {
      await tester.pumpWidget(
        _wrap(
          ProductCard(
            product: _product(
              price: '1200',
              oldPrice: '1500',
              discountPercent: 20,
            ),
          ),
        ),
      );
      expect(find.byType(DiscountBadge), findsOneWidget);

      // Stale old price with no computed discount: no badge.
      await tester.pumpWidget(
        _wrap(
          ProductCard(
            product: _product(price: '1500', oldPrice: '1500'),
          ),
        ),
      );
      expect(find.byType(DiscountBadge), findsNothing);
    });

    testWidgets('wishlist toggle reflects and reports state', (tester) async {
      var toggled = false;

      await tester.pumpWidget(
        _wrap(
          ProductCard(
            product: _product(),
            isInWishlist: true,
            onToggleWishlist: () => toggled = true,
          ),
        ),
      );

      expect(find.byIcon(Icons.favorite), findsOneWidget);

      await tester.tap(find.byIcon(Icons.favorite));
      await tester.pump();

      expect(toggled, isTrue);
    });
  });

  group('RTL layout', () {
    testWidgets('Arabic renders right-to-left', (tester) async {
      await tester.pumpWidget(_wrap(ProductCard(product: _product())));

      final direction = Directionality.of(
        tester.element(find.byType(ProductCard)),
      );
      expect(direction, TextDirection.rtl);
    });

    testWidgets('English renders left-to-right', (tester) async {
      await tester.pumpWidget(
        _wrap(ProductCard(product: _product()), lang: 'en'),
      );

      final direction = Directionality.of(
        tester.element(find.byType(ProductCard)),
      );
      expect(direction, TextDirection.ltr);
    });

    testWidgets('the discount badge mirrors to the other edge in RTL',
        (tester) async {
      // PositionedDirectional must resolve `start` to the right in Arabic;
      // hardcoded `left` would leave the badge on the wrong corner.
      final product = _product(
        price: '1200',
        oldPrice: '1500',
        discountPercent: 20,
      );

      await tester.pumpWidget(_wrap(ProductCard(product: product)));
      final rtlX = tester.getTopLeft(find.byType(DiscountBadge)).dx;

      await tester.pumpWidget(
        _wrap(ProductCard(product: product), lang: 'en'),
      );
      final ltrX = tester.getTopLeft(find.byType(DiscountBadge)).dx;

      expect(
        rtlX,
        greaterThan(ltrX),
        reason: 'in RTL the badge must sit further right than in LTR',
      );
    });

    testWidgets('currency symbol side follows the language', (tester) async {
      await tester.pumpWidget(
        _wrap(PriceTag(price: Decimal.parse('1500'))),
      );
      expect(find.text('1,500 ج.م'), findsOneWidget);

      await tester.pumpWidget(
        _wrap(PriceTag(price: Decimal.parse('1500')), lang: 'en'),
      );
      expect(find.text('EGP 1,500'), findsOneWidget);
    });
  });

  group('State views', () {
    testWidgets('EmptyState shows its action only when provided',
        (tester) async {
      await tester.pumpWidget(
        _wrap(
          const EmptyState(title: 'سلتك فارغة', message: 'أضف منتجات'),
        ),
      );

      expect(find.text('سلتك فارغة'), findsOneWidget);
      expect(find.byType(FilledButton), findsNothing);

      await tester.pumpWidget(
        _wrap(
          EmptyState(
            title: 'سلتك فارغة',
            actionLabel: 'ابدأ التسوق',
            onAction: () {},
          ),
        ),
      );

      expect(find.text('ابدأ التسوق'), findsOneWidget);
    });
  });
  // ── Card sizing ───────────────────────────────────────────────────────────
  // These exist because every other test PASSED while the card was visibly
  // clipping text on a real device. `Expanded` absorbed the shortfall, so no
  // overflow warning fired and the layout reported itself valid while the
  // second line of long product names was cut through the middle.
  //
  // The assertion is on the HEIGHT HANDED TO THE NAME, not on
  // didExceedMaxLines. Under flutter_test the default font renders every glyph
  // as a fixed-width square, so line counts here bear no relation to Cairo on
  // a device — but the layout BUDGET is font-independent and is what actually
  // broke.
  group('Card sizing', () {
    // titleSmall is 14px with height 1.4, so two lines need 39.2dp.
    const twoLines = 2 * 14 * 1.4;

    Future<double> nameHeightAt(WidgetTester tester, double width,
        {double? cardHeight}) async {
      const name = 'Anker Z87 Sport Smart Watch';
      await tester.pumpWidget(
        MaterialApp(
          locale: const Locale('ar'),
          localizationsDelegates: const [
            AppLocalizations.delegate,
            GlobalMaterialLocalizations.delegate,
            GlobalWidgetsLocalizations.delegate,
            GlobalCupertinoLocalizations.delegate,
          ],
          theme: AppTheme.light(),
          home: Scaffold(
            body: Center(
              child: SizedBox(
                width: width,
                height: cardHeight ?? AppSizes.productCardHeight(width),
                child: ProductCard(product: _product(name: name)),
              ),
            ),
          ),
        ),
      );
      await tester.pumpAndSettle();
      return tester.getSize(find.text(name)).height;
    }

    testWidgets('the name gets room for two full lines at every card width',
        (tester) async {
      // 160 is the home carousel width, ~183.5 the 2-column grid tile on a
      // 411dp phone, 200 a wider device. The carousel is the case a shared
      // childAspectRatio got most wrong, because height followed width.
      for (final width in <double>[160, 183.5, 200]) {
        final h = await nameHeightAt(tester, width);
        expect(h, greaterThanOrEqualTo(twoLines - 0.5),
            reason: 'at card width $width the name got ${h.toStringAsFixed(1)}dp, '
                'less than the ${twoLines.toStringAsFixed(1)}dp two lines need — '
                'the second line will be clipped');
        expect(tester.takeException(), isNull,
            reason: 'card overflowed at width $width');
      }
    });

    test('the card height budget covers everything the body renders',
        () {
      // Asserted arithmetically, not by pumping a widget. flutter_test
      // substitutes a font whose line height is 1.0, so two lines measure 28dp
      // in a widget test versus the 39.2dp Cairo needs on a device — the test
      // font physically cannot reproduce this bug, and two earlier attempts to
      // catch it by measuring rendered text both passed against the BROKEN
      // layout for exactly that reason.
      //
      // What can be checked reliably is the budget itself, in the same units
      // the production theme declares.
      const brand = 11 * 1.2;        // labelSmall
      const gapAfterBrand = 2.0;     // AppSpacing.xxs
      const nameTwoLines = 2 * 14 * 1.4; // titleSmall, the two lines required
      const availability = 11 * 1.2; // labelSmall
      const gapBeforePrice = 4.0;    // AppSpacing.xs
      const addButton = 36.0;        // _AddButton
      const padding = 8.0 * 2;       // EdgeInsets.all(AppSpacing.sm)

      const required = brand +
          gapAfterBrand +
          nameTwoLines +
          availability +
          gapBeforePrice +
          addButton +
          padding;

      expect(AppSizes.productCardBodyHeight, greaterThanOrEqualTo(required),
          reason: 'the body height is $required dp short of what the card '
              'renders, so the second line of long names will be clipped');

      // And the old width-derived ratio must genuinely fail this budget,
      // otherwise the change fixed nothing. At the 2-column grid width on a
      // 411dp phone a tile is ~183.5dp, and 183.5/0.62 left only
      // 183.5/0.62 - 183.5 = 112.5dp for a body needing 123.6dp.
      const gridTileWidth = 183.5;
      const oldBodyHeight = gridTileWidth / 0.62 - gridTileWidth;
      expect(oldBodyHeight, lessThan(required),
          reason: 'the old 0.62 ratio no longer under-allocates, so this test '
              'has stopped covering the regression it was written for');

      // The home carousel is narrower (160dp) and was therefore starved worse
      // by the same ratio — height followed width.
      const carouselWidth = 160.0;
      const oldCarouselBody = carouselWidth / 0.62 - carouselWidth;
      expect(oldCarouselBody, lessThan(oldBodyHeight),
          reason: 'the carousel should have been the worse case');

      // Both are correct now because height no longer depends on width.
      for (final width in <double>[carouselWidth, gridTileWidth, 200]) {
        expect(AppSizes.productCardHeight(width) - width,
            greaterThanOrEqualTo(required),
            reason: 'body starved at card width $width');
      }
    });
  });
}
