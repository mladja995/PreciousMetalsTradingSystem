using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Application.Trading.Activity.Models;

namespace PreciousMetalsTradingSystem.Application.Trading.Activity.Queries.GetCollection
{
    public class GetActivityQueryResult : PaginatedQueryResult<ActivityItemResult>
    {
        public GetActivityQueryResult(
            IReadOnlyCollection<ActivityItemResult> items, int count, int pageNumber, int pageSize)
            : base(items, count, pageNumber, pageSize)
        {
        }
    }
}
