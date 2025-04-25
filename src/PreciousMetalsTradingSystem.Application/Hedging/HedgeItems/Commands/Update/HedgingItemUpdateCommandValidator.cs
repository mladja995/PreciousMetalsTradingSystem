using FluentValidation;
using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Commands.Update
{
    public class HedgingItemUpdateCommandValidator : AbstractValidator<HedgingItemUpdateCommand>
    {
        public HedgingItemUpdateCommandValidator()
        {
            RuleFor(x => x.AccountId)
                .NotEmpty();

            RuleFor(x => x.HedgingItemId)
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
