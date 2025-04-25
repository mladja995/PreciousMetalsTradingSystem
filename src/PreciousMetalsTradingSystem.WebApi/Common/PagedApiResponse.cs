using PreciousMetalsTradingSystem.Application.Common.Models;

namespace PreciousMetalsTradingSystem.WebApi.Common
{
    public class PagedApiResponse : ApiResponse
    {
        public int TotalCount { get; init; }
        public int TotalPages { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }
        public bool HasPreviousPage { get; init; }
        public bool HasNextPage { get; init; }


        public PagedApiResponse(
            object? data, 
            int totalCount, 
            int totalPages, 
            int pageNumber, 
            int pageSize, 
            bool hasPreviousPage, 
            bool hasNextPage) : base(data, true, 200) 
        {
            TotalCount = totalCount;
            TotalPages = totalPages;
            PageNumber = pageNumber;
            PageSize = pageSize;
            HasPreviousPage = hasPreviousPage;
            HasNextPage = hasNextPage;
        }

        public static PagedApiResponse Success<T>(PaginatedQueryResult<T> result)
            where T : class
            => new(
                result.Items, 
                result.TotalCount, 
                result.TotalPages, 
                result.PageNumber, 
                result.PageSize, 
                result.HasPreviousPage, 
                result.HasNextPage);
    }
}
