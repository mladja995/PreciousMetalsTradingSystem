using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Domain.Primitives.Exceptions
{
    public class WrongTypeForOffsetTradeException : DomainRuleViolationException
    {
        public WrongTypeForOffsetTradeException()
            : base($"Type of offset trade must be {TradeType.OffsetTrade}")
        {
        }
    }
}
