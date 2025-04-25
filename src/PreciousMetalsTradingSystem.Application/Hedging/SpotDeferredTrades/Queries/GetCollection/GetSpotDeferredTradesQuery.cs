using PreciousMetalsTradingSystem.Application.Common.CustomAttributes;
using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Domain.Enums;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Queries.GetCollection
{
    public class GetSpotDeferredTradesQuery : PaginatedQuery, IRequest<GetSpotDeferredTradesQueryResult>
    {
        [OpenApiExclude]
        public required Guid AccountId { get; set; }
        public required MetalType MetalType { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
    }
}
