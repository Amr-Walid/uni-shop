import 'dart:io';

import 'package:dio/dio.dart';

import '../error/api_error_codes.dart';
import '../error/app_exception.dart';

/// Translates every Dio failure into an [AppException].
///
/// This is the single place where transport concerns are converted into
/// something the UI can render, so no screen ever inspects a `DioException`.
class ErrorInterceptor extends Interceptor {
  @override
  void onError(DioException err, ErrorInterceptorHandler handler) {
    final exception = _map(err);

    // Reject with the mapped exception attached so callers can rethrow it
    // directly instead of re-deriving the cause.
    handler.reject(
      DioException(
        requestOptions: err.requestOptions,
        response: err.response,
        type: err.type,
        error: exception,
      ),
    );
  }

  AppException _map(DioException err) {
    switch (err.type) {
      case DioExceptionType.connectionTimeout:
      case DioExceptionType.sendTimeout:
      case DioExceptionType.receiveTimeout:
      // transformTimeout fires when encoding/decoding a body exceeds the
      // transformer budget. It is grouped with the transport timeouts because
      // it is equally transient from the user's point of view.
      case DioExceptionType.transformTimeout:
        return const AppException(
          code: ApiErrorCodes.timeout,
          message: 'انتهت مهلة الاتصال. تحقق من الشبكة وحاول مرة أخرى.',
        );

      case DioExceptionType.connectionError:
      case DioExceptionType.unknown:
        // A SocketException here means the device is offline or the host is
        // unreachable — distinct from a server that answered with an error.
        if (err.error is SocketException || err.error is HttpException) {
          return const AppException(
            code: ApiErrorCodes.networkError,
            message: 'لا يوجد اتصال بالإنترنت. تحقق من الشبكة وحاول مرة أخرى.',
          );
        }
        return AppException(
          code: ApiErrorCodes.unknown,
          message: err.message ?? 'حدث خطأ غير متوقع.',
        );

      case DioExceptionType.cancel:
        return const AppException(
          code: ApiErrorCodes.unknown,
          message: 'تم إلغاء الطلب.',
        );

      case DioExceptionType.badCertificate:
        return const AppException(
          code: ApiErrorCodes.networkError,
          message: 'تعذّر التحقق من شهادة الأمان للخادم.',
        );

      case DioExceptionType.badResponse:
        return _mapResponse(err.response);
    }
  }

  /// Parses an RFC 7807 ProblemDetails body.
  ///
  /// Written defensively: a proxy, load balancer or WAF can return an HTML
  /// error page instead, and the app must still produce a usable message
  /// rather than crashing on a failed cast.
  AppException _mapResponse(Response<dynamic>? response) {
    final status = response?.statusCode;
    final data = response?.data;

    if (data is Map<String, dynamic>) {
      final code = data['errorCode'] as String?;
      final detail = data['detail'] as String?;
      final title = data['title'] as String?;
      final traceId = data['traceId'] as String?;

      // ValidationProblemDetails carries per-field messages.
      final fieldErrors = <String, List<String>>{};
      final errors = data['errors'];
      if (errors is Map) {
        errors.forEach((key, value) {
          if (value is List) {
            fieldErrors[key.toString()] =
                value.map((e) => e.toString()).toList();
          } else if (value != null) {
            fieldErrors[key.toString()] = [value.toString()];
          }
        });
      }

      return AppException(
        code: code ?? _codeFromStatus(status),
        message: detail ?? title ?? _messageFromStatus(status),
        statusCode: status,
        fieldErrors: fieldErrors,
        traceId: traceId,
      );
    }

    // Non-JSON body (HTML error page, empty response, plain text).
    return AppException(
      code: _codeFromStatus(status),
      message: _messageFromStatus(status),
      statusCode: status,
    );
  }

  String _codeFromStatus(int? status) => switch (status) {
        400 => ApiErrorCodes.badRequest,
        401 => ApiErrorCodes.unauthorized,
        403 => ApiErrorCodes.forbidden,
        404 => ApiErrorCodes.notFound,
        409 => ApiErrorCodes.conflict,
        423 => ApiErrorCodes.accountLocked,
        429 => ApiErrorCodes.rateLimited,
        503 => ApiErrorCodes.maintenance,
        _ => ApiErrorCodes.serverError,
      };

  String _messageFromStatus(int? status) => switch (status) {
        400 => 'طلب غير صالح.',
        401 => 'مطلوب تسجيل الدخول.',
        403 => 'غير مصرح بهذه العملية.',
        404 => 'العنصر المطلوب غير موجود.',
        409 => 'تعارض في البيانات.',
        423 => 'الحساب مقفل مؤقتاً. حاول بعد قليل.',
        429 => 'طلبات كثيرة جداً. انتظر قليلاً ثم حاول مرة أخرى.',
        503 => 'الخدمة تحت الصيانة حالياً.',
        _ => 'حدث خطأ في الخادم. حاول مرة أخرى.',
      };
}
