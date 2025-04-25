using FluentValidation;
using PreciousMetalsTradingSystem.Application.Common.Models.Validator;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Commands.Create
{
    public class ProductCreateCommandValidator : AbstractValidator<ProductCreateCommand>
    {
        public ProductCreateCommandValidator()
        {
            // ProductSKU should not be empty and have a maximum length of SKU.MaxLength
            RuleFor(x => x.ProductSKU)
                .NotEmpty().WithMessage("ProductSKU cannot be empty.")
                .MaximumLength(SKU.MaxLength).WithMessage($"ProductSKU cannot be longer than {SKU.MaxLength} characters.");

            // ProductName should not be empty and have a maximum length of 100
            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("ProductName cannot be empty.")
                .MaximumLength(100).WithMessage("ProductName cannot be longer than 100 characters.");

            // MetalType should be a valid enum value
            RuleFor(x => x.MetalType)
                .IsInEnum().WithMessage("MetalType must be a valid enum value.");

            // WeightInOz should be greater than zero and have a maximum of 4 decimal places
            RuleFor(x => x.WeightInOz)
                .GreaterThan(0).WithMessage("WeightInOz must be greater than zero.")
                .PrecisionScale(12, 4, true).WithMessage("WeightInOz must not have more than 4 decimal places.");

            // Validate LocationConfigurations collection
            RuleForEach(x => x.LocationConfigurations)
                .SetValidator(new ProductLocationConfigurationValidator());

            // Ensure no duplicate LocationConfigurations if collection is not null or empty
            RuleFor(x => x.LocationConfigurations)
                .Must(locationConfigurations =>
                    locationConfigurations == null || !locationConfigurations.Any() || locationConfigurations.GroupBy(lc => lc.Location).All(g => g.Count() == 1))
                .WithMessage("There cannot be more than one configuration for the same Location.");
        }
    }
}
