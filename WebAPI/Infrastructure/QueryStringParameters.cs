namespace WebAPI.Infrastructure
{
    public class QueryStringParameters
    {
        public string? Search { get; set; }
        public string? SortColum { get; set; }
        public string? SortOrder { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
