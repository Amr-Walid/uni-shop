import 'package:flutter_test/flutter_test.dart';
import 'package:unishop_app/core/network/paged_result.dart';

Map<String, dynamic> _envelope({
  required List<int> ids,
  int page = 1,
  int pageSize = 20,
  int totalCount = 100,
  bool? hasNext,
  int? totalPages,
}) {
  return {
    'items': ids.map((id) => {'id': id}).toList(),
    'page': page,
    'pageSize': pageSize,
    'totalCount': totalCount,
    if (totalPages != null) 'totalPages': totalPages,
    if (hasNext != null) 'hasNext': hasNext,
  };
}

int _parseId(Map<String, dynamic> json) => json['id'] as int;

void main() {
  group('PagedResult.fromJson', () {
    test('uses the server-computed paging flags when present', () {
      final result = PagedResult.fromJson(
        _envelope(ids: [1, 2], page: 2, totalPages: 5, hasNext: true),
        _parseId,
      );

      expect(result.page, 2);
      expect(result.totalPages, 5);
      expect(result.hasNext, isTrue);
      expect(result.hasPrevious, isTrue);
    });

    test('recomputes paging flags when the server omits them', () {
      // Guards against a trimmed or older response silently breaking
      // infinite scroll.
      final result = PagedResult.fromJson(
        _envelope(ids: [1], page: 1, pageSize: 20, totalCount: 45),
        _parseId,
      );

      expect(result.totalPages, 3);
      expect(result.hasNext, isTrue);
      expect(result.hasPrevious, isFalse);
    });

    test('hasNext is false on the exact-multiple last page', () {
      // The specific case `items.length < pageSize` gets wrong: a final page
      // that happens to be full looks like there is more to fetch.
      final result = PagedResult.fromJson(
        _envelope(
          ids: List.generate(20, (i) => i),
          page: 2,
          pageSize: 20,
          totalCount: 40,
        ),
        _parseId,
      );

      expect(result.items.length, result.pageSize);
      expect(result.hasNext, isFalse);
    });

    test('an empty result is not paginated', () {
      final result = PagedResult.fromJson(
        _envelope(ids: [], totalCount: 0),
        _parseId,
      );

      expect(result.isEmpty, isTrue);
      expect(result.totalPages, 0);
      expect(result.hasNext, isFalse);
    });
  });

  group('PagedResult.appendTo', () {
    test('appends the next page in order', () {
      final first = PagedResult.fromJson(
        _envelope(ids: [1, 2, 3], page: 1, hasNext: true),
        _parseId,
      );
      final second = PagedResult.fromJson(
        _envelope(ids: [4, 5], page: 2, hasNext: false),
        _parseId,
      );

      final merged = second.appendTo(first, keyOf: (id) => id);

      expect(merged.items, [1, 2, 3, 4, 5]);
      expect(merged.page, 2);
      expect(merged.hasNext, isFalse);
    });

    test('de-duplicates items already present', () {
      // An item inserted or re-sorted between two page fetches can appear on
      // both pages. The backend's ThenBy(Id) makes ordering stable, but not
      // the underlying data.
      final first = PagedResult.fromJson(
        _envelope(ids: [1, 2, 3], page: 1, hasNext: true),
        _parseId,
      );
      final second = PagedResult.fromJson(
        _envelope(ids: [3, 4], page: 2, hasNext: false),
        _parseId,
      );

      final merged = second.appendTo(first, keyOf: (id) => id);

      expect(merged.items, [1, 2, 3, 4]);
      expect(
        merged.items.where((id) => id == 3).length,
        1,
        reason: 'the duplicate must be dropped, not rendered twice',
      );
    });

    test('a fully duplicated page adds nothing', () {
      final first = PagedResult.fromJson(
        _envelope(ids: [1, 2], page: 1, hasNext: true),
        _parseId,
      );
      final second = PagedResult.fromJson(
        _envelope(ids: [1, 2], page: 2, hasNext: false),
        _parseId,
      );

      expect(second.appendTo(first, keyOf: (id) => id).items, [1, 2]);
    });
  });

  group('page size limits', () {
    test('the client cap matches the backend MaxPageSize', () {
      // Requesting more is clamped server-side, so asking for more than this
      // would silently return fewer items than the caller expects.
      expect(PagedResult.maxPageSize, 50);
      expect(PagedResult.defaultPageSize, 20);
    });
  });
}
