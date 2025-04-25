namespace PreciousMetalsTradingSystem.WebApi.Common.Authorization
{
    public enum Permission
    {
        ViewAllData = 1,
        ManageProducts,
        ManageSpotDeferredTrades,
        ManageHedgingItems,
        ManagePositions, 
        ManageFinancialAdjustments, 
        Trading,
        ManageTrades
    }
}
