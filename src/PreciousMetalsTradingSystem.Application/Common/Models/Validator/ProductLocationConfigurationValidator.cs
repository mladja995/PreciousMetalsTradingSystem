using FluentValidation;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Models;

namespace PreciousMetalsTradingSystem.Application.Common.Models.Validator
{
    public class ProductLocationConfigurationValidator : AbstractValidator<ProductLocationConfiguration>
    {
        public ProductLocationConfigurationValidator()
        {
            // Location must be a valid enum
            RuleFor(x => x.Location)
                .IsInEnum().WithMessage("Location must be a valid enum value.");

            // PremiumUnitType must be a valid enum
            RuleFor(x => x.PremiumUnitType)
                .IsInEnum().WithMessage("PremiumUnitType must be a valid enum value.");
        }
    }
}
