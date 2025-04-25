using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Financials.Models;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace PreciousMetalsTradingSystem.Application.Financials.Queries.GetCurrentBalance
{
    public class GetCurrentBalanceQueryHandler : IRequestHandler<GetCurrentBalanceQuery, Balance>
    {
        private readonly IRepository<Domain.Entities.FinancialTransaction, FinancialTransactionId> _transactionRepository;

        public GetCurrentBalanceQueryHandler(IRepository<Domain.Entities.FinancialTransaction, FinancialTransactionId> transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<Balance> Handle(GetCurrentBalanceQuery request, CancellationToken cancellationToken)
        {
          var balances = await _transactionRepository
                        .StartQuery(readOnly: true, asSplitQuery: true)
                        .Where(t => t.BalanceType == Domain.Enums.BalanceType.Actual ||
                                    t.BalanceType == Domain.Enums.BalanceType.Effective)
                        .GroupBy(t => t.BalanceType)
                        .Select(g => new
                        {
                            BalanceType = g.Key,
                            LatestBalance = g.OrderByDescending(t => t.TimestampUtc).Select(t => t.Balance.Value).FirstOrDefault()
                        })
                        .ToListAsync();

            var actualBalance = balances.FirstOrDefault(b => b.BalanceType == Domain.Enums.BalanceType.Actual)?.LatestBalance ?? 0;
            var availableForTradingBalance = balances.FirstOrDefault(b => b.BalanceType == Domain.Enums.BalanceType.Effective)?.LatestBalance ?? 0;

            return new Balance
            {
                Actual = actualBalance,
                AvailableForTrading = availableForTradingBalance,
            };

        }
    }
}
