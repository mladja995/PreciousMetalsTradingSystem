using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Trading.Execution.Exceptions
{
    public class TradeQuoteInvalidStatusException : ConflictException
    {
        public TradeQuoteInvalidStatusException(TradeQuoteStatusType tradeQuoteStatus)
            : base($"Trade Quote in wrong state ({tradeQuoteStatus})!")
        {
        }
    }
}
