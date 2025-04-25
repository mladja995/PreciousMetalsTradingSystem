using FluentValidation;

namespace PreciousMetalsTradingSystem.Application.Hedging.Accounts.Queries.GetDetails
{
    public class GetHedgingAccountDetailsQueryValidator : AbstractValidator<GetHedgingAccountDetailsQuery>
    {
        public GetHedgingAccountDetailsQueryValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
