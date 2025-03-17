using Microsoft.EntityFrameworkCore;
using Tim_Udoma_Western_Insurance.DTOs.Responses.Http;

namespace Tim_Udoma_Western_Insurance.DTOs.Extensions
{
    public static class IQueryableExtensions
    {
        /// <summary>
        /// Returns a paginated list.
        /// </summary>
        /// <typeparam name="T">Type of items in list.</typeparam>
        /// <param name="source">A IQueryable instance to apply.</param>
        /// <param name="pageIndex">Page number that starts with zero.</param>
        /// <param name="pageSize">Size of each page.</param>
        /// <returns>Returns a paginated list.</returns>
        public static async Task<PagedList<T>> ToPaginatedListAsync<T>(this IQueryable<T> source, int pageIndex, int pageSize)
        {
            //return new PagedList<T>(source, index, pageSize);
            return await CreateAsync(source, pageIndex, pageSize);
        }

        /// <summary>
        /// Returns a paginated list. This function returns 15 rows from specific pageIndex.
        /// </summary>
        /// <typeparam name="T">Type of items in list.</typeparam>
        /// <param name="source">A IQueryable instance to apply.</param>
        /// <param name="pageIndex">Page number that starts with zero.</param>    
        /// <returns>Returns a paginated list.</returns>
        public static async Task<PagedList<T>> ToPaginatedListAsync<T>(this IQueryable<T> source, int pageIndex)
        {
            return await CreateAsync(source, pageIndex, 15);
        }

        /// <summary>
        /// Returns a paginated list. This function returns 15 rows from page one.
        /// </summary>
        /// <typeparam name="T">Type of items in list.</typeparam>
        /// <param name="source">A IQueryable instance to apply.</param>    
        /// <returns>Returns a paginated list.</returns>tionality may not work in SQL Compact 3.5</remarks>
        public static async Task<PagedList<T>> ToPaginatedListAsync<T>(this IQueryable<T> source)
        {
            return await CreateAsync(source, 1, 15);
        }

        /// <summary>
        /// Creates a paged list from an IQueryable source asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list</typeparam>
        /// <param name="source">The IQueryable source</param>
        /// <param name="pageIndex">The 1-based page index</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="cancellationToken">Optional cancellation token</param>
        /// <returns>A paged list containing the requested items</returns>
        public static async Task<PagedList<T>> CreateAsync<T>(IQueryable<T> source, int pageIndex, int pageSize, CancellationToken cancellationToken = default)
        {

            try
            {
                // Get total count from source
                int totalCount = await GetTotalCountAsync(source, cancellationToken);

                // Get items for the requested page
                var items = await FetchPageItemsAsync(source, pageIndex, pageSize, totalCount, cancellationToken);

                return new PagedList<T>(items, totalCount, pageIndex, pageSize);
            }
            catch (Exception ex) when (IsQueryTranslationException(ex))
            {
                throw CreateQueryTranslationException(ex);
            }
        }

        private static async Task<int> GetTotalCountAsync<T>(IQueryable<T> source, CancellationToken cancellationToken)
        {
            return await source.CountAsync(cancellationToken);
        }

        private static async Task<List<T>> FetchPageItemsAsync<T>(IQueryable<T> source, int pageIndex, int pageSize, int totalCount, CancellationToken cancellationToken)
        {
            // If page is out of bounds, return empty list
            if (totalCount == 0 || (pageIndex - 1) * pageSize >= totalCount)
            {
                return new List<T>();
            }

            // Fetch items for the requested page
            return await source
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }

        private static bool IsQueryTranslationException(Exception ex)
        {
            return ex is InvalidOperationException || ex is NotSupportedException;
        }

        private static InvalidOperationException CreateQueryTranslationException(Exception innerException)
        {
            return new InvalidOperationException(
                "The pagination operation is not supported by the current query provider. " +
                "Ensure the source is an IQueryable from a compatible provider like Entity Framework.",
                innerException);
        }

    }
}
