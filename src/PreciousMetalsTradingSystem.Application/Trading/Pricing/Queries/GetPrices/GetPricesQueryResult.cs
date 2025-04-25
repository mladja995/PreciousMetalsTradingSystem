using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Application.Trading.Pricing.Models;

namespace PreciousMetalsTradingSystem.Application.Trading.Pricing.Queries.GetPrices
{
    public class GetPricesQueryResult : ListQueryResult<ProductPrice>
    {
        public GetPricesQueryResult(IReadOnlyCollection<ProductPrice> items) 
            : base(items)
        {
        }
    }
}
