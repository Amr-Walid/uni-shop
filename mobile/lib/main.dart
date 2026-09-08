import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:intl/date_symbol_data_local.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'core/l10n/app_localizations.dart';
import 'core/l10n/locale_controller.dart';
import 'core/providers/core_providers.dart';
import 'core/theme/app_theme.dart';
import 'features/home/presentation/home_controller.dart';
import 'routing/app_router.dart';

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();

  // Loads intl's per-locale date symbols. Without this, any DateFormat
  // constructed with an explicit locale — every order date and timeline
  // timestamp — throws LocaleDataException at runtime.
  initializeDateFormatting();

  // Resolved before the first frame so the locale, theme and cart are all
  // correct on initial paint rather than popping in a frame later.
  final prefs = await SharedPreferences.getInstance();

  runApp(
    ProviderScope(
      overrides: [
        sharedPreferencesProvider.overrideWithValue(prefs),
        appPreferencesProvider.overrideWithValue(AppPreferences(prefs)),
      ],
      child: const UniShopApp(),
    ),
  );
}

class UniShopApp extends ConsumerWidget {
  const UniShopApp({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final router = ref.watch(appRouterProvider);
    final locale = ref.watch(localeControllerProvider);
    final themeMode = ref.watch(themeModeControllerProvider);

    // The admin can change the brand colour in SiteSettings, and the app picks
    // it up from the home payload — so the theme is rebuilt from the server
    // value rather than a bundled constant.
    final primary = ref.watch(storeSettingsProvider).primaryColor;

    return MaterialApp.router(
      // Title is fixed rather than localized: it appears in the Android task
      // switcher, which is read before any locale is resolved.
      title: 'Uni-Shop',
      debugShowCheckedModeBanner: false,

      routerConfig: router,

      theme: AppTheme.light(primary: primary),
      darkTheme: AppTheme.dark(primary: primary),
      themeMode: themeMode,

      locale: locale,
      supportedLocales: AppLocalizations.supportedLocales,
      localizationsDelegates: const [
        AppLocalizations.delegate,
        // Material/Cupertino/Widgets delegates supply the built-in strings and
        // — critically — the RTL directionality that Arabic needs.
        GlobalMaterialLocalizations.delegate,
        GlobalWidgetsLocalizations.delegate,
        GlobalCupertinoLocalizations.delegate,
      ],

      builder: (context, child) {
        // Clamp the OS font scale. Egyptian Android devices are frequently set
        // to a large system font, and beyond 1.3x the two-line product title
        // and the price row start colliding in a grid cell.
        final scale = MediaQuery.textScalerOf(context).scale(1).clamp(0.85, 1.3);

        return MediaQuery(
          data: MediaQuery.of(context).copyWith(
            textScaler: TextScaler.linear(scale),
          ),
          child: child ?? const SizedBox.shrink(),
        );
      },
    );
  }
}
