using System.ComponentModel;

namespace PreciousMetalsTradingSystem.Domain.Enums
{
    public enum PositionSideType
    {
        [Description("Buy")]
        In = 1,
        [Description("Sell")]
        Out = -1
    }
}
