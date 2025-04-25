using PreciousMetalsTradingSystem.Application.Common.CustomAttributes;
using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Domain.Enums;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Inventory.Queries.GetPositionsHistory
{
    public class GetInventoryPositionsHistoryQuery : PaginatedQuery, IRequest<GetInventoryPositionsHistoryQueryResult>
    {
        [OpenApiExclude]
        public required LocationType Location { get; set; }

        // HACK: string.Empty -> Because of model binding error for string type
        [OpenApiExclude]
        public required string ProductSKU { get; set; } = string.Empty; 
        public required PositionType PositionType { get; set; }
    }
}
