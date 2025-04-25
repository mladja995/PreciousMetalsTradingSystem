using PreciousMetalsTradingSystem.Domain.DomainServices;
using Microsoft.Extensions.DependencyInjection;

namespace PreciousMetalsTradingSystem.Domain
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.AddTransient<ITradeCancellationService, TradeCancellationService>();
            services.AddTransient<ITradeFactory, TradeFactory>();
            services.AddTransient<IFinancialBalanceValidator, FinancialBalanceValidator>();
            services.AddTransient<IInventoryPositionValidator, InventoryPositionValidator>();

            return services;
        }
    }
}
