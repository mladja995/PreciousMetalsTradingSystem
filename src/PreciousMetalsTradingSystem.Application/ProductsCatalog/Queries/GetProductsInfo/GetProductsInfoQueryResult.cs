using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Models;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Queries.GetProductsInfo
{
    public class GetProductsInfoQueryResult : ListQueryResult<ProductInfo>
    {
        public GetProductsInfoQueryResult(IReadOnlyCollection<ProductInfo> items) 
            : base(items)
        {
        }
    }
}
