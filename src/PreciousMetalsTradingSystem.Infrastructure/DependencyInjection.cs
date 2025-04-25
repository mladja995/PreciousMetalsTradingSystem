using PreciousMetalsTradingSystem.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Caching;
using PreciousMetalsTradingSystem.Infrastructure.Caching;
using Hangfire;
using Hangfire.SqlServer;
using PreciousMetalsTradingSystem.Infrastructure.Jobs.Options;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PreciousMetalsTradingSystem.Infrastructure.Jobs;
using PreciousMetalsTradingSystem.Application.Emailing.Services;
using PreciousMetalsTradingSystem.Infrastructure.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PreciousMetalsTradingSystem.Infrastructure.SignalR;
using PreciousMetalsTradingSystem.Application.Common.DomainEvents;
using PreciousMetalsTradingSystem.Application.Common.Notifications;

namespace PreciousMetalsTradingSystem.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            bool initializeForTests = false)
        {
            services
                .AddDatabase(initializeForTests)
                .AddGenericRepository()
                .AddServices()
                .AddHangfire(initializeForTests);

            return services;
        }

        private static IServiceCollection AddDatabase(
            this IServiceCollection services,
            bool initializeForTests = false)
        {
            services.AddDbContext<TradingSystemDbContext>(
                (serviceProvider, dbContextOptionsBuilder) =>
                {
                    var environment = serviceProvider.GetRequiredService<IHostEnvironment>();

                    if (initializeForTests && environment.IsDevelopment())
                    {
                        // Use InMemoryDatabase for testing
                        Console.WriteLine($"Using inMemoryDatabase as storage -> integration tests running: {initializeForTests}");
                        dbContextOptionsBuilder.UseInMemoryDatabase("InMemoryTestDb");
                        dbContextOptionsBuilder.EnableDetailedErrors(true);
                        dbContextOptionsBuilder.EnableSensitiveDataLogging(true);
                    }
                    else
                    {
                        var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
                        
                        dbContextOptionsBuilder.UseSqlServer(databaseOptions.ConnectionString);
                        dbContextOptionsBuilder.EnableDetailedErrors(databaseOptions.EnableDetailedErrors);
                        dbContextOptionsBuilder.EnableSensitiveDataLogging(databaseOptions.EnableSensitiveDataLogging);
                    }

                    // Add logging for development environment
                    if (environment.IsDevelopment())
                    {
                        dbContextOptionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
                    }
                });

            services.AddScoped<IUnitOfWork>(factory => factory.GetRequiredService<TradingSystemDbContext>());

            return services;
        }

        private static IServiceCollection AddGenericRepository(this IServiceCollection services)
            => services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));

        private static IServiceCollection AddServices(this IServiceCollection services)
        {
            return services
                .AddCaching()
                .AddJobServices()
                .AddScoped<IEmailService, EmailService>()
                .AddSingleton<IDomainEventQueue, InMemoryDomainEventQueue>()
                .AddTransient<IDomainEventProcessor, DomainEventProcessor>()
                .AddScoped<IDomainEventDispatcher, DomainEventDispatcher>()
                .AddScoped<IRealTimeNotificationPublisher, SignalRNotificationPublisher>()
                .AddSignalRServices();
        }

        private static IServiceCollection AddSignalRServices(this IServiceCollection services)
        {
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            })
            .AddJsonProtocol(options =>
            {
                options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                options.PayloadSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                options.PayloadSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });

            return services;
        }

        private static IServiceCollection AddJobServices(this IServiceCollection services)
        {
            return services
                .AddTransient<FinancialSettlementJob>()
                .AddTransient<ConfirmTradesJob>()
                .AddTransient<TradeQuotesExpirationJob>(); // TODO: Register DomainEventsProcessingJob
        }

        private static IServiceCollection AddCaching(this IServiceCollection services)
        {
            return services
                .AddDistributedMemoryCache()
                .AddSingleton<ICacheService, CacheService>();
        }

        private static IServiceCollection AddHangfire(this IServiceCollection services, bool initializeForTests = false)
        {
            if (initializeForTests)
            {
                Console.WriteLine($"Skipping Hangfire configuration -> integration tests running: {initializeForTests}");
                return services;
            }

            // Configure Hangfire with SQL Server Storage
            services
                .AddHangfire((provider, config) =>
                {
                    var options = provider.GetRequiredService<IOptions<HangfireOptions>>().Value;

                    config
                        .UseSerilogLogProvider()
                        .UseSqlServerStorage(options.ConnectionString, new SqlServerStorageOptions
                        {
                            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                            QueuePollInterval = TimeSpan.Zero, // Real-time processing
                            UseRecommendedIsolationLevel = true,
                            InactiveStateExpirationTimeout = TimeSpan.FromDays(1)
                        })
                        .UseSerializerSettings(new JsonSerializerSettings()
                        {
                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        });
                })
                .AddHangfireServer();

            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
            {
                Attempts = 0 // Disable retry attempts globally for all jobs
            });

            return services;
        }
    }
}
