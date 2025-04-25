using FluentValidation;
using PreciousMetalsTradingSystem.Application.Common.Models.Validator;
using PreciousMetalsTradingSystem.Application.Inventory.Queries.GetPositionsHistory;
using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Inventory.Queries
{
    public class GetInventoryPositionsHistoryQueryValidator : PaginatedQueryValidator<GetInventoryPositionsHistoryQuery>
    {
        public GetInventoryPositionsHistoryQueryValidator()
        {
            RuleFor(x => x.Location)
               .IsInEnum()
               .WithMessage($"Location must be within the defined range of LocationType enum values " +
                               $"({string.Join(", ", Enum.GetValues(typeof(LocationType)).Cast<LocationType>().ToList())}).");

            RuleFor(x => x.ProductSKU)
                .NotNull()
                .NotEmpty();

            RuleFor(x => x.PositionType)
              .IsInEnum()
              .WithMessage($"Position Type must be within the defined range of PositionType enum values " +
                              $"({string.Join(", ", Enum.GetValues(typeof(PositionType)).Cast<PositionType>().ToList())}).");
        }
    }
}
