using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Queries.GetCollection
{
    public class GetSpotDeferredTradesQueryResult : PaginatedQueryResult<SpotDeferredTradeItemResult>
    {
        public GetSpotDeferredTradesQueryResult(
            IReadOnlyCollection<SpotDeferredTradeItemResult> items, int count, int pageNumber, int pageSize)
            : base(items, count, pageNumber, pageSize)
        {
        }
    }
}
