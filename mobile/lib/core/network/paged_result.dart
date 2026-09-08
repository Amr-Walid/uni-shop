import '../utils/json.dart';

/// Client mirror of the backend `PagedResult<T>` envelope.
///
/// Every list endpoint returns this shape, so infinite scroll is driven by
/// [hasNext] rather than by guessing from `items.length < pageSize` — that
/// guess breaks on the exact-multiple case, where the last full page looks
/// like there is more to fetch.
class PagedResult<T> {
  const PagedResult({
    required this.items,
    required this.page,
    required this.pageSize,
    required this.totalCount,
    required this.totalPages,
    required this.hasNext,
    required this.hasPrevious,
  });

  final List<T> items;

  /// 1-based, matching the API.
  final int page;
  final int pageSize;
  final int totalCount;
  final int totalPages;
  final bool hasNext;
  final bool hasPrevious;

  /// Backend `PagedResult.MaxPageSize`. Requesting more is silently clamped
  /// server-side, so the app never asks for more than it can get.
  static const int maxPageSize = 50;
  static const int defaultPageSize = 20;

  bool get isEmpty => items.isEmpty;
  bool get isNotEmpty => items.isNotEmpty;

  static PagedResult<T> empty<T>() => PagedResult<T>(
        items: const [],
        page: 1,
        pageSize: defaultPageSize,
        totalCount: 0,
        totalPages: 0,
        hasNext: false,
        hasPrevious: false,
      );

  factory PagedResult.fromJson(
    Map<String, dynamic> json,
    T Function(Map<String, dynamic>) parseItem,
  ) {
    final page = Json.integer(json, 'page', fallback: 1);
    final pageSize =
        Json.integer(json, 'pageSize', fallback: defaultPageSize);
    final totalCount = Json.integer(json, 'totalCount');

    // `totalPages`, `hasNext` and `hasPrevious` are computed properties on the
    // C# side and therefore *are* serialised — but they are recomputed here as
    // a fallback so a trimmed or older response still drives paging correctly.
    final serverTotalPages = Json.intOrNull(json, 'totalPages');
    final totalPages = serverTotalPages ??
        (pageSize <= 0 ? 0 : (totalCount + pageSize - 1) ~/ pageSize);

    return PagedResult<T>(
      items: Json.list(json, 'items', parseItem),
      page: page,
      pageSize: pageSize,
      totalCount: totalCount,
      totalPages: totalPages,
      hasNext: json['hasNext'] is bool
          ? json['hasNext'] as bool
          : page < totalPages,
      hasPrevious:
          json['hasPrevious'] is bool ? json['hasPrevious'] as bool : page > 1,
    );
  }

  /// Appends the next page onto an accumulated list.
  ///
  /// Duplicates are filtered by [keyOf] because a product inserted or
  /// re-sorted between two page fetches can otherwise appear twice — the
  /// backend's `ThenBy(Id)` tiebreak makes ordering stable, but not the
  /// underlying data.
  PagedResult<T> appendTo(
    PagedResult<T> previous, {
    required Object Function(T item) keyOf,
  }) {
    final seen = previous.items.map(keyOf).toSet();
    final merged = [
      ...previous.items,
      ...items.where((item) => seen.add(keyOf(item))),
    ];

    return PagedResult<T>(
      items: merged,
      page: page,
      pageSize: pageSize,
      totalCount: totalCount,
      totalPages: totalPages,
      hasNext: hasNext,
      hasPrevious: hasPrevious,
    );
  }
}
