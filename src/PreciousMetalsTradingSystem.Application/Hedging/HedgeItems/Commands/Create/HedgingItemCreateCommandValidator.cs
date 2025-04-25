using FluentValidation;
using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Commands.Create
{
    public class HedgingItemCreateCommandValidator : AbstractValidator<HedgingItemCreateCommand>
    {
        public HedgingItemCreateCommandValidator()
        {
            RuleFor(x => x.AccountId)
                .NotEmpty();

            RuleFor(x => x.Amount)
                .GreaterThan(0);

            RuleFor(x => x.SideType)
                .IsInEnum()
                .WithMessage($"Type must be within the defined range of HedgingItemSideType enum values " +
                $"({string.Join(", ", Enum.GetValues(typeof(HedgingItemSideType)).Cast<HedgingItemSideType>())}).");

            RuleFor(x => x.HedgingItemType)
                .IsInEnum()
                .WithMessage($"Type must be within the defined range of HedgingItemType enum values " +
                $"({string.Join(", ", Enum.GetValues(typeof(HedgingItemType)).Cast<HedgingItemType>())}).");
        }
    }
}
