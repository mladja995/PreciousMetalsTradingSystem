using FluentValidation;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Models;
using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Create
{
    public class DealerTradeCreateCommandValidator : AbstractValidator<DealerTradeCreateCommand>
    {
        public DealerTradeCreateCommandValidator()
        {
            RuleFor(x => x.TradeDate)
                .NotEmpty();

            RuleFor(x => x.Location)
                .IsInEnum()
                .WithMessage($"Location must be within the defined range of Location enum values " +
                    $"({string.Join(", ", Enum.GetValues(typeof(SideType)).Cast<LocationType>())}).");

            RuleFor(x => x.SideType)
                .IsInEnum()
                .WithMessage($"Side must be within the defined range of SideType enum values " +
                    $"({string.Join(", ", Enum.GetValues(typeof(SideType)).Cast<SideType>())}).");

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("Items collection cannot be empty or null.");

            RuleFor(x => x.Items)
                .Must(items => items.GroupBy(item => item.ProductId).All(group => group.Count() == 1))
                .WithMessage("Each ProductId must be unique in the Items collection.");

            RuleForEach(x => x.Items).Custom((item, context) =>
            {
                DealerTradeCreateCommand command = context.InstanceToValidate;

                if (command.AutoHedge)
                {
                    ValidateForAutoHedge(item, context);
                }
                else
                {
                    ValidateWithoutAutoHedge(item, context);
                }
            });
        }

        private static void ValidateForAutoHedge(
            DealerTradeItemRequest item, 
            ValidationContext<DealerTradeCreateCommand> context)
        {
            if (item.ProductId == Guid.Empty)
            {
                context.AddFailure("ProductId", "ProductId cannot be empty.");
            }

            if (item.UnitQuantity <= 0)
            {
                context.AddFailure("UnitQuantity", "UnitQuantity must be greater than zero.");
            }

            if (item.DealerPricePerOz <= 0)
            {
                context.AddFailure("DealerPricePerOz", "DealerPricePerOz must be greater than zero.");
            }
        }

        private static void ValidateWithoutAutoHedge(
            DealerTradeItemRequest item, 
            ValidationContext<DealerTradeCreateCommand> context)
        {
            if (item.ProductId == Guid.Empty)
            {
                context.AddFailure("ProductId", "ProductId cannot be empty.");
            }

            if (item.UnitQuantity <= 0)
            {
                context.AddFailure("UnitQuantity", "UnitQuantity must be greater than zero.");
            }

            if (item.DealerPricePerOz <= 0)
            {
                context.AddFailure("DealerPricePerOz", "DealerPricePerOz must be greater than zero.");
            }

            if (!item.SpotPricePerOz.HasValue || item.SpotPricePerOz <= 0)
            {
                context.AddFailure(
                    "SpotPricePerOz", 
                    "SpotPricePerOz must be defined and greater than zero if AutoHedge is not enabled.");
            }
        }
    }
}
