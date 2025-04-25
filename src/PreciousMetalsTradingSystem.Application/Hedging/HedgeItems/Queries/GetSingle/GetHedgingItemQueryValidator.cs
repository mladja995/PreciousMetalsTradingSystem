using FluentValidation;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Queries.GetSingle
{
    public class GetHedgingItemQueryValidator : AbstractValidator<GetHedgingItemQuery>
    {
        public GetHedgingItemQueryValidator() 
        {
            RuleFor(x => x.AccountId).NotEmpty();
            RuleFor(x => x.HedingItemId).NotEmpty();
        }
    }
}
