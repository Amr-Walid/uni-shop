import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import 'app_colors.dart';
import 'app_spacing.dart';
import 'app_typography.dart';

/// Builds the light and dark [ThemeData].
///
/// The theme is parameterised on the seed colour so the primary colour returned
/// by `/api/v1/settings` can re-theme the app at runtime without touching any
/// widget. Component themes are configured centrally here rather than per
/// screen: that is the only way to guarantee a button in the cart looks like a
/// button in checkout.
abstract final class AppTheme {
  static ThemeData light({Color primary = AppColors.primary}) {
    final scheme = ColorScheme.fromSeed(
      seedColor: primary,
      brightness: Brightness.light,
    ).copyWith(
      // fromSeed harmonises the seed into a tonal palette, which shifts the
      // brand colour noticeably. Pin the values the brand actually owns.
      primary: primary,
      onPrimary: AppColors.textOnPrimary,
      surface: AppColors.surface,
      onSurface: AppColors.textPrimary,
      error: AppColors.danger,
    );

    final text = AppTypography.textTheme(
      AppColors.textPrimary,
      AppColors.textSecondary,
    );

    return _base(
      scheme: scheme,
      textTheme: text,
      scaffoldBackground: AppColors.background,
      surface: AppColors.surface,
      surfaceAlt: AppColors.surfaceAlt,
      border: AppColors.border,
      divider: AppColors.divider,
      textSecondary: AppColors.textSecondary,
      textTertiary: AppColors.textTertiary,
      statusBarIconBrightness: Brightness.dark,
    );
  }

  static ThemeData dark({Color primary = AppColors.primary}) {
    final scheme = ColorScheme.fromSeed(
      seedColor: primary,
      brightness: Brightness.dark,
    ).copyWith(
      primary: AppColors.primaryLight,
      onPrimary: AppColors.textPrimaryDark,
      surface: AppColors.surfaceDark,
      onSurface: AppColors.textPrimaryDark,
      error: AppColors.danger,
    );

    final text = AppTypography.textTheme(
      AppColors.textPrimaryDark,
      AppColors.textSecondaryDark,
    );

    return _base(
      scheme: scheme,
      textTheme: text,
      scaffoldBackground: AppColors.backgroundDark,
      surface: AppColors.surfaceDark,
      surfaceAlt: AppColors.surfaceAltDark,
      border: AppColors.borderDark,
      divider: AppColors.dividerDark,
      textSecondary: AppColors.textSecondaryDark,
      textTertiary: AppColors.textTertiaryDark,
      statusBarIconBrightness: Brightness.light,
    );
  }

  static ThemeData _base({
    required ColorScheme scheme,
    required TextTheme textTheme,
    required Color scaffoldBackground,
    required Color surface,
    required Color surfaceAlt,
    required Color border,
    required Color divider,
    required Color textSecondary,
    required Color textTertiary,
    required Brightness statusBarIconBrightness,
  }) {
    OutlineInputBorder fieldBorder(Color color, [double width = 1]) =>
        OutlineInputBorder(
          borderRadius: AppRadius.fieldRadius,
          borderSide: BorderSide(color: color, width: width),
        );

    return ThemeData(
      useMaterial3: true,
      colorScheme: scheme,
      scaffoldBackgroundColor: scaffoldBackground,
      textTheme: textTheme,
      fontFamily: AppTypography.fontFamily,
      dividerColor: divider,

      // Tonal elevation overlays tint every raised surface with the primary
      // colour; disabling keeps cards neutral white/dark as designed.
      applyElevationOverlayColor: false,

      // ── App bar ───────────────────────────────────────────────────────────
      appBarTheme: AppBarTheme(
        backgroundColor: surface,
        foregroundColor: scheme.onSurface,
        surfaceTintColor: Colors.transparent,
        elevation: 0,
        scrolledUnderElevation: 0.5,
        centerTitle: true,
        toolbarHeight: AppSizes.appBarHeight,
        titleTextStyle: textTheme.titleLarge,
        systemOverlayStyle: SystemUiOverlayStyle(
          statusBarColor: Colors.transparent,
          statusBarIconBrightness: statusBarIconBrightness,
          statusBarBrightness: statusBarIconBrightness == Brightness.dark
              ? Brightness.light
              : Brightness.dark,
        ),
      ),

      // ── Cards ─────────────────────────────────────────────────────────────
      // CardTheme (not CardThemeData): the *ThemeData variants were introduced
      // in Flutter 3.27; this project targets 3.24 LTS.
      cardTheme: CardTheme(
        color: surface,
        surfaceTintColor: Colors.transparent,
        elevation: 0,
        margin: EdgeInsets.zero,
        shape: RoundedRectangleBorder(
          borderRadius: AppRadius.cardRadius,
          side: BorderSide(color: border),
        ),
      ),

      // ── Buttons ───────────────────────────────────────────────────────────
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          minimumSize: const Size.fromHeight(AppSizes.buttonHeight),
          backgroundColor: scheme.primary,
          foregroundColor: scheme.onPrimary,
          disabledBackgroundColor: scheme.primary.withOpacity(0.4),
          disabledForegroundColor: scheme.onPrimary.withOpacity(0.7),
          textStyle: textTheme.labelLarge,
          shape: const RoundedRectangleBorder(
            borderRadius: AppRadius.fieldRadius,
          ),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          minimumSize: const Size.fromHeight(AppSizes.buttonHeight),
          foregroundColor: scheme.primary,
          side: BorderSide(color: scheme.primary),
          textStyle: textTheme.labelLarge,
          shape: const RoundedRectangleBorder(
            borderRadius: AppRadius.fieldRadius,
          ),
        ),
      ),
      textButtonTheme: TextButtonThemeData(
        style: TextButton.styleFrom(
          foregroundColor: scheme.primary,
          textStyle: textTheme.labelMedium,
          // Default TextButton padding pushes the tap target under 44dp on
          // short labels such as "المزيد".
          minimumSize: const Size(AppSizes.minTapTarget, AppSizes.minTapTarget),
        ),
      ),
      iconButtonTheme: IconButtonThemeData(
        style: IconButton.styleFrom(
          foregroundColor: scheme.onSurface,
          minimumSize: const Size.square(AppSizes.minTapTarget),
        ),
      ),

