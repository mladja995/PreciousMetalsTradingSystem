using PreciousMetalsTradingSystem.Application.Common.CustomAttributes;
using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Domain.Enums;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Inventory.Queries.GetState
{
    public class GetInventoryStateQuery : PaginatedQuery, IRequest<GetInventoryStateQueryResult>
    {
        [OpenApiExclude]
        public required LocationType Location { get; set; }
        public DateTime? OnDate { get; set; }
    }
}
