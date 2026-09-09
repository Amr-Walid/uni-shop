import '../../../core/network/api_client.dart';
import '../../../core/network/paged_result.dart';
import '../../catalog/domain/product.dart';
import '../../home/domain/home_data.dart';
import '../domain/static_page.dart';

/// Static pages, contact, wishlist, push-token registration and app config.
class ContentRepository {
  const ContentRepository(this._client);

  final ApiClient _client;

  /// Remote kill-switch / force-update contract.
  ///
  /// Fetched at startup before the first screen renders. This is the only way
  /// to react to a critical bug in a build already installed on customers'
  /// phones, so a failure here must NOT block the app — the caller falls back
  /// to a permissive default.
  Future<AppConfig> getAppConfig() async {
    final json = await _client.get<Map<String, dynamic>>('app/config');
    return AppConfig.fromJson(json);
  }

  /// A CMS page (terms, privacy, about) by slug.
  Future<StaticPage> getPage(String slug) async {
    final json = await _client.get<Map<String, dynamic>>(
      'content/pages/$slug',
    );
    return StaticPage.fromJson(json);
  }

  Future<ContactInfo> getContactInfo() async {
    final json = await _client.get<Map<String, dynamic>>(
      'content/contact-info',
    );
    return ContactInfo.fromJson(json);
  }

  Future<void> sendContactMessage({
    required String name,
    required String message,
    String? email,
    String? phone,
    String? subject,
  }) async {
    await _client.post<Map<String, dynamic>>(
      'contact',
      body: {
        'name': name.trim(),
        'email': email?.trim(),
        'phone': phone?.trim(),
        'subject': subject?.trim(),
        'message': message.trim(),
      },
    );
  }

  // ── Wishlist ──────────────────────────────────────────────────────────────

  /// The signed-in user's wishlist.
  Future<PagedResult<Product>> getWishlist({
    int page = 1,
    int pageSize = PagedResult.defaultPageSize,
  }) async {
    final json = await _client.get<Map<String, dynamic>>(
      'wishlist',
      query: {'page': page, 'pageSize': pageSize},
    );

    // The envelope holds WishlistItemDto ({id, createdAt, product}); the app
    // only needs the product, so it is unwrapped here rather than modelling a
    // join row the UI would immediately discard.
    return PagedResult.fromJson(json, (item) {
      final product = item['product'];
      return Product.fromJson(
        product is Map ? Map<String, dynamic>.from(product) : item,
      );
    });
  }

  Future<void> addToWishlist(int productId) async {
    await _client.post<Map<String, dynamic>>('wishlist/$productId');
  }

  Future<void> removeFromWishlist(int productId) async {
    await _client.delete<Map<String, dynamic>>('wishlist/$productId');
  }

  Future<bool> isInWishlist(int productId) async {
    final json = await _client.get<Map<String, dynamic>>(
      'wishlist/$productId/status',
    );
    return json['inWishlist'] == true || json['isInWishlist'] == true;
  }

  // ── Push notifications ────────────────────────────────────────────────────

  /// Registers this device's push token.
  ///
  /// Works for guests too — `DeviceToken.UserId` is nullable server-side
  /// precisely so someone who checks out without an account can still be
  /// notified about their order.
  Future<void> registerDevice({
    required String token,
    required String platform,
    String? deviceInfo,
    String? appVersion,
  }) async {
    await _client.post<Map<String, dynamic>>(
      'devices/register',
      body: {
        'token': token,
        'platform': platform,
        'deviceInfo': deviceInfo,
        'appVersion': appVersion,
      },
    );
  }

  Future<void> unregisterDevice(String token) async {
    await _client.post<Map<String, dynamic>>(
      'devices/unregister',
      body: {'token': token},
    );
  }
}
