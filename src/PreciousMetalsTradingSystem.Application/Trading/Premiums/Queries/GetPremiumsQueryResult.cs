using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Application.Trading.Premiums.Models;

namespace PreciousMetalsTradingSystem.Application.Trading.Premiums.Queries
{
    public class GetPremiumsQueryResult : ListQueryResult<ProductPremium>
    {
        public GetPremiumsQueryResult(IReadOnlyCollection<ProductPremium> items) 
            : base(items)
        {
        }
    }
}
