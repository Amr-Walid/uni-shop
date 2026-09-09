import 'package:flutter/material.dart';
import 'package:flutter_localizations/flutter_localizations.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:unishop_app/core/error/api_error_codes.dart';
import 'package:unishop_app/core/error/app_exception.dart';
import 'package:unishop_app/core/l10n/app_localizations.dart';
import 'package:unishop_app/core/theme/app_theme.dart';
import 'package:unishop_app/core/widgets/state_views.dart';

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
    home: Scaffold(body: child),
  );
}

void main() {
  group('ErrorView copy selection', () {
    testWidgets('a network error gets offline copy and a wifi icon',
        (tester) async {
      await tester.pumpWidget(
        _wrap(
          const ErrorView(
            error: AppException(
              code: ApiErrorCodes.networkError,
              message: 'raw transport message',
            ),
          ),
        ),
      );

      expect(find.text('لا يوجد اتصال بالإنترنت'), findsOneWidget);
      expect(find.byIcon(Icons.wifi_off_outlined), findsOneWidget);
    });

    testWidgets('a timeout is distinguished from a generic failure',
        (tester) async {
      await tester.pumpWidget(
        _wrap(
          const ErrorView(
            error: AppException(
              code: ApiErrorCodes.timeout,
              message: 'timed out',
            ),
          ),
        ),
      );

      expect(find.text('انتهت مهلة الاتصال'), findsOneWidget);
      expect(find.byIcon(Icons.timer_off_outlined), findsOneWidget);
    });

    testWidgets('an unmapped code shows the server message verbatim',
        (tester) async {
      // The server already localizes and specialises these, so its wording
      // beats anything the client could invent.
      await tester.pumpWidget(
        _wrap(
          const ErrorView(
            error: AppException(
              code: 'PRODUCT_UNAVAILABLE',
              message: 'المنتج لم يعد متوفراً',
            ),
          ),
        ),
      );

      expect(find.text('المنتج لم يعد متوفراً'), findsOneWidget);
    });

    testWidgets('a non-AppException degrades to generic copy', (tester) async {
      await tester.pumpWidget(
        _wrap(ErrorView(error: Exception('unexpected'))),
      );

      expect(find.text('حدث خطأ ما. حاول مرة أخرى.'), findsOneWidget);
    });
  });

  group('ErrorView affordances', () {
    testWidgets('retry appears only when a handler is given', (tester) async {
      const error = AppException(
        code: ApiErrorCodes.networkError,
        message: 'offline',
      );

      await tester.pumpWidget(_wrap(const ErrorView(error: error)));
      expect(find.byType(OutlinedButton), findsNothing);

      var retried = false;
      await tester.pumpWidget(
        _wrap(ErrorView(error: error, onRetry: () => retried = true)),
      );

      await tester.tap(find.text('إعادة المحاولة'));
      await tester.pump();

      expect(retried, isTrue);
    });

    testWidgets('the server traceId is surfaced for support', (tester) async {
      // The only way to correlate a real user's report with a server log.
      await tester.pumpWidget(
        _wrap(
          const ErrorView(
            error: AppException(
              code: ApiErrorCodes.serverError,
              message: 'boom',
              traceId: '00-abc123-def456-01',
            ),
          ),
        ),
      );

      expect(find.text('00-abc123-def456-01'), findsOneWidget);
    });

    testWidgets('no traceId means no stray identifier on screen',
        (tester) async {
      await tester.pumpWidget(
        _wrap(
          const ErrorView(
            error: AppException(
              code: ApiErrorCodes.serverError,
              message: 'boom',
            ),
          ),
        ),
      );

      expect(find.byType(SelectableText), findsNothing);
    });
  });

  group('AppException semantics', () {
    test('tokenExpired is NOT fatal — it is what refresh exists for', () {
      const expired = AppException(
        code: ApiErrorCodes.tokenExpired,
        message: '',
      );
      expect(expired.isFatalAuth, isFalse);
      expect(expired.isTokenExpired, isTrue);
    });

    test('a revoked or reused refresh token IS fatal', () {
      // Reuse means the token was stolen; the family is revoked server-side
      // and the session cannot be recovered.
      for (final code in [
        ApiErrorCodes.tokenInvalid,
        ApiErrorCodes.refreshTokenInvalid,
        ApiErrorCodes.refreshTokenReused,
        ApiErrorCodes.accountDisabled,
      ]) {
        expect(
          AppException(code: code, message: '').isFatalAuth,
          isTrue,
          reason: '$code must end the session',
        );
      }
    });

    test('a temporary account lockout is NOT fatal', () {
      // ACCOUNT_LOCKED is Identity's time-boxed lockout after failed
      // attempts. The credentials still exist, so the user can sign in again
      // shortly — discarding their session would be wrong.
      expect(
        const AppException(code: ApiErrorCodes.accountLocked, message: '')
            .isFatalAuth,
        isFalse,
      );
    });

    test('field errors are addressable per input', () {
      const exception = AppException(
        code: ApiErrorCodes.validationFailed,
        message: 'invalid',
        fieldErrors: {
          'customerPhone': ['رقم الهاتف غير صحيح'],
        },
      );

      expect(exception.fieldError('customerPhone'), 'رقم الهاتف غير صحيح');
      expect(exception.fieldError('customerName'), isNull);
    });

    test('transport failures are retryable, business rejections are not', () {
      expect(
        const AppException(code: ApiErrorCodes.networkError, message: '')
            .isRetryable,
        isTrue,
      );
      expect(
        const AppException(code: ApiErrorCodes.validationFailed, message: '')
            .isRetryable,
        isFalse,
      );
    });
  });
}
