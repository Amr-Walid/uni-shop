import '../../../core/network/api_client.dart';
import '../domain/cart_item.dart';

/// Server-side cart sync for signed-in users.
///
/// Guests never touch this: their cart lives only in [CartStorage]. Once a
/// user signs in the local cart is merged up, and from then on the two are
/// kept in step so the same cart appears on the website and on another device.
class CartRepository {
  const CartRepository(this._client);

  final ApiClient _client;

  /// The server's copy of the cart, with live prices and totals.
  Future<Cart> getCart() async {
    final json = await _client.get<Map<String, dynamic>>('cart');
    return Cart.fromJson(json);
  }

  Future<Cart> addItem({required int productId, int quantity = 1}) async {
    final json = await _client.post<Map<String, dynamic>>(
      'cart/items',
      body: {'productId': productId, 'quantity': quantity},
    );
    return Cart.fromJson(json);
  }

  Future<Cart> updateItem({
    required int productId,
    required int quantity,
  }) async {
    final json = await _client.put<Map<String, dynamic>>(
      'cart/items/$productId',
      body: {'quantity': quantity},
    );
    return Cart.fromJson(json);
  }

  Future<Cart> removeItem(int productId) async {
    final json = await _client.delete<Map<String, dynamic>>(
      'cart/items/$productId',
    );
    return Cart.fromJson(json);
  }

  Future<Cart> clear() async {
    final json = await _client.delete<Map<String, dynamic>>('cart');
    return Cart.fromJson(json);
  }

  /// Merges the local guest cart into the account's cart on sign-in.
  ///
  /// [replace] defaults to false so quantities are ADDED rather than
  /// overwritten. Replacing would silently discard whatever the user had put
  /// in their cart on another device, which is the more surprising outcome —
  /// a duplicated quantity is visible and fixable, a vanished item is not.
  Future<Cart> merge(Cart localCart, {bool replace = false}) async {
    final json = await _client.post<Map<String, dynamic>>(
      'cart/merge',
      body: {
        'items': localCart.toRequestItems(),
        'replace': replace,
      },
    );
    return Cart.fromJson(json);
  }
}
