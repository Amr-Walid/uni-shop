import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'app_localizations.dart';

/// Persisted app preferences: language and theme mode.
///
/// Stored in [SharedPreferences] rather than secure storage — neither value is
/// a secret, and secure storage on Android is noticeably slower to read, which
/// would delay first paint.
class AppPreferences {
  AppPreferences(this._prefs);

  final SharedPreferences _prefs;

  static const _localeKey = 'app.locale';
  static const _themeKey = 'app.themeMode';

  String? readLocale() => _prefs.getString(_localeKey);
  Future<void> writeLocale(String code) => _prefs.setString(_localeKey, code);

  String? readThemeMode() => _prefs.getString(_themeKey);
  Future<void> writeThemeMode(String mode) =>
      _prefs.setString(_themeKey, mode);
}

/// Overridden in `main()` after `SharedPreferences.getInstance()` resolves, so
/// no widget ever has to await preferences during build.
final appPreferencesProvider = Provider<AppPreferences>((ref) {
  throw UnimplementedError(
    'appPreferencesProvider must be overridden in ProviderScope.',
  );
});

/// Current UI locale. Also drives the `Accept-Language` header, so changing it
/// re-localises server-supplied product and category names too.
class LocaleController extends Notifier<Locale> {
  @override
  Locale build() {
    final stored = ref.read(appPreferencesProvider).readLocale();
    if (stored != null) return Locale(stored);

    // No stored choice yet: honour the device language when we support it,
    // otherwise Arabic — this store's primary audience.
    final device = WidgetsBinding.instance.platformDispatcher.locale;
    final supported = AppLocalizations.supportedLocales
        .any((l) => l.languageCode == device.languageCode);
    return supported ? Locale(device.languageCode) : const Locale('ar');
  }

  Future<void> setLocale(Locale locale) async {
    if (locale.languageCode == state.languageCode) return;
    state = Locale(locale.languageCode);
    await ref.read(appPreferencesProvider).writeLocale(locale.languageCode);
  }

  Future<void> toggle() =>
      setLocale(Locale(state.languageCode == 'ar' ? 'en' : 'ar'));
}

final localeControllerProvider =
    NotifierProvider<LocaleController, Locale>(LocaleController.new);

/// Language code for the API. Watched by the network layer, so a language
/// change immediately affects subsequent requests.
final languageCodeProvider = Provider<String>((ref) {
  final locale = ref.watch(localeControllerProvider);
  return locale.languageCode == 'en' ? 'en' : 'ar';
});

class ThemeModeController extends Notifier<ThemeMode> {
  @override
  ThemeMode build() {
    final stored = ref.read(appPreferencesProvider).readThemeMode();
    return switch (stored) {
      'light' => ThemeMode.light,
      'dark' => ThemeMode.dark,
      _ => ThemeMode.system,
    };
  }

  Future<void> setMode(ThemeMode mode) async {
    if (mode == state) return;
    state = mode;
    await ref.read(appPreferencesProvider).writeThemeMode(mode.name);
  }
}

final themeModeControllerProvider =
    NotifierProvider<ThemeModeController, ThemeMode>(ThemeModeController.new);
