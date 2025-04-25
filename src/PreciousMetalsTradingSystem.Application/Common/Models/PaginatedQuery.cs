namespace PreciousMetalsTradingSystem.Application.Common.Models
{
    public abstract class PaginatedQuery
    {
        public required int PageNumber { get; init; }
        public int? PageSize { get; set; }
        public string? Sort { get; set; }

    }
}
