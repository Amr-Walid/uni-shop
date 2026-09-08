import 'package:equatable/equatable.dart';

import 'api_error_codes.dart';

/// A failure the UI can render, normalised from any source (HTTP error,
/// socket failure, timeout, parse error).
///
/// Everything below the presentation layer throws this type, so no widget ever
/// has to know about `DioException` — which also keeps Dio swappable.
class AppException extends Equatable implements Exception {
  const AppException({
    required this.code,
    required this.message,
    this.statusCode,
    this.fieldErrors = const {},
    this.traceId,
  });

  /// Stable machine-readable code — see [ApiErrorCodes].
  final String code;

  /// Human-readable message. Server-provided messages are already localized
  /// (the API honours `Accept-Language`), so they are shown as-is.
  final String message;

  final int? statusCode;

  /// Per-field validation errors from `ValidationProblemDetails.errors`,
  /// used to mark individual form inputs rather than showing one banner.
  final Map<String, List<String>> fieldErrors;

  /// Correlation id from the server, surfaced in support screens so a report
  /// can be tied to a specific request in the logs.
  final String? traceId;

  /// True when the session cannot be recovered and the user must sign in again.
  bool get isFatalAuth => ApiErrorCodes.fatalAuthCodes.contains(code);

  /// True when the access token merely expired: the interceptor refreshes and
  /// replays the request, so this must NOT trigger a sign-out.
  bool get isTokenExpired => code == ApiErrorCodes.tokenExpired;

  /// True for transport-level problems, which are worth offering a retry for
  /// (unlike a 400, where retrying the same payload changes nothing).
  bool get isRetryable =>
      code == ApiErrorCodes.networkError ||
      code == ApiErrorCodes.timeout ||
      code == ApiErrorCodes.serverError ||
      code == ApiErrorCodes.rateLimited;

  bool get isNotFound =>
      statusCode == 404 ||
      code == ApiErrorCodes.notFound ||
      code == ApiErrorCodes.productNotFound ||
      code == ApiErrorCodes.orderNotFound ||
      code == ApiErrorCodes.pageNotFound;

  /// First error for a given form field, if any.
  String? fieldError(String field) {
    // The server uses PascalCase property names ("Email"); the client may ask
    // with either casing, so match case-insensitively.
    for (final entry in fieldErrors.entries) {
      if (entry.key.toLowerCase() == field.toLowerCase()) {
        return entry.value.isEmpty ? null : entry.value.first;
      }
    }
    return null;
  }

  @override
  List<Object?> get props => [code, message, statusCode];

  @override
  String toString() => 'AppException($code, $statusCode): $message';
}