      // ── Inputs ────────────────────────────────────────────────────────────
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: surfaceAlt,
        contentPadding: const EdgeInsets.symmetric(
          horizontal: AppSpacing.lg,
          vertical: AppSpacing.lg,
        ),
        hintStyle: textTheme.bodyMedium?.copyWith(color: textTertiary),
        labelStyle: textTheme.bodyMedium?.copyWith(color: textSecondary),
        // Errors are shown under Arabic fields that already have generous
        // leading; keep the style compact so forms do not jump on validation.
        errorStyle: textTheme.bodySmall?.copyWith(color: scheme.error),
        border: fieldBorder(border),
        enabledBorder: fieldBorder(border),
        focusedBorder: fieldBorder(scheme.primary, 1.5),
        errorBorder: fieldBorder(scheme.error),
        focusedErrorBorder: fieldBorder(scheme.error, 1.5),
        disabledBorder: fieldBorder(divider),
      ),

      // ── Chips (filters, attribute values) ─────────────────────────────────
      chipTheme: ChipThemeData(
        backgroundColor: surfaceAlt,
        selectedColor: scheme.primary,
        side: BorderSide(color: border),
        labelStyle: textTheme.labelMedium,
        secondaryLabelStyle:
            textTheme.labelMedium?.copyWith(color: scheme.onPrimary),
        padding: const EdgeInsets.symmetric(
          horizontal: AppSpacing.md,
          vertical: AppSpacing.sm,
        ),
        shape: const RoundedRectangleBorder(
          borderRadius: AppRadius.pillRadius,
        ),
        showCheckmark: false,
      ),

      // ── Navigation ────────────────────────────────────────────────────────
      navigationBarTheme: NavigationBarThemeData(
        backgroundColor: surface,
        surfaceTintColor: Colors.transparent,
        indicatorColor: scheme.primary.withOpacity(0.12),
        height: AppSizes.bottomNavHeight,
        elevation: 0,
        labelBehavior: NavigationDestinationLabelBehavior.alwaysShow,
        labelTextStyle: WidgetStateProperty.resolveWith((states) {
          final selected = states.contains(WidgetState.selected);
          return textTheme.labelSmall?.copyWith(
            color: selected ? scheme.primary : textTertiary,
            fontWeight: selected ? FontWeight.w700 : FontWeight.w500,
          );
        }),
        iconTheme: WidgetStateProperty.resolveWith((states) {
          final selected = states.contains(WidgetState.selected);
          return IconThemeData(
            size: 24,
            color: selected ? scheme.primary : textTertiary,
          );
        }),
      ),

      // ── Sheets & dialogs ──────────────────────────────────────────────────
      bottomSheetTheme: BottomSheetThemeData(
        backgroundColor: surface,
        surfaceTintColor: Colors.transparent,
        modalBackgroundColor: surface,
        elevation: 0,
        shape: const RoundedRectangleBorder(
          borderRadius: AppRadius.sheetRadius,
        ),
        showDragHandle: true,
        dragHandleColor: textTertiary,
      ),
      dialogTheme: DialogTheme(
        backgroundColor: surface,
        surfaceTintColor: Colors.transparent,
        elevation: 0,
        titleTextStyle: textTheme.titleLarge,
        contentTextStyle: textTheme.bodyMedium?.copyWith(color: textSecondary),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(AppRadius.lg),
        ),
      ),

      // ── Feedback ──────────────────────────────────────────────────────────
      snackBarTheme: SnackBarThemeData(
        behavior: SnackBarBehavior.floating,
        backgroundColor: AppColors.textPrimary,
        contentTextStyle: textTheme.bodyMedium?.copyWith(
          color: AppColors.textOnPrimary,
        ),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(AppRadius.sm),
        ),
        insetPadding: const EdgeInsets.all(AppSpacing.lg),
      ),
      progressIndicatorTheme: ProgressIndicatorThemeData(
        color: scheme.primary,
        linearMinHeight: 3,
      ),

      dividerTheme: DividerThemeData(
        color: divider,
        thickness: 1,
        space: 1,
      ),
      listTileTheme: ListTileThemeData(
        contentPadding: const EdgeInsets.symmetric(horizontal: AppSpacing.lg),
        titleTextStyle: textTheme.titleSmall,
        subtitleTextStyle: textTheme.bodySmall?.copyWith(color: textSecondary),
        iconColor: textSecondary,
      ),
      tabBarTheme: TabBarTheme(
        labelStyle: textTheme.labelLarge,
        unselectedLabelStyle: textTheme.labelLarge,
        labelColor: scheme.primary,
        unselectedLabelColor: textTertiary,
        indicatorColor: scheme.primary,
        indicatorSize: TabBarIndicatorSize.label,
        dividerColor: divider,
      ),

      // Ripples are tuned for touch; the default splash on a dense product
      // grid reads as a flash.
      splashFactory: InkSparkle.splashFactory,
    );
  }
}
