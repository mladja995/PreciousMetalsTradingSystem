using FluentValidation;
using PreciousMetalsTradingSystem.Application.Common.Models.Validator;
using PreciousMetalsTradingSystem.Application.Inventory.Queries.GetState;
using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Inventory.Queries
{
    public class GetInventoryStateQueryValidator : PaginatedQueryValidator<GetInventoryStateQuery>
    {
        public GetInventoryStateQueryValidator()
        {
            RuleFor(x => x.Location)
           .IsInEnum()
           .WithMessage($"Location must be within the defined range of LocationType enum values " +
           $"({string.Join(", ", Enum.GetValues(typeof(LocationType)).Cast<LocationType>().ToList())}).");
        }
    }
}
