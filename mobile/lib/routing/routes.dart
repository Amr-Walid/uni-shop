/// Route paths, in one place.
///
/// String literals are never used at call sites: a typo in `context.go('/prodcut/x')`
/// is a runtime error that only surfaces when that path is exercised, whereas a
/// typo'd constant fails to compile.
abstract final class Routes {
  // ── Tabs ──────────────────────────────────────────────────────────────────
  static const String home = '/';
  static const String catalog = '/shop';
  static const String cart = '/cart';
  static const String orders = '/orders';
  static const String account = '/account';

  // ── Pushed routes ─────────────────────────────────────────────────────────
  static const String search = '/search';
  static const String checkout = '/checkout';
  static const String trackOrder = '/track';
  static const String login = '/login';

  /// Patterns used to register parameterised routes.
  static const String productPattern = '/product/:slug';
  static const String orderDetailPattern = '/order/:id';
  static const String orderConfirmationPattern = '/order-confirmed/:orderNumber';

  // ── Builders ──────────────────────────────────────────────────────────────
  // Path segments are percent-encoded: a slug or order number is
  // admin/server-generated but may still contain characters that would
  // otherwise break the URL or change which route matches.

  static String product(String slug) =>
      '/product/${Uri.encodeComponent(slug)}';

  static String orderDetail(int id) => '/order/$id';

  static String orderConfirmation(String orderNumber) =>
      '/order-confirmed/${Uri.encodeComponent(orderNumber)}';

  static String categoryCatalog(String slug, {String? title}) => Uri(
        path: catalog,
        queryParameters: {
          'category': slug,
          if (title != null) 'title': title,
        },
      ).toString();

  static String brandCatalog(String slug, {String? title}) => Uri(
        path: catalog,
        queryParameters: {
          'brand': slug,
          if (title != null) 'title': title,
        },
      ).toString();

  static String searchResults(String query) => Uri(
        path: search,
        queryParameters: {'q': query},
      ).toString();

  /// Routes that require a session.
  ///
  /// Checkout is deliberately ABSENT: guest checkout is a core capability of
  /// this store, and forcing sign-in at the payment step is the single biggest
  /// cause of cart abandonment. Tracking is likewise public, protected instead
  /// by the phone-digits second factor.
  static const List<String> protectedPaths = [
    orders,
    account,
  ];
}
