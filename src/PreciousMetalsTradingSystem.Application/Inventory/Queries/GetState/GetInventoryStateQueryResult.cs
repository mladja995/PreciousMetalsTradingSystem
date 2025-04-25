using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Application.Inventory.Models;

namespace PreciousMetalsTradingSystem.Application.Inventory.Queries.GetState
{
    public class GetInventoryStateQueryResult : PaginatedQueryResult<ProductLocationState>
    {
        public GetInventoryStateQueryResult(IReadOnlyCollection<ProductLocationState> items, int count, int pageNumber, int pageSize)
            : base(items, count, pageNumber, pageSize)
        {
        }
    }
}
