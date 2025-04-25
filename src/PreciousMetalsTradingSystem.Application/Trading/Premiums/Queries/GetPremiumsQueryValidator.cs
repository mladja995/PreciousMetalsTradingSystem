using FluentValidation;
using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Trading.Premiums.Queries
{
    public class GetPremiumsQueryValidator : AbstractValidator<GetPremiumsQuery>
    {
        public GetPremiumsQueryValidator()
        {
            RuleFor(x => x.Location)
               .Must(location => location is null || Enum.IsDefined(typeof(LocationType), location))
               .WithMessage($"Location must be within the defined range of LocationType enum values " +
                $"({string.Join(", ", Enum.GetValues(typeof(LocationType)).Cast<LocationType>().ToList())}).");

            RuleFor(x => x.ClientSide)
                .Must(side => side == null || Enum.IsDefined(typeof(ClientSideType), side))
                .WithMessage($"Type must be within the defined range of SideType enum values " +
                $"({string.Join(", ", Enum.GetValues(typeof(ClientSideType)).Cast<ClientSideType>())}).");
        }
    }
}
