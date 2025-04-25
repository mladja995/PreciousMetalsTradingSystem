using FluentValidation;
using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Queries.GetCollection
{
    public class GetHedgingItemsQueryValidator : AbstractValidator<GetHedgingItemsQuery>
    {
        public GetHedgingItemsQueryValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Page number must not be zero or empty.");

            RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("Page size must not be zero or empty.");

            RuleFor(x => x.AccountId).NotEmpty();

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage($"Type must be within the defined range of HedgingItemType enum values " +
                $"({string.Join(", ", Enum.GetValues(typeof(HedgingItemType)).Cast<HedgingItemType>())}).");
        }
    }
}
