using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Application.Financials.Models;

namespace PreciousMetalsTradingSystem.Application.Financials.Queries.GetTransactions
{
    public class GetTransactionsQueryResult : PaginatedQueryResult<FinancialTransaction>
    {
        public GetTransactionsQueryResult(
            IReadOnlyCollection<FinancialTransaction> items,
            int count, 
            int pageNumber, 
            int pageSize
        ) : base(items, count, pageNumber, pageSize)
        {
        }
    }
}
