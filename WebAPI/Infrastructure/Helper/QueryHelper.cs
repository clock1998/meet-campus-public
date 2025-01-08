using System.Linq.Expressions;

namespace WebAPI.Infrastructure.Helper
{
    public static class QueryHelper
    {
        /// <summary>
        /// Orders an IQueryable by the given keySelector in either ascending or descending order,
        /// based on the provided sortOrder.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="source">The source IQueryable.</param>
        /// <param name="keySelector">The expression used for ordering.</param>
        /// <param name="sortOrder">A string indicating the sort order (e.g., "desc" or "asc").</param>
        /// <returns>An IOrderedQueryable of T.</returns>
        public static IQueryable<T> DefaultSort<T>( this IQueryable<T> source, Expression<Func<T, object>>? keySelector = null, string? sortOrder = null)
        {
            // If no keySelector is provided, just return the unmodified IQueryable
            if (keySelector == null)
            {
                return source;
            }

            // Compare sortOrder to "desc" (case-insensitive)
            if (!string.IsNullOrEmpty(sortOrder) &&
                sortOrder.Equals("desc", StringComparison.OrdinalIgnoreCase))
            {
                return source.OrderByDescending(keySelector);
            }

            // Default is ascending
            return source.OrderBy(keySelector);
        }
    }
}
