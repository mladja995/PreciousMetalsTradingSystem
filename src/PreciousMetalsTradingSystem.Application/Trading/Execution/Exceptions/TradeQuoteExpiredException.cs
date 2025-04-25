using PreciousMetalsTradingSystem.Application.Common.Exceptions;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Exceptions
{
    public class TradeQuoteExpiredException : ConflictException
    {
        public TradeQuoteExpiredException()
            : base("Trade Quote expired!")
        {
        }
    }
}
