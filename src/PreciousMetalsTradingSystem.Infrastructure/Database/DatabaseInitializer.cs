using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PreciousMetalsTradingSystem.Infrastructure.Database
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeDatabase(IApplicationBuilder app, bool initializeForTests = false)
        {
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope();

            var context = serviceScope.ServiceProvider.GetRequiredService<TradingSystemDbContext>();
            var env = app.ApplicationServices.GetRequiredService<IHostEnvironment>();

            await Seed(context, env, initializeForTests);
        }

        private static async Task Seed(DbContext context, IHostEnvironment env, bool initializeForTests = false)
        {
            // Skip migrations for in-memory database
            if (context.Database.IsRelational())
            {
                await context.Database.MigrateAsync();
            }

            if (initializeForTests && env.IsDevelopment())
            {
                await SeedTestData(context);
            }
        }

        private static async Task SeedTestData(DbContext context)
        {
            await InitializeTestProductsAsync(context);
            await InitilazeTestFinancialsRecordsAsync(context);
            await InitializeTestTradesAsync(context);
            await InitializeTestPositionsAsync(context);
            await InitializeTestHedgingAccountsAsync(context);
            await InitializeTestLocationHedgingConfigurationAsync(context);
        }
        private static async Task InitializeTestHedgingAccountsAsync(DbContext context)
        {
            await context.Set<HedgingAccount>().AddRangeAsync(GetTestHedgingAccounts());
            await context.SaveChangesAsync();
        }
        private static async Task InitializeTestLocationHedgingConfigurationAsync(DbContext context)
        {
            await context.Set<LocationHedgingAccountConfiguration>().AddRangeAsync(GetTestLocationHedgingConfiguration(context));
            await context.SaveChangesAsync();
        }
        private static async Task InitializeTestProductsAsync(DbContext context)
        {
            await context.Set<Product>().AddRangeAsync(GetTestProducts());
            await context.SaveChangesAsync();
        }

        private static async Task InitilazeTestFinancialsRecordsAsync(DbContext context)
        {
            await context.Set<FinancialAdjustment>().AddAsync(GetTestFinancialAdjustment());
            await context.SaveChangesAsync();
        }

        private static async Task InitializeTestTradesAsync(DbContext context)
        {
            await context.Set<Trade>().AddRangeAsync(await GetTestTradesAsync(context));
            await context.SaveChangesAsync();               
        }

        private static async Task InitializeTestPositionsAsync(DbContext context)
        {
            await context.Set<ProductLocationPosition>().AddRangeAsync(await GetTestProductLocationPositionsAsync(context));
            await context.SaveChangesAsync();     
        }

        private static IEnumerable<Product> GetTestProducts()
        {
            var product1 = Product.Create(
                name: "1 oz Silver Britannia Coin (Common Date)",
                sku: new SKU("M"),
                weightInOz: new Weight(1.0000m),
                metalType: MetalType.XAG,
                isAvailable: true
            );

            var product1LocationConfig = ProductLocationConfiguration.Create(
                locationType: LocationType.SLC,
                premiumUnitType: PremiumUnitType.Dollars,
                buyPremium: new Premium(0.70m),
                sellPremium: new Premium(1.50m),
                isAvailableForBuy: true,
                isAvailableForSell: true
            );

            //product1LocationConfig.SetProduct(product1.Id);
            product1.AddLocationConfiguration(product1LocationConfig);

            var product2 = Product.Create(
                name: "1 oz American Gold Buffalo Coin",
                sku: new SKU("X"),
                weightInOz: new Weight(1.0000m),
                metalType: MetalType.XAU,
                isAvailable: true
            );

            var product2LocationConfig = ProductLocationConfiguration.Create(
                locationType: LocationType.SLC,
                premiumUnitType: PremiumUnitType.Percentage,
                buyPremium: new Premium(1.30m),
                sellPremium: new Premium(1.70m),
                isAvailableForBuy: true,
                isAvailableForSell: true
            );

            product2.AddLocationConfiguration(product2LocationConfig);

            return [product1, product2];
        }

        private static FinancialAdjustment GetTestFinancialAdjustment()
        {
            var adjustment = FinancialAdjustment.Create(
                date: DateOnly.FromDateTime(DateTime.UtcNow),
                sideType: TransactionSideType.Credit,
                amount: new Money(1000000m),
                note: "Initial adjustment for testing");

            var initialBalance = new FinancialBalance(0m);

            // Create FinancialTransactions for Effective Balance
            var effectiveTransaction = FinancialTransaction.Create(
                sideType: TransactionSideType.Credit,
                balanceType: BalanceType.Effective,
                activityType: ActivityType.Adjustment,
                amount: new Money(1000000m),
                relatedActivity: adjustment.Id,
                currentBalance: initialBalance);

            // Create FinancialTransactions for Actual Balance
            var actualTransaction = FinancialTransaction.Create(
                sideType: TransactionSideType.Credit,
                balanceType: BalanceType.Actual,
                activityType: ActivityType.Adjustment,
                amount: new Money(1000000m),
                relatedActivity: adjustment.Id,
                currentBalance: initialBalance);

            // Link Transactions to the Adjustment
            adjustment.AddFinancialTransaction(effectiveTransaction);
            adjustment.AddFinancialTransaction(actualTransaction);

            return adjustment;
        }

        private static async Task<IEnumerable<Trade>> GetTestTradesAsync(DbContext context)
        {
            var products = await context.Set<Product>().ToListAsync();

            if (products.Count == 0)
            {
                return [];
            }

            var trades = new List<Trade>();

            foreach (var product in products)
            {
                var isGold = product.MetalType == MetalType.XAU;
                var spotPricePerOz = isGold ? new Money(1900) : new Money(25); // Example prices for gold and silver
                var tradePricePerOz = isGold ? new Money(2000) : new Money(30); // Example trade prices
                var premiumPerOz = new Premium(tradePricePerOz.Value - spotPricePerOz.Value);
                var effectivePricePerOz = tradePricePerOz;

                var locationType = product.LocationConfigurations.FirstOrDefault()?.LocationType ?? LocationType.SLC;

                var trade = Trade.Create(
                    tradeType: TradeType.ClientTrade,
                    sideType: SideType.Buy,
                    locationType: locationType,
                    tradeDate: DateOnly.FromDateTime(DateTime.UtcNow),
                    financialsSettleOn: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)),
                    note: $"Trade for {product.Name}");

                var tradeItem = TradeItem.Create(
                    tradeSide: SideType.Buy,
                    productId: product.Id,
                    productWeightInOz: product.WeightInOz,
                    quantityUnits: new QuantityUnits(100),
                    spotPricePerOz: spotPricePerOz,
                    tradePricePerOz: tradePricePerOz,
                    premiumPerOz: premiumPerOz,
                    effectivePricePerOz: effectivePricePerOz);

                trade.AddItem(tradeItem);

                trades.Add(trade);
            }

            return trades;
        }

        private static async Task<IEnumerable<ProductLocationPosition>> GetTestProductLocationPositionsAsync(DbContext context)
        {
            var trades = await context.Set<Trade>().Include(x => x.Items).ToListAsync();

            if (trades.Count == 0)
            {
                return [];
            }

            var positions = new List<ProductLocationPosition>();

            foreach (var trade in trades)
            {
                foreach (var tradeItem in trade.Items)
                {
                    var position = ProductLocationPosition.Create(
                        productId: tradeItem.ProductId,
                        relatedTradeId: trade.Id,
                        location: trade.LocationType,
                        sideType: trade.Side == SideType.Buy ? PositionSideType.In : PositionSideType.Out,
                        type: PositionType.AvailableForTrading,
                        quantityUnits: tradeItem.QuantityUnits,
                        currentPositionQuantityUnits: new PositionQuantityUnits(0));

                    positions.Add(position);
                }
            }

            return positions;
        }

        private static IEnumerable<HedgingAccount> GetTestHedgingAccounts()
        {

            var hedgingAccount = HedgingAccount.Create(new HedgingAccountName("TestAccount"), new HedgingAccountCode("12345"));
            
            return [hedgingAccount];
        }

        private static IEnumerable<LocationHedgingAccountConfiguration> GetTestLocationHedgingConfiguration(DbContext context)
        {
            var hedgingAccounts = context.Set<HedgingAccount>().ToList();

            var locationHedgingConfiguration = new List<LocationHedgingAccountConfiguration>();

            foreach (var account in hedgingAccounts)
            {
                
                var hedgeAccount = LocationHedgingAccountConfiguration.Create(LocationType.SLC, account.Id);
                locationHedgingConfiguration.Add(hedgeAccount);
            }
            return locationHedgingConfiguration;
        }
    }
}
