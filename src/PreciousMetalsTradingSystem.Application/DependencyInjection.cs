using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using FluentValidation;
using PreciousMetalsTradingSystem.Application.Common.Behaviors;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Services;
using PreciousMetalsTradingSystem.Application.Inventory.Services;
using PreciousMetalsTradingSystem.Application.AMark.Services;
using PreciousMetalsTradingSystem.Application.Heding.SpotDeferredTrades.Services;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Services;
using PreciousMetalsTradingSystem.Application.Trading.Pricing.Services;
using PreciousMetalsTradingSystem.Application.Financials.Services;
using PreciousMetalsTradingSystem.Application.Common.Locking;
using PreciousMetalsTradingSystem.Application.Trading.Services;

namespace PreciousMetalsTradingSystem.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);
            

            services.AddMediatR(cfg => {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingPipelineBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PaginationPipelineBehavior<,>));
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LockingBehavior<,>)); 
            });

            services.AddScoped<IProductsService, ProductsService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IHedgingService, HedgingService>();
            services.AddScoped<IAMarkTradingServiceFactory, AMarkTradingServiceFactory>();
            services.AddScoped<ICalendarService, CalendarService>();
            services.AddScoped<IHolidayProvider, StaticHolidayProvider>();
            services.AddScoped<IPricingService, PricingService>();
            services.AddScoped<IFinancialsService, FinancialsService>();
            services.AddTransient<ITradingService, TradingService>();
            services.AddSingleton<ILockManager, LockManager>();

            return services;
        }
    }
}
