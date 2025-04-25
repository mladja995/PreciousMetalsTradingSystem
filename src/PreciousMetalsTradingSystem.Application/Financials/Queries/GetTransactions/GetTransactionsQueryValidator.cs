using FluentValidation;
using PreciousMetalsTradingSystem.Application.Common.Models.Validator;
using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Financials.Queries.GetTransactions
{
    public class GetTransactionsQueryValidator : PaginatedQueryValidator<GetTransactionsQuery>
    {
        public GetTransactionsQueryValidator()
        { 
            RuleFor(x => x.BalanceType)
                .IsInEnum()
                .WithMessage($"BalanceType must be within the defined range of BalanceType enum values " +
                                $"({string.Join(", ", Enum.GetValues(typeof(BalanceType)).Cast<BalanceType>().ToList())}).");
            
            RuleFor(x => x.FromDate)
               .Must(date => date == null || date.Value > DateTime.MinValue && date.Value < DateTime.MaxValue)
               .WithMessage("FromDate must be a valid date if provided.");

            RuleFor(x => x.ToDate)
                .Must(date => date == null || (date.Value > DateTime.MinValue && date.Value < DateTime.MaxValue))
                .WithMessage("ToDate must be a valid date if provided.");

            RuleFor(x => x.ActivityType)
                .IsInEnum()
                .When(x => x.ActivityType.HasValue)
                .WithMessage($"ActivityType must be within the defined range of ActivityType enum values " +
                                $"({string.Join(", ", Enum.GetValues(typeof(ActivityType)).Cast<ActivityType>().ToList())}).");

            RuleFor(x => x.SideType)
                .IsInEnum()
                .When(x => x.SideType.HasValue)
                .WithMessage($"Side must be within the defined range of TransactionSideType enum values " +
                                $"({string.Join(", ", Enum.GetValues(typeof(TransactionSideType)).Cast<TransactionSideType>().ToList())}).");
        }
    }
}
