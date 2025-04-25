using FluentValidation;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Queries.GetSingle
{
    public class GetTradeQueryValidator : AbstractValidator<GetTradeDetailsQuery>
    {
        public GetTradeQueryValidator() 
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}
