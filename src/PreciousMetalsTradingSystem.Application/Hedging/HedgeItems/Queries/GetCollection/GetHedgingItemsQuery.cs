using PreciousMetalsTradingSystem.Application.Common.CustomAttributes;
using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Domain.Enums;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Queries.GetCollection
{
    public class GetHedgingItemsQuery : PaginatedQuery, IRequest<GetHedgingItemsQueryResult>
    {
        [OpenApiExclude]
        public required Guid AccountId { get; set; }
        public required HedgingItemType Type { get; set; }
    }
}
