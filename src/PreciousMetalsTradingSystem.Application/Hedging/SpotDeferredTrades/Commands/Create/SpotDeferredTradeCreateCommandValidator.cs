using FluentValidation;
using PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Models;
using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Commands.Create
{
    public class SpotDeferredTradeCreateCommandValidator : AbstractValidator<SpotDeferredTradeCreateCommand>
    {
        public SpotDeferredTradeCreateCommandValidator()
        {
            RuleFor(x => x.AccountId).NotEmpty();
            
            RuleFor(x => x.TradeConfirmationNumber).NotEmpty();

            RuleFor(x => x.Date)
                .NotNull()
                .NotEmpty();
            
            RuleFor(x => x.SideType)
                .IsInEnum()
                .WithMessage($"Type must be within the defined range of SideType enum values " +
                $"({string.Join(", ", Enum.GetValues(typeof(SideType)).Cast<SideType>())}).");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Items collection cannot be empty or null.")
                .Must(items => items.GroupBy(item => item.MetalType).All(g => g.Count() == 1))
                .WithMessage("Items collection can only have one record for each MetalType.");

            RuleForEach(x => x.Items)
               .SetValidator(new SpotDeferredTradeItemValidator());
        }

        private class SpotDeferredTradeItemValidator : AbstractValidator<SpotDeferredTradeItem>
        {
            public SpotDeferredTradeItemValidator()
            {
                RuleFor(x => x.MetalType)
                    .IsInEnum()
                    .WithMessage($"Type must be within the defined range of MetalType enum values " +
                    $"({string.Join(", ", Enum.GetValues(typeof(MetalType)).Cast<MetalType>())}).");

                RuleFor(x => x.SpotPricePerOz)
                    .GreaterThan(0);

                RuleFor(x => x.QuantityOz)
                    .GreaterThan(0);
            }
        }
    }
}
