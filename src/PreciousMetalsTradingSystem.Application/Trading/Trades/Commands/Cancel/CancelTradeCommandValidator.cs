using FluentValidation;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Cancel
{
    public class CancelTradeCommandValidator : AbstractValidator<CancelTradeCommand>
    {
        public CancelTradeCommandValidator()
        {
            RuleFor(x => x.TradeNumber).NotEmpty();
        }
    }
}
