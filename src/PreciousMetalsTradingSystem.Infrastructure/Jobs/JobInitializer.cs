using PreciousMetalsTradingSystem.Infrastructure.Jobs.Options;
using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace PreciousMetalsTradingSystem.Infrastructure.Jobs
{
    public static class JobInitializer
    {
        public static void InitializeJobs(IApplicationBuilder app, bool initializeForTests = false)
        {
            if (initializeForTests)
            {
                Console.WriteLine($"Skipping job scheduling -> integration tests running: {initializeForTests}");
                return;
            }

            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope();

            var jobManager = serviceScope.ServiceProvider.GetRequiredService<IRecurringJobManager>();
            var options = serviceScope.ServiceProvider.GetRequiredService<IOptions<HangfireOptions>>().Value;

            jobManager.RemoveAllRecurringJobs();

            jobManager.ScheduleJob<ConfirmTradesJob>(options.ConfirmTradesJobCronExpression);
            jobManager.ScheduleJob<FinancialSettlementJob>(options.FinancialSettlementJobCronExpression);
            jobManager.ScheduleJob<TradeQuotesExpirationJob>(options.TradeQuotesExpirationJobCronExpression);
            jobManager.ScheduleJob<DomainEventsProcessingJob>(options.DomainEventsProcessingJobCronExpression);
        }

        private static void RemoveAllRecurringJobs(this IRecurringJobManager jobManager)
        {
            using var connection = JobStorage.Current.GetConnection();
            var recurringJobs = connection.GetRecurringJobs();

            foreach (var job in recurringJobs)
            {
                jobManager.RemoveIfExists(job.Id);
                Console.WriteLine($"Removed job: {job.Id}");
            }
        }

        private static void ScheduleJob<T>(this IRecurringJobManager jobManager, string cronExpression)
            where T : BaseJob
        {
            if (!string.IsNullOrWhiteSpace(cronExpression))
            {
                jobManager.AddOrUpdate<T>(
                    typeof(T).Name,
                    job => job.ExecuteAsync(CancellationToken.None),
                    cronExpression,
                    new RecurringJobOptions { TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time") }
                );
            }
        }
    }
}
