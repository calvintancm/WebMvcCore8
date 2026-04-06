/*
 * ═══════════════════════════════════════════════════════════════
 * Helpers/KendoGrid.cs
 *
 * PURPOSE:
 *   Global reusable Kendo Grid helper for ALL controllers and views.
 *   Replaces [DataSourceRequest] which crashes on .NET 8 + Kendo 2019.
 *
 * HOW TO USE IN ANY CONTROLLER:
 *   1. Add parameter: [FromForm] KendoGridRequest kendo
 *   2. Call: var result = await kendo.ToResultAsync(query, selector);
 *   3. Return: return Json(result);
 *
 * HOW TO USE IN ANY VIEW:
 *   1. Remove type: 'aspnetmvc-ajax' from dataSource
 *   2. Keep schema: { data:'Data', total:'Total', errors:'Errors' }
 *   3. Keep serverPaging: true, serverSorting: true
 *   4. parameterMap sends data as form fields (default behavior)
 *
 * WORKS ON: .NET 6, 7, 8, 9, 10 — any future version forever
 * NO DEPENDENCY ON: Telerik NuGet, AddKendo(), [DataSourceRequest]
 * ═══════════════════════════════════════════════════════════════
 */

using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;   // NuGet: System.Linq.Dynamic.Core

namespace ptc_IGH_Sys.Helpers
{
    /* ════════════════════════════════════════════════════════════
       KendoGridRequest
       Receives paging + sorting posted by Kendo JS grid.
       Bind with [FromForm] in your controller action.
    ════════════════════════════════════════════════════════════ */
    public class KendoGridRequest
    {
        /* Kendo posts these automatically with serverPaging: true */
        public int Page     { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public int Skip     { get; set; } = 0;
        public int Take     { get; set; } = 50;

        /* Kendo posts sort[0][field] and sort[0][dir] */
        /* Captured as raw strings and parsed in ToResultAsync */
        public List<KendoSortItem> Sort { get; set; } = new();

        /* ── Computed helpers ── */
        public int ActualSkip => Skip > 0 ? Skip : (Page - 1) * PageSize;
        public int ActualTake => Take > 0 ? Take : PageSize;

        /* ── Build ORDER BY string from sort fields ── */
        public string GetOrderBy(string defaultSort = "Id")
        {
            if (Sort == null || !Sort.Any())
                return defaultSort;

            var parts = Sort
                .Where(s => !string.IsNullOrWhiteSpace(s.Field))
                .Select(s => $"{s.Field} {(s.Dir?.ToLower() == "desc" ? "descending" : "ascending")}");

            return parts.Any() ? string.Join(", ", parts) : defaultSort;
        }

        /* ════════════════════════════════════════════════════════
           ToResultAsync<T>()
           Main method — call this from any controller action.
           
           Parameters:
             query       → your IQueryable (with filters already applied)
             selector    → projection lambda e.g. x => new MyViewModel { ... }
             defaultSort → column to sort by if no sort sent from grid
           
           Returns KendoGridResult<T> which serializes as:
             { Data: [...], Total: 123, Errors: null }
        ════════════════════════════════════════════════════════ */
        public async Task<KendoGridResult<T>> ToResultAsync<TSource, T>(
            IQueryable<TSource> query,
            System.Linq.Expressions.Expression<Func<TSource, T>> selector,
            string defaultSort = "Id")
        {
            /* Total BEFORE paging — needed for Kendo pager */
            var total = await query.CountAsync();

            /* Apply sorting */
            var orderBy = GetOrderBy(defaultSort);
            try
            {
                query = query.OrderBy(orderBy);
            }
            catch
            {
                /* If dynamic sort fails, keep original order */
            }

            /* Apply paging */
            var data = await query
                .Skip(ActualSkip)
                .Take(ActualTake)
                .Select(selector)
                .ToListAsync();

            return new KendoGridResult<T> { Data = data, Total = total };
        }

        /* ── Sync version for in-memory lists ── */
        public KendoGridResult<T> ToResult<TSource, T>(
            IEnumerable<TSource> source,
            Func<TSource, T> selector,
            string defaultSort = "")
        {
            var list  = source.ToList();
            var total = list.Count;

            var data = list
                .Skip(ActualSkip)
                .Take(ActualTake)
                .Select(selector)
                .ToList();

            return new KendoGridResult<T> { Data = data, Total = total };
        }
    }

    /* ════════════════════════════════════════════════════════════
       KendoSortItem
       Represents one sort column posted by Kendo grid.
       Kendo posts: sort[0][field]=DriverName&sort[0][dir]=asc
    ════════════════════════════════════════════════════════════ */
    public class KendoSortItem
    {
        public string Field { get; set; }
        public string Dir   { get; set; }   /* "asc" or "desc" */
    }

    /* ════════════════════════════════════════════════════════════
       KendoGridResult<T>
       Standard response shape Kendo JS expects.
       schema: { data: 'Data', total: 'Total', errors: 'Errors' }
    ════════════════════════════════════════════════════════════ */
    public class KendoGridResult<T>
    {
        public List<T> Data   { get; set; } = new();
        public int     Total  { get; set; } = 0;
        public object  Errors { get; set; } = null;

        /* Convenience factory for error responses */
        public static KendoGridResult<T> Error(string message) => new()
        {
            Data   = new List<T>(),
            Total  = 0,
            Errors = new { ServerError = message }
        };
    }
}
