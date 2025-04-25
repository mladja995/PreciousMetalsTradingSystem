using FluentValidation;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.ExecuteQuote
{
    public class QuoteExecuteCommandValidator : AbstractValidator<QuoteExecuteCommand>
    {
        public QuoteExecuteCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();
        }
    }
}
