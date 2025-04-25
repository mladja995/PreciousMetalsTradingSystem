using FluentValidation;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Settle
{
    public class TradeSettleCommandValidator : AbstractValidator<TradeSettleCommand>
    {
        public TradeSettleCommandValidator()
        {
            RuleFor(x => x.Id)
            .NotEmpty();
        }
    }
}
