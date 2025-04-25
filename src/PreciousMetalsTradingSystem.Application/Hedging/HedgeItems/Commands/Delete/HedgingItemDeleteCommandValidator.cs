using FluentValidation;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Commands.Delete
{
    public class HedgingItemDeleteCommandValidator : AbstractValidator<HedgingItemDeleteCommand>
    {
        public HedgingItemDeleteCommandValidator() 
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.AccountId).NotEmpty();
        }
    }
}
