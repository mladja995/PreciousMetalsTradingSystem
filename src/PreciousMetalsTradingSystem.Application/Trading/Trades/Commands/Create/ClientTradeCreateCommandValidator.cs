using FluentValidation;
using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Create
{
    public class ClientTradeCreateCommandValidator : AbstractValidator<ClientTradeCreateCommand>
    {
        public ClientTradeCreateCommandValidator()
        {
            RuleFor(x => x.TradeDate)
                .NotEmpty();

            RuleFor(x => x.Location)
                .IsInEnum()
                .WithMessage($"Location must be within the defined range of Location enum values " +
                    $"({string.Join(", ", Enum.GetValues(typeof(SideType)).Cast<LocationType>())}).");

            RuleFor(x => x.SideType)
                .IsInEnum()
                .WithMessage($"Side must be within the defined range of ClientSideType enum values " +
                    $"({string.Join(", ", Enum.GetValues(typeof(SideType)).Cast<ClientSideType>())}).");

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("Items collection cannot be empty or null.");

            RuleFor(x => x.Items)
                .Must(items => items.GroupBy(item => item.ProductId).All(group => group.Count() == 1))
                .WithMessage("Each ProductId must be unique in the Items collection.");

            RuleForEach(x => x.Items)
                .ChildRules(items =>
                {
                    items.RuleFor(item => item.ProductId)
                        .NotEqual(Guid.Empty)
                        .WithMessage("ProductId cannot be an empty GUID.");

                    items.RuleFor(item => item.SpotPricePerOz)
                        .GreaterThan(0)
                        .WithMessage("SpotPricePerOz must be greater than zero.");

                    items.RuleFor(item => item.UnitQuantity)
                        .GreaterThan(0)
                        .WithMessage("UnitQuantity must be greater than zero.");
                });
        }
    }
}
