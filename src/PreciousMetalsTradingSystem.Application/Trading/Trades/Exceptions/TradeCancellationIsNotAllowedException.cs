using PreciousMetalsTradingSystem.Application.Common.Exceptions;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Exceptions
{
    public class TradeCancellationIsNotAllowedException : ConflictException
    {
        public TradeCancellationIsNotAllowedException() 
            : base("Trade is already cancelled or type of this trade does not support cancellation.") 
        { }   
    }
}
