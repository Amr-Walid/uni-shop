import 'dart:convert';

import 'package:shared_preferences/shared_preferences.dart';

import '../domain/cart_item.dart';

/// Local cart persistence.
///
/// The cart is authoritative on the DEVICE, not the server, for guests: the
/// store is fully shoppable without an account, so the cart must survive an
/// app restart with no session. For signed-in users the server keeps a copy
/// (`UserCart.CartJson`) which is merged on sign-in.
///
/// [SharedPreferences] rather than secure storage: a cart is not a secret, and
/// secure storage reads are slow enough on Android to delay the first paint of
/// the cart tab.
class CartStorage {
  const CartStorage(this._prefs);

  final SharedPreferences _prefs;

  static const _key = 'cart.items';

  /// Reads the persisted cart.
  ///
  /// Any decode failure yields an empty cart rather than propagating: a
  /// corrupt entry (interrupted write, schema change between versions) must
  /// not make the app unusable, and the worst case is a shopper re-adding
  /// items.
  List<CartItem> read() {
    final raw = _prefs.getString(_key);
    if (raw == null || raw.isEmpty) return const [];

    try {
      final decoded = jsonDecode(raw);
      if (decoded is! List) return const [];

      final items = <CartItem>[];
      for (final entry in decoded) {
        if (entry is! Map) continue;
        try {
          items.add(CartItem.fromStorageJson(Map<String, dynamic>.from(entry)));
        } catch (_) {
          continue;
        }
      }
      return items;
    } catch (_) {
      return const [];
    }
  }

  Future<void> write(List<CartItem> items) async {
    if (items.isEmpty) {
      await _prefs.remove(_key);
      return;
    }

    final encoded = jsonEncode(
      items.map((item) => item.toStorageJson()).toList(),
    );
    await _prefs.setString(_key, encoded);
  }

  Future<void> clear() => _prefs.remove(_key);
}
