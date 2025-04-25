using PreciousMetalsTradingSystem.Application.Common.Models;

namespace PreciousMetalsTradingSystem.Application.Hedging.Accounts.Queries.GetCollection
{
    public class GetHedgingAccountsQueryResult : ListQueryResult<Models.HedgingAccount>
    {
        public GetHedgingAccountsQueryResult(IReadOnlyCollection<Models.HedgingAccount> items) 
            : base(items)
        {
        }
    }
}
