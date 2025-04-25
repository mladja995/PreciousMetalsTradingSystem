using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Data;
using static OneOf.Types.TrueFalseOrNull;

namespace PreciousMetalsTradingSystem.Application.Financials.Queries.GetTransactions
{
    public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, GetTransactionsQueryResult>
    {
        private readonly IRepository<FinancialTransaction, FinancialTransactionId> _repository;

        public GetTransactionsQueryHandler(
            IRepository<FinancialTransaction, 
            FinancialTransactionId> repository
        )
        {
            _repository = repository;
        }

        public async Task<GetTransactionsQueryResult> Handle(
            GetTransactionsQuery request, 
            CancellationToken cancellationToken)
        {
            var (transactions, totalCount) = await GetTransactionsAsync(request, cancellationToken);

            return new GetTransactionsQueryResult(
                transactions.ToList(),
                totalCount!.Value, 
                request.PageNumber,
                request.PageSize!.Value
            );
        }

        private async Task<(IEnumerable<Models.FinancialTransaction>, int?)> GetTransactionsAsync(
            GetTransactionsQuery request,
            CancellationToken cancellationToken = default)
        {
            DateTime? fromDateUtc = request.FromDate.HasValue ? request.FromDate.Value.Date.ConvertEstToUtc() : null;
            DateTime? toDateUtc = request.ToDate.HasValue ? request.ToDate.Value.Date.ConvertEstToUtc() : null;

            toDateUtc = (toDateUtc.HasValue && toDateUtc.Value.Date < DateTime.MaxValue.Date) ?
                toDateUtc.Value.AddDays(1)
                : DateTime.MaxValue;

            // Start the query with necessary includes
            var transactions = _repository.StartQuery(readOnly: true)
                .Include(x => x.Trade)
                .Include(x => x.FinancialAdjustment)
                .Where(x => x.BalanceType == request.BalanceType
                    && (!fromDateUtc.HasValue || x.TimestampUtc >= fromDateUtc.Value)
                    && (!toDateUtc.HasValue || x.TimestampUtc <= toDateUtc.Value)
                    && (!request.ActivityType.HasValue || request.ActivityType.Value == x.ActivityType)
                    && (!request.SideType.HasValue || request.SideType.Value == x.SideType)
                    && (string.IsNullOrWhiteSpace(request.Reference) || request.Reference == (x.Trade == null ? string.Empty : x.Trade.TradeNumber))
                    && (!request.IsFinancialSettled.HasValue || request.IsFinancialSettled == (x.Trade == null ? true : x.Trade.IsFinancialSettled)));

            // Fetch the total count
            int totalCount = await transactions.CountAsync(cancellationToken);

            // Apply sort
            var sortedTransactions = transactions?.Select(entity => new Models.FinancialTransaction
            {
                Id = entity.Id,
                DateUtc = entity.TimestampUtc,
                BalanceType = entity.BalanceType,
                ActivityType = entity.ActivityType,
                ActivityReference = ReferenceEquals(entity.Trade, null) ? string.Empty : entity.Trade.TradeNumber,
                SideType = entity.SideType,
                IsActivityFinancialSettled = ReferenceEquals(entity.Trade, null) ? true : entity.Trade.IsFinancialSettled,
                Amount = entity.Amount,
                Balance = entity.Balance,
                Note = ReferenceEquals(entity.FinancialAdjustment, null) ? null : entity.FinancialAdjustment.Note,
            }).Sort(!string.IsNullOrWhiteSpace(request.Sort) ? request.Sort : "-DateUtc"); // TODO: Refactor default sort param;

            if (sortedTransactions != null && sortedTransactions.Any())
            {
                // Apply pagination and fetch the result
                var itemsDto = await sortedTransactions
                    .Skip((request.PageNumber - 1) * request.PageSize!.Value)
                    .Take(request.PageSize.Value!)
                    .ToListAsync(cancellationToken);

                // Return the result
                return (itemsDto, totalCount);
            }

            return ([], 0);
        }
    }
}
