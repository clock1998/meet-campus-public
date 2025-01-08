using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace WebAPI.Infrastructure
{
    public class PagedList<T>
    {
        public List<T> Items { get; private set; }
        public int Page { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPagesCount { get; private set; }
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page * PageSize < TotalCount;

        public string[] ColumnNames { get; private set; }
        public PagedList(List<T> items, int page, int pageSize, int totalCount)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            ColumnNames = typeof(T).GetProperties().Select(n => FormatColumnName(n.Name)).ToArray();
            TotalCount = totalCount;
            TotalPagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);
        }
        public async static Task<PagedList<T>> ToPagedListAsync(IQueryable<T> query, int page, int pageSize)
        {
            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedList<T>(items, page, pageSize, totalCount);
        }

        private string FormatColumnName(string name)
        {
            string formattedName = string.Empty;
            foreach (char c in name)
            {
                if (char.IsUpper(c))
                {
                    formattedName += " ";
                }
                formattedName += c;
            }
            return formattedName.Trim();
        }
    }
}
