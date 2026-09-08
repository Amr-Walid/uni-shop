import 'package:flutter/material.dart';

/// Layout constants.
///
/// Centralised so a spacing change is one edit rather than a search across
/// every screen, and so RTL-safe directional insets are used consistently.
abstract final class AppSpacing {
  static const double xxs = 2;
  static const double xs = 4;
  static const double sm = 8;
  static const double md = 12;
  static const double lg = 16;
  static const double xl = 20;
  static const double xxl = 24;
  static const double xxxl = 32;

  /// Standard horizontal page padding.
  static const EdgeInsets page = EdgeInsets.symmetric(horizontal: lg);

  /// Padding for scrollable content that ends above a bottom nav bar.
  static const EdgeInsets pageWithBottomNav =
      EdgeInsets.fromLTRB(lg, lg, lg, 96);

  static const EdgeInsets card = EdgeInsets.all(md);
  static const EdgeInsets sheet = EdgeInsets.fromLTRB(lg, sm, lg, xxl);
}

/// Corner radii.
abstract final class AppRadius {
  static const double xs = 6;
  static const double sm = 10;
  static const double md = 14;
  static const double lg = 18;
  static const double xl = 24;

  /// Large enough to always render as a stadium regardless of widget height.
  static const double pill = 999;

  /// Cards use [xl] (24px) to match the website's `--r-xl: 1.5rem`, which is
  /// what `.product-card` renders with. The app previously used [md] (14px),
  /// and that difference in corner softness was one of the reasons the two
  /// surfaces read as different products side by side.
  static const BorderRadius cardRadius = BorderRadius.all(Radius.circular(xl));
  static const BorderRadius fieldRadius = BorderRadius.all(Radius.circular(sm));
  static const BorderRadius sheetRadius = BorderRadius.vertical(
    top: Radius.circular(xl),
  );
  static const BorderRadius pillRadius =
      BorderRadius.all(Radius.circular(pill));
}

/// Elevation replacements.
///
/// Material 3's tonal elevation tints surfaces with the primary colour, which
/// on a product grid reads as a blue wash. Soft explicit shadows are used
/// instead and `elevation: 0` is set on the components themselves.
abstract final class AppShadows {
  static const List<BoxShadow> card = [
    BoxShadow(
      color: Color(0x0F14181F),
      blurRadius: 12,
      offset: Offset(0, 4),
    ),
  ];

  static const List<BoxShadow> raised = [
    BoxShadow(
      color: Color(0x1A14181F),
      blurRadius: 20,
      offset: Offset(0, 8),
    ),
  ];

  /// Shadow cast upward by a bottom bar / sticky checkout footer.
  static const List<BoxShadow> bottomBar = [
    BoxShadow(
      color: Color(0x1414181F),
      blurRadius: 16,
      offset: Offset(0, -4),
    ),
  ];
}

/// Fixed sizes referenced from more than one screen.
abstract final class AppSizes {
  static const double buttonHeight = 52;
  static const double fieldHeight = 52;
  static const double appBarHeight = 56;
  static const double bottomNavHeight = 64;

  /// Product image aspect ratio. Matches the web catalogue's 1:1 crop, so the
  /// same uploaded asset is never letterboxed differently between platforms.
  static const double productImageAspect = 1;

  /// Height a [ProductCard] body needs, excluding the image.
  ///
  /// Measured from the widget's own contents rather than estimated:
  ///
  ///   padding (8 top + 8 bottom)              16.0
  ///   brand label      labelSmall 11 * 1.2    13.2
  ///   gap              AppSpacing.xxs          2.0
  ///   name, TWO lines  titleSmall 2*14 * 1.4  39.2
  ///   availability     labelSmall 11 * 1.2    13.2
  ///   gap              AppSpacing.xs           4.0
  ///   add button       fixed                  36.0
  ///   ------------------------------------------------
  ///                                          123.6
  ///
  /// Rounded up to 126 for a little slack against font-metric rounding.
  ///
  /// This exists because a single `childAspectRatio` cannot serve both places
  /// a product card appears. The catalogue grid tile is ~183.5dp wide while the
  /// home carousel card is 160dp, and with a shared ratio the height follows
  /// the WIDTH — so the narrower carousel got even less body room. At the old
  /// 0.62 the grid was 11dp short and the carousel 25dp short, which is exactly
  /// what the screenshots showed: one-line names fine, every two-line name cut
  /// through its second line. `Expanded` had suppressed the overflow warning,
  /// so the layout reported itself valid while silently clipping text.
  ///
  /// Sizing by [productCardHeight] instead makes the body height constant and
  /// correct at any card width.
  static const double productCardBodyHeight = 126;

  /// Total height of a product card for a given width.
  ///
  /// The image is square ([productImageAspect]), so total height is the width
  /// plus the fixed body. Use this for carousels and for a grid's
  /// `mainAxisExtent`; do not reintroduce a hard-coded aspect ratio.
  static double productCardHeight(double width) =>
      width / productImageAspect + productCardBodyHeight;

  /// Minimum tap target. Below 44dp iOS HIG flags the control as unreachable.
  static const double minTapTarget = 44;
}
