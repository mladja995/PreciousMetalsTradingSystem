using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Financials.Exceptions
{
    public class NotEnoughCashForBuyException : ConflictException
    {
        public NotEnoughCashForBuyException(BalanceType balanceType)
            : base($"There is no enough cash for buy of balance type {balanceType}")
        {
        }
    }
}
