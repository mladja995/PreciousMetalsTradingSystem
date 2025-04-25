using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Models;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Queries.GetCollection
{
    public class GetProductsQueryResult : PaginatedQueryResult<Product>
    {

        public GetProductsQueryResult(IReadOnlyCollection<Product> items, int count,int pageNumber, int pageSize) 
            : base (items,count,pageNumber,pageSize) 
        {
        
        }

    }
}
