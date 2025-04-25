using FluentValidation;

namespace PreciousMetalsTradingSystem.Application.Hedging.SpotDeferredTrades.Queries.GetSummary
{
    public class GetSpotDeferredTradesSummaryQueryValidator : AbstractValidator<GetSpotDeferredTradesSummaryQuery>
    {
        public GetSpotDeferredTradesSummaryQueryValidator()
        {
            RuleFor(x => x.AccountId).NotEmpty();
        }
    }
}
