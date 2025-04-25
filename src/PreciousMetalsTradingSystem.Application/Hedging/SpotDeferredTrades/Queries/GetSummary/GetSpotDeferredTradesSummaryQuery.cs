using MediatR;
using PreciousMetalsTradingSystem.Application.Common.CustomAttributes;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Queries.GetSummary
{
    public class GetSpotDeferredTradesSummaryQuery : IRequest<GetSpotDeferredTradesSummaryQueryResult>
    {
        [OpenApiExclude]
        public required Guid AccountId { get; set; }
    }
}
