using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Domain.Enums;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Financials.Queries.GetTransactions
{
    public class GetTransactionsQuery : PaginatedQuery, IRequest<GetTransactionsQueryResult>
    {
        public required BalanceType BalanceType { get; init; }
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public ActivityType? ActivityType { get; init; }
        public string? Reference { get; init; }
        public TransactionSideType? SideType { get; init; }
        public bool? IsFinancialSettled { get; init; }
    }
}
