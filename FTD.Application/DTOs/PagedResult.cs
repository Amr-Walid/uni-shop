using System;
using System.Collections.Generic;

namespace FTD.Application.DTOs
{
    /// <summary>
    /// Envelope for every paginated list the API returns.
    ///
    /// Why this exists: the original endpoints returned the ENTIRE result set.
    /// With the heavyweight ProductDto that meant multi-megabyte responses,
    /// which is unusable on a mobile connection. Every list endpoint now returns
    /// a bounded page plus the metadata a client needs to drive infinite scroll.
    /// </summary>
    public class PagedResult<T>
    {
        /// <summary>Default page size when the client does not specify one.</summary>
        public const int DefaultPageSize = 20;

        /// <summary>
        /// Hard ceiling on page size. Without a cap, a caller could request
        /// pageSize=100000 and turn pagination back into a full table scan —
        /// an easy denial-of-service vector.
        /// </summary>
        public const int MaxPageSize = 50;

        public List<T> Items { get; set; } = new();

        /// <summary>1-based page index.</summary>
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = DefaultPageSize;

        public int TotalCount { get; set; }

        public int TotalPages => PageSize <= 0
            ? 0
            : (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasNext => Page < TotalPages;
        public bool HasPrevious => Page > 1;

        public PagedResult() { }

        public PagedResult(List<T> items, int page, int pageSize, int totalCount)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        /// <summary>
        /// Clamps caller-supplied paging values into the supported range.
        /// Centralised here so every endpoint enforces the same limits and no
        /// controller can accidentally forget the cap.
        /// </summary>
        public static (int Page, int PageSize) Normalize(int? page, int? pageSize)
        {
            var p = page.GetValueOrDefault(1);
            if (p < 1) p = 1;

            var size = pageSize.GetValueOrDefault(DefaultPageSize);
            if (size < 1) size = DefaultPageSize;
            if (size > MaxPageSize) size = MaxPageSize;

            return (p, size);
        }

        public static PagedResult<T> Empty(int page = 1, int pageSize = DefaultPageSize)
            => new(new List<T>(), page, pageSize, 0);
    }
}
