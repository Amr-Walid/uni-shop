import 'package:flutter/material.dart';

/// Brand palette — the single source of truth is the WEBSITE stylesheet
/// (`FTD.Web/wwwroot/css/site.css`), not the database.
///
/// An earlier version of this file took [primary] from the `site.primary.color`
/// SiteSetting seed (#1A6BFF, a blue). That was wrong: the storefront never
/// renders that value — its `:root` block hard-codes `--primary: #330077`
/// (deep purple) with `--accent: #FF7A00` (orange), which is what a visitor
/// actually sees. The app therefore looked like a different product than the
/// site. Every value below is copied from that stylesheet so the two match.
///
/// The server can still override the primary colour at runtime through
/// `/api/v1/settings` (`PublicSettingsDto.PrimaryColor`), so treat these as
/// defaults used until the first successful settings fetch — never as the only
/// source of truth.
abstract final class AppColors {
  // ── Brand ─────────────────────────────────────────────────────────────────
  /// `--primary: #330077`.
  static const Color primary = Color(0xFF330077);

  /// `--tertiary: #2c0080` — used where the site goes darker than primary.
  static const Color primaryDark = Color(0xFF2C0080);

  /// `--primary-hover: #4a248e`, also `--primary-container`.
  static const Color primaryLight = Color(0xFF4A248E);

  /// `--on-primary-container: #b896ff` — the light lilac the site uses for
  /// text and icons sitting ON a purple container.
  static const Color onPrimaryContainer = Color(0xFFB896FF);

  /// `--surface-container-low: #eff4ff`. Very light tint for selected chips
  /// and info banners.
  static const Color primarySurface = Color(0xFFEFF4FF);

  // ── Accent (orange) ───────────────────────────────────────────────────────
  // The site's `--accent`. Used sparingly: the logo cart, badges, focus glow
  // and the radial wash behind product imagery. Never as a large fill.
  /// `--accent: #FF7A00`.
  static const Color accent = Color(0xFFFF7A00);

  /// `--secondary-container: #fb7800`.
  static const Color accentStrong = Color(0xFFFB7800);

  /// `--accent-soft: #ffdbc8`.
  static const Color accentSoft = Color(0xFFFFDBC8);

  /// `--secondary: #994700` — accent text dark enough to pass contrast.
  static const Color accentText = Color(0xFF994700);

  // ── Semantic ──────────────────────────────────────────────────────────────
  /// `--success: #059669`.
  static const Color success = Color(0xFF059669);

  /// `--warning: #d97706`.
  static const Color warning = Color(0xFFD97706);

  /// `--error: #ba1a1a`.
  static const Color danger = Color(0xFFBA1A1A);

  /// The site has no distinct info colour; it reuses primary.
  static const Color info = Color(0xFF330077);

  /// Discount / sale accent. Distinct from [danger] so a price badge is never
  /// mistaken for an error state.
  static const Color sale = Color(0xFFFF7A00);

  // ── Neutrals (light) ──────────────────────────────────────────────────────
  /// `--surface: #f8f9ff` — the site's faintly violet page background.
  static const Color background = Color(0xFFF8F9FF);

  /// `--surface-container-lowest: #ffffff`.
  static const Color surface = Color(0xFFFFFFFF);

  /// `--surface-container-low: #eff4ff`.
  static const Color surfaceAlt = Color(0xFFEFF4FF);

  /// `--surface-container: #e5eeff`, the site's `--outline-soft`.
  static const Color surfaceTint = Color(0xFFE5EEFF);

  /// `--outline-variant: #cbc3d3` — the product card's 1px border.
  static const Color border = Color(0xFFCBC3D3);

  /// `--outline-soft: #e5eeff`.
  static const Color divider = Color(0xFFE5EEFF);

  /// `--navy: #0b1c30`, the site's `--on-surface`. Prices use this.
  static const Color textPrimary = Color(0xFF0B1C30);

  /// `--on-surface-variant: #4a4452` (also `--navy-3`).
  static const Color textSecondary = Color(0xFF4A4452);

  /// `--on-surface-muted: #7b7483`, the site's `--outline`.
  static const Color textTertiary = Color(0xFF7B7483);

  /// `--navy-2: #213145`, the site's `--inverse-surface`.
  static const Color navySoft = Color(0xFF213145);
  static const Color textOnPrimary = Color(0xFFFFFFFF);

  // ── Neutrals (dark) ───────────────────────────────────────────────────────
  static const Color backgroundDark = Color(0xFF0F1319);
  static const Color surfaceDark = Color(0xFF171C24);
  static const Color surfaceAltDark = Color(0xFF1F2530);
  static const Color borderDark = Color(0xFF2A323F);
  static const Color dividerDark = Color(0xFF222A35);

  static const Color textPrimaryDark = Color(0xFFF2F4F8);
  static const Color textSecondaryDark = Color(0xFFA9B2C1);
  static const Color textTertiaryDark = Color(0xFF6D7787);

  // ── Shimmer ───────────────────────────────────────────────────────────────
  static const Color shimmerBase = Color(0xFFE8EBF0);
  static const Color shimmerHighlight = Color(0xFFF6F8FA);
  static const Color shimmerBaseDark = Color(0xFF1F2530);
  static const Color shimmerHighlightDark = Color(0xFF2A323F);

  /// Order-status colours keyed by `OrderStatus.Id`.
  ///
  /// The API also returns `colorHex` per status; prefer that when present and
  /// fall back to this map so an admin-added status still renders sensibly.
  static const Map<int, Color> orderStatus = {
    1: Color(0xFF1A6BFF), // جديد / New
    2: Color(0xFF0E4FCC), // مؤكد / Confirmed
    3: Color(0xFFFF9500), // في انتظار الشحن / Pending Shipment
    4: Color(0xFFFF6B35), // مع شركة الشحن / With Courier
    5: Color(0xFF00C48C), // تم التسليم / Delivered
    6: Color(0xFFFF3B30), // مرتجع / Returned
    7: Color(0xFF6C757D), // ملغي / Cancelled
  };

  /// Parses a `#RRGGBB` or `#AARRGGBB` string coming from the API.
  ///
  /// Returns [fallback] for anything unparseable: admin-entered colour values
  /// are free text in the database, so malformed input must never crash a list.
  static Color fromHex(String? hex, {Color fallback = primary}) {
    if (hex == null) return fallback;
    var value = hex.trim().replaceFirst('#', '');
    if (value.length == 6) value = 'FF$value';
    if (value.length != 8) return fallback;
    final parsed = int.tryParse(value, radix: 16);
    return parsed == null ? fallback : Color(parsed);
  }
}
