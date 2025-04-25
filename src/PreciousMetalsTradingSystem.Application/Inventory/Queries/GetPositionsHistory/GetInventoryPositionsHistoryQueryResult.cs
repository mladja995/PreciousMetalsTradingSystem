using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Application.Inventory.Models;

namespace PreciousMetalsTradingSystem.Application.Inventory.Queries.GetPositionsHistory
{
    public class GetInventoryPositionsHistoryQueryResult : PaginatedQueryResult<ProductLocationPositionHistory>
    {
        public GetInventoryPositionsHistoryQueryResult(
            IReadOnlyCollection<ProductLocationPositionHistory> items, 
            int count, 
            int pageNumber, 
            int pageSize) : base(items, count, pageNumber, pageSize)
        {
        }        
    }
}
