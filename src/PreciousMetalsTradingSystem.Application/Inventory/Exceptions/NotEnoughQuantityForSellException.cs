using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Inventory.Exceptions
{
    public class NotEnoughQuantityForSellException : ConflictException
    {
        public NotEnoughQuantityForSellException(PositionType postionType)
            : base($"There is no enough quantity for sell of position type {postionType}")
        {
        }
    }
}
