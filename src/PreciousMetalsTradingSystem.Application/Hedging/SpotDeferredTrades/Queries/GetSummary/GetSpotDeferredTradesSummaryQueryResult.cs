using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Queries.GetSummary
{
    public class GetSpotDeferredTradesSummaryQueryResult : ListQueryResult<SpotDeferredTradeSummaryItem>
    {
        public GetSpotDeferredTradesSummaryQueryResult(IReadOnlyCollection<SpotDeferredTradeSummaryItem> items)
            : base(items)
        {
        }
    }
}
