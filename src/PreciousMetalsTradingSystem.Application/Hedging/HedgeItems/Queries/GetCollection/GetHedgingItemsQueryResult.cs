using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Models;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Queries.GetCollection
{
    public class GetHedgingItemsQueryResult : PaginatedQueryResult<HedgingItem>
    {
        public GetHedgingItemsQueryResult(IReadOnlyCollection<HedgingItem> items, int count, int pageNumber, int pageSize)
            : base(items, count, pageNumber, pageSize)
        {
        }
    }
}
