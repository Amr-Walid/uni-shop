import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

/// Typography.
///
/// Arabic is the primary language of this store, and the default Roboto that
/// ships with Material has no Arabic coverage — it falls back to the platform
/// font, which differs between Android versions and iOS and makes the app look
/// inconsistent. Cairo is bundled through google_fonts for both languages so
/// the rendering is identical everywhere and mixed Arabic/Latin strings (an
/// Arabic product name with a Latin brand) share one typeface.
abstract final class AppTypography {
  /// Registers the font licence so `flutter_oss_licenses`-style screens and the
  /// Google Fonts terms are satisfied. Call once from `main()`.
  static const String fontFamily = 'Cairo';

  static TextTheme textTheme(Color primary, Color secondary) {
    final base = GoogleFonts.cairoTextTheme();

    return base
        .copyWith(
          // ── Display / headline: screen titles and hero numbers ────────────
          displaySmall: base.displaySmall?.copyWith(
            fontSize: 32,
            fontWeight: FontWeight.w700,
            height: 1.25,
          ),
          headlineMedium: base.headlineMedium?.copyWith(
            fontSize: 24,
            fontWeight: FontWeight.w700,
            height: 1.3,
          ),
          headlineSmall: base.headlineSmall?.copyWith(
            fontSize: 20,
            fontWeight: FontWeight.w700,
            height: 1.3,
          ),

          // ── Title: cards, list rows, app bar ─────────────────────────────
          titleLarge: base.titleLarge?.copyWith(
            fontSize: 18,
            fontWeight: FontWeight.w700,
            height: 1.35,
          ),
          titleMedium: base.titleMedium?.copyWith(
            fontSize: 16,
            fontWeight: FontWeight.w600,
            height: 1.4,
          ),
          titleSmall: base.titleSmall?.copyWith(
            fontSize: 14,
            fontWeight: FontWeight.w600,
            height: 1.4,
          ),

          // ── Body ─────────────────────────────────────────────────────────
          // Arabic needs more leading than Latin at the same point size: the
          // script has deep descenders and stacked diacritics that collide at
          // Material's default 1.43 height.
          bodyLarge: base.bodyLarge?.copyWith(
            fontSize: 16,
            height: 1.6,
          ),
          bodyMedium: base.bodyMedium?.copyWith(
            fontSize: 14,
            height: 1.6,
          ),
          bodySmall: base.bodySmall?.copyWith(
            fontSize: 12,
            height: 1.55,
          ),

          // ── Label: buttons, chips, badges ────────────────────────────────
          labelLarge: base.labelLarge?.copyWith(
            fontSize: 15,
            fontWeight: FontWeight.w700,
            height: 1.2,
          ),
          labelMedium: base.labelMedium?.copyWith(
            fontSize: 13,
            fontWeight: FontWeight.w600,
            height: 1.2,
          ),
          labelSmall: base.labelSmall?.copyWith(
            fontSize: 11,
            fontWeight: FontWeight.w600,
            height: 1.2,
          ),
        )
        .apply(
          bodyColor: primary,
          displayColor: primary,
        );
  }

  /// Tabular figures for prices and quantities.
  ///
  /// Without `tabularFigures` the digit glyphs have proportional widths, so a
  /// column of prices in a cart visually jitters as values change.
  static TextStyle price(TextStyle? base) =>
      (base ?? const TextStyle()).copyWith(
        fontFeatures: const [FontFeature.tabularFigures()],
        fontWeight: FontWeight.w700,
      );
}
