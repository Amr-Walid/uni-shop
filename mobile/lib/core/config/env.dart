/// Build-time configuration.
///
/// Values come from `--dart-define`, NOT from a committed file, so the same
/// source tree produces dev / staging / production builds without a code edit:
///
///   flutter build appbundle --dart-define=API_BASE_URL=https://api.unishop.eg
library;

enum Flavor { dev, staging, prod }

class Env {
  const Env._();

  /// Base URL of the API, without a trailing slash.
  ///
  /// The default targets the Android emulator loopback alias (10.0.2.2), which
  /// maps to the host machine's localhost — `localhost` inside the emulator
  /// would resolve to the emulator itself and always fail.
  static const String apiBaseUrl = String.fromEnvironment(
    'API_BASE_URL',
    defaultValue: 'http://10.0.2.2:5100',
  );

  static const String flavorName = String.fromEnvironment(
    'FLAVOR',
    defaultValue: 'dev',
  );

  static Flavor get flavor => switch (flavorName) {
        'prod' => Flavor.prod,
        'staging' => Flavor.staging,
        _ => Flavor.dev,
      };

  static bool get isProduction => flavor == Flavor.prod;

  /// Network request timeouts.
  ///
  /// Deliberately generous: on a congested mobile network a 10-second ceiling
  /// produces spurious failures for requests that would have succeeded.
  static const Duration connectTimeout = Duration(seconds: 20);
  static const Duration receiveTimeout = Duration(seconds: 30);

  /// Only log request/response bodies outside production. Logging them in a
  /// release build would leak bearer tokens and customer data into device logs.
  static bool get enableNetworkLogging => !isProduction;

  /// Versioned API prefix. Pinned per app release so a future v2 cannot change
  /// the contract underneath an already-published build.
  static const String apiVersion = 'v1';

  static String get apiRoot => '$apiBaseUrl/api/$apiVersion';
}
