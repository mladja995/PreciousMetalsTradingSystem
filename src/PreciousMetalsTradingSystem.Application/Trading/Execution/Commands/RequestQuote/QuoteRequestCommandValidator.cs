using FluentValidation;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Models;
using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.RequestQuote
{
    public class QuoteRequestCommandValidator : AbstractValidator<QuoteRequestCommand>
    {
        public QuoteRequestCommandValidator()
        {
            RuleFor(x => x.Location)
                .IsInEnum()
                .WithMessage($"Location must be within the defined range of Location enum values " +
                    $"({string.Join(", ", Enum.GetValues(typeof(LocationType)).Cast<LocationType>())}).");

            RuleFor(x => x.SideType)
                .IsInEnum()
                .WithMessage($"Side must be within the defined range of SideType enum values " +
                    $"({string.Join(", ", Enum.GetValues(typeof(ClientSideType)).Cast<ClientSideType>())}).");

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("Items collection cannot be empty or null.");

            RuleFor(x => x.Items)
                .Must(items => items.GroupBy(item => item.ProductSKU).All(group => group.Count() == 1))
                .WithMessage("Each ProductSKU must be unique in the Items collection.");

            RuleForEach(x => x.Items)
                .SetValidator(x => new QuoteRequestItemValidator());
        }

        private class QuoteRequestItemValidator : AbstractValidator<QuoteRequestItem>
        {
            public QuoteRequestItemValidator()
            {
                RuleFor(x => x.ProductSKU)
                    .NotEmpty();

                RuleFor(x => x.QuantityUnits)
                    .GreaterThan(0);
            }
        }
    }
}
