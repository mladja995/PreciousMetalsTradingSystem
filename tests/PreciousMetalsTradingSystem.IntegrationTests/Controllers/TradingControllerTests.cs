using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.ExecuteQuote;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Commands.RequestQuote;
using PreciousMetalsTradingSystem.Application.Trading.Execution.Models;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Create;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Models;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.WebApi.Common;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text.Json;
using Entities = PreciousMetalsTradingSystem.Domain.Entities;

namespace PreciousMetalsTradingSystem.IntegrationTests.Controllers
{
    [Collection("IntegrationTests Collection")]
    public class TradingControllerTests : IntegrationTestBase
    {
        private const string URL = "/Trading";

        public TradingControllerTests()
        {
        }

        [Fact]
        public async Task SubmitDealerTrade_ShouldReturnCreatedDealerTradeId_AndTradeShouldExistInDb()
        {
            //Arange: DealerTradeCreateCommand request
            var products = GetRepository<Product, ProductId>().StartQuery().Take(2).ToList();
            var initialPositions = new Dictionary<Guid, decimal>();
            
            foreach (var product in products)
            {
                var initialPosition = await GetRepository<ProductLocationPosition, ProductLocationPositionId>()
                    .StartQuery(readOnly: true)
                    .Where(p => p.ProductId == product.Id && p.LocationType == LocationType.SLC && p.Type == PositionType.AvailableForTrading)
                    .OrderByDescending(p => p.TimestampUtc)
                    .FirstOrDefaultAsync();

                initialPositions[product.Id.Value] = initialPosition?.PositionUnits.Value ?? 0;
            }

            var lastBalance = await GetRepository<FinancialTransaction, FinancialTransactionId>().StartQuery(readOnly: true, asSplitQuery: true)
                        .Where(t => t.BalanceType == Domain.Enums.BalanceType.Effective)
                        .OrderByDescending(t => t.TimestampUtc).Select(t => t.Balance.Value).FirstOrDefaultAsync();


            var request = new DealerTradeCreateCommand
            {
                TradeDate = DateTime.UtcNow.ConvertUtcToEst(),
                Location = LocationType.SLC,
                SideType = SideType.Buy,
                AutoHedge = true,
                Note = "Test note",
                Items = products.Select((product, index) => new DealerTradeItemRequest
                {
                    ProductId = product.Id.Value,
                    UnitQuantity = (index + 1) * 5, //  5, 10, 15
                    DealerPricePerOz = 35.5m + index * 100, // Different prices for each product
                    SpotPricePerOz = null,
                }).ToList()
            };

            var sideTypeConvertedTransaction = request.SideType.ToTransactionSideType();
            var sideTypeConvertedPosition = request.SideType.ToPositionSideType();


            // Act: Send the request to create a Dealer Trade
            var response = await Client.PostAsJsonAsync($"{URL}/dealer-trades", request);

            response.EnsureSuccessStatusCode();
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
            var createdTradeId = Guid.Parse(apiResponse!.Data!.ToString()!);

            //Assert
            Assert.NotEqual(Guid.Empty, createdTradeId);

            var createdTrade = await GetRepository<Entities.Trade, TradeId>()
                .GetByIdAsync(
                    id: new TradeId(createdTradeId),
                    includes: x => x.Items );

            Assert.NotNull(createdTrade);
            Assert.Equal(request.Location, createdTrade.LocationType);
            Assert.Equal(request.Note, createdTrade.Note);
            Assert.Equal(request.SideType, createdTrade.Side);
            Assert.Equal(DateOnly.FromDateTime(request.TradeDate), createdTrade.TradeDate);


            foreach (var createdItem in createdTrade.Items)
            {
                var expectedItem = request.Items.SingleOrDefault(i => i.ProductId == createdItem.ProductId.Value);
                Assert.NotNull(expectedItem);
                Assert.Equal(expectedItem.UnitQuantity, createdItem.QuantityUnits);
                Assert.Equal(expectedItem.DealerPricePerOz, createdItem.TradePricePerOz.Value);
            }


            var spotDeferredTrade = await GetRepository<SpotDeferredTrade, SpotDeferredTradeId>()
                                    .StartQuery()
                                    .OrderByDescending(x => x.TimestampUtc)
                                    .FirstOrDefaultAsync();

            Assert.Equal(createdTrade.SpotDeferredTradeId, spotDeferredTrade?.Id);

            foreach (var item in request.Items)
            {
                var updatedPosition = await GetRepository<ProductLocationPosition, ProductLocationPositionId>()
                    .StartQuery(readOnly: true)
                    .Where(p => p.ProductId == new ProductId(item.ProductId) && p.LocationType == LocationType.SLC && p.Type == PositionType.AvailableForTrading)
                    .OrderByDescending(p => p.TimestampUtc)
                    .FirstOrDefaultAsync();

                Assert.NotNull(item);
                Assert.NotNull(updatedPosition);

                // Calculating expecting value
                var initialUnits = initialPositions[item.ProductId];
                var sideTypeMultiplier = request.SideType.ToPositionSideType();
                var expectedUnits = initialUnits + ((int)sideTypeMultiplier * item.UnitQuantity);

                // Assert positions
                Assert.Equal(expectedUnits, updatedPosition.PositionUnits.Value);
            }
            
            var newBalance = await GetRepository<FinancialTransaction, FinancialTransactionId>().StartQuery(readOnly: true, asSplitQuery: true)
                        .Where(t => t.BalanceType == Domain.Enums.BalanceType.Effective)
                        .OrderByDescending(t => t.TimestampUtc).Select(t => t.Balance.Value).FirstOrDefaultAsync();

            var amount = createdTrade.GetTotalAmount();

            //Assert balance
            Assert.Equal((lastBalance + (int)sideTypeConvertedTransaction * amount), newBalance);
        }

        [Fact]
        public async Task SubmitClientTrade_ShouldReturnCreatedTradeId_AndTradeShouldExistInDb()
        {
            // Arrange: Prepare ClientTradeCreateCommand request
            var products = GetRepository<Product, ProductId>().StartQuery().Take(2).ToList();
            var initialPositions = new Dictionary<Guid, decimal>();

            foreach (var product in products)
            {
                var initialPosition = await GetRepository<ProductLocationPosition, ProductLocationPositionId>()
                    .StartQuery(readOnly: true)
                    .Where(p => p.ProductId == product.Id && p.LocationType == LocationType.SLC && p.Type == PositionType.AvailableForTrading)
                    .OrderByDescending(p => p.TimestampUtc)
                    .FirstOrDefaultAsync();

                initialPositions[product.Id.Value] = initialPosition?.PositionUnits.Value ?? 0;
            }

            var lastBalance = await GetRepository<FinancialTransaction, FinancialTransactionId>().StartQuery(readOnly: true, asSplitQuery: true)
                        .Where(t => t.BalanceType == Domain.Enums.BalanceType.Effective)
                        .OrderByDescending(t => t.TimestampUtc).Select(t => t.Balance.Value).FirstOrDefaultAsync();

            var request = new ClientTradeCreateCommand
            {
                TradeDate = DateTime.UtcNow,
                Location = LocationType.SLC,
                SideType = ClientSideType.Buy,
                Note = "Test client trade",
                Items = products.Select((product, index) => new ClientTradeItemRequest
                {
                    ProductId = product.Id.Value,
                    UnitQuantity = (index + 1) * 3, // 3, 6, 9
                    SpotPricePerOz = 40.5m + index * 50 // Different prices for each product
                }).ToList()
            };

            var sideTypeConvertedTransaction = request.SideType.ToSideType().ToTransactionSideType();
            var sideTypeConvertedPosition = request.SideType.ToSideType().ToPositionSideType();

            // Act: Send the request to create a Client Trade
            var response = await Client.PostAsJsonAsync($"{URL}/client-trades", request);

            response.EnsureSuccessStatusCode();
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
            var clientTradeResult = JsonConvert.DeserializeObject<ClientTradeResult>(apiResponse!.Data!.ToString()!);

            // Assert: Validate the created trade
            Assert.NotEqual(Guid.Empty, clientTradeResult!.Id);
            Assert.NotEmpty(clientTradeResult!.TradeNumber);

            var createdTrade = await GetRepository<Entities.Trade, TradeId>()
                .GetByIdAsync(
                    id: new TradeId(clientTradeResult!.Id),
                    includes: x => x.Items);

            Assert.NotNull(createdTrade);
            Assert.Equal(request.Location, createdTrade.LocationType);
            Assert.Equal(request.Note, createdTrade.Note);
            Assert.Equal(request.SideType.ToSideType(), createdTrade.Side);
            Assert.Equal(DateOnly.FromDateTime(request.TradeDate), createdTrade.TradeDate);

            foreach (var createdItem in createdTrade.Items)
            {
                var expectedItem = request.Items.SingleOrDefault(i => i.ProductId == createdItem.ProductId.Value);
                Assert.NotNull(expectedItem);
                Assert.Equal(expectedItem.UnitQuantity, createdItem.QuantityUnits);
                Assert.Equal(expectedItem.SpotPricePerOz, createdItem.SpotPricePerOz.Value);
            }

            foreach (var item in request.Items)
            {
                var updatedPosition = await GetRepository<ProductLocationPosition, ProductLocationPositionId>()
                    .StartQuery(readOnly: true)
                    .Where(p => p.ProductId == new ProductId(item.ProductId) && p.LocationType == LocationType.SLC && p.Type == PositionType.AvailableForTrading)
                    .OrderByDescending(p => p.TimestampUtc)
                    .FirstOrDefaultAsync();

                Assert.NotNull(updatedPosition);

                // Calculating expected position
                var initialUnits = initialPositions[item.ProductId];
                var expectedUnits = initialUnits + ((int)sideTypeConvertedPosition * item.UnitQuantity);
                Assert.Equal(expectedUnits, updatedPosition.PositionUnits.Value);
            }

            var newBalance = await GetRepository<FinancialTransaction, FinancialTransactionId>()
                .StartQuery(readOnly: true, asSplitQuery: true)
                .Where(t => t.BalanceType == Domain.Enums.BalanceType.Effective)
                .OrderByDescending(t => t.TimestampUtc)
                .Select(t => t.Balance.Value)
                .FirstOrDefaultAsync();

            var amount = createdTrade.GetTotalAmount();

            // Assert: Validate balance updates
            Assert.Equal((lastBalance + (int)sideTypeConvertedTransaction * amount), newBalance);
        }

        [Fact]
        public async Task QuoteRequest_ShouldReturnCreatedQuote_AndTradeQuoteShouldExistInDb_AndTradeQuoteItemShouldExistInDb()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };

            Console.WriteLine("Running QuoteRequest_ShouldReturnCreatedQuote_AndTradeQuoteShouldExistInDb_AndTradeQuoteItemShouldExistInDb test!");
            // Arrange: Quote request request
            var quoteRequest = new QuoteRequestCommand
            {
                Location = LocationType.SLC,
                SideType = ClientSideType.Buy,
                Items =
                [
                    new QuoteRequestItem
                    {
                        ProductSKU = "M",
                        QuantityUnits = 1,
                    }
                ]
            };

            // Act: Send quote request
            var response = await Client.PostAsJsonAsync($"{URL}/quotes", quoteRequest);

            // Assert: Verify the response and quote
            response.EnsureSuccessStatusCode();
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();

            var newQuote = JsonConvert.DeserializeObject<Quote?>(((JsonElement)apiResponse!.Data!).GetRawText(), settings);

            Assert.NotNull(newQuote);
            Assert.NotEqual(Guid.Empty, Guid.Parse(newQuote.Id!.ToString()!));

            // Verify the quote exists in the database
            var addedQuote = await GetRepository<TradeQuote, TradeQuoteId>()
                .GetByIdAsync(
                    id: new TradeQuoteId(newQuote.Id!),
                    includes: x => x.Items);

            Assert.NotNull(addedQuote);
            Assert.Equal(newQuote.IssuedAtUtc, addedQuote!.IssuedAtUtc);
            Assert.Equal(newQuote.Side.ToSideType(), addedQuote.Side);
            Assert.Equal(newQuote.ExpiriesAtUtc, addedQuote.ExpiresAtUtc);
            Assert.Equal(newQuote.Location, addedQuote.LocationType);
            Assert.Equal(newQuote.Id, addedQuote.Id);

            var addedQuoteItem = addedQuote.Items.SingleOrDefault();
            var requestedItem = quoteRequest.Items.SingleOrDefault();

            Assert.NotNull(addedQuoteItem);
            Assert.Equal(requestedItem!.QuantityUnits, addedQuoteItem!.QuantityUnits);

            Console.WriteLine("Running QuoteExecute_ShouldReturnCreatedQuote_AndTradeQuoteShouldExistInDb_AndTradeQuoteItemShouldExistInDb test!");
            // Arrange: Quote Execute request
            var quoteExecuteRequest = new QuoteExecuteCommand { Id = addedQuote.Id , ReferenceNumber = "reference number" };

            // Act: Send QuoteExecute
            var quoteExecuteResponse = await Client.PatchAsJsonAsync($"{URL}/quotes/{addedQuote.Id!.ToString()}", quoteExecuteRequest);

            // Assert: Verify the response and quote
            quoteExecuteResponse.EnsureSuccessStatusCode();
            var quoteExecuteApiResponse = await quoteExecuteResponse.Content.ReadFromJsonAsync<ApiResponse>();
            var newQuoteExecute = JsonConvert.DeserializeObject<QuoteExecuteCommandResult?>(((JsonElement)quoteExecuteApiResponse!.Data!).GetRawText(), settings);

            Assert.NotNull(newQuoteExecute);
            Assert.NotEqual(Guid.Empty, Guid.Parse(newQuoteExecute.TradeId!.ToString()!));
            Assert.NotEqual(string.Empty, newQuoteExecute.TradeNumber!);
            Assert.Equal(quoteExecuteRequest.Id!, newQuoteExecute.Quote.Id);
            Assert.Equal(addedQuote.IssuedAtUtc!, newQuoteExecute.Quote.IssuedAtUtc);
            Assert.Equal(addedQuote.ExpiresAtUtc!, newQuoteExecute.Quote.ExpiriesAtUtc);
            Assert.Equal(addedQuote.LocationType!, newQuoteExecute.Quote.Location);
            Assert.Equal(quoteRequest.SideType, newQuoteExecute.Quote.Side);
            Assert.NotNull(newQuoteExecute.Quote.Items);
            Assert.Equal(quoteRequest.Items.Count(), newQuoteExecute.Quote.Items.Count());

            var QuoteExecuteItem = newQuoteExecute.Quote.Items.SingleOrDefault();

            // Verify the trade exists in the database
            var tradeId = new TradeId(newQuoteExecute.TradeId!);
            var tradeList = await GetRepository<Entities.Trade, TradeId>().StartQuery()
                .Include(x => x.FinancialTransactions)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Product) // ???????
                .Where(x => x.Id == tradeId)
                .ToListAsync(default);

            var trade = tradeList.FirstOrDefault();

            Assert.NotNull(trade);
            Assert.NotEqual(Guid.Empty, Guid.Parse(trade.Id!.ToString()!));
            Assert.NotEqual(Guid.Empty, Guid.Parse(trade.SpotDeferredTradeId!.ToString()!));
            Assert.NotEqual(Guid.Empty, Guid.Parse(trade.TradeQuoteId!.ToString()!));
            Assert.NotEqual(string.Empty, trade.TradeNumber!);
            Assert.Equal(newQuoteExecute!.TradeId, trade.Id);
            Assert.Equal(newQuoteExecute!.TradeNumber, trade.TradeNumber);
            Assert.Equal(addedQuote!.Side, trade.Side);
            Assert.Equal(TradeType.ClientTrade, trade.Type);
            Assert.Equal(addedQuote.LocationType, trade.LocationType);
            Assert.False(trade.IsPositionSettled);
            Assert.False(trade.IsFinancialSettled);

            Assert.Null(trade.PositionSettledOnUtc);
            Assert.Null(trade.FinancialSettledOnUtc);
            Assert.Null(trade.ConfirmedOnUtc);

            var tradeItem = trade.Items.SingleOrDefault();

            Assert.NotNull(tradeItem);
            Assert.NotEqual(Guid.Empty, Guid.Parse(tradeItem.TradeQuoteItemId!.ToString()!));
            Assert.NotEqual(Guid.Empty, Guid.Parse(tradeItem.ProductId!.ToString()!));
            Assert.Equal(tradeItem!.QuantityUnits, addedQuoteItem.QuantityUnits);
            Assert.Equal(tradeItem!.SpotPricePerOz, addedQuoteItem.SpotPricePerOz);
            Assert.Equal(tradeItem!.TradePricePerOz, addedQuoteItem.SpotPricePerOz);
            Assert.Equal(tradeItem!.PremiumPerOz, addedQuoteItem.PremiumPricePerOz);
            Assert.Equal(tradeItem!.EffectivePricePerOz, addedQuoteItem.EffectivePricePerOz);

            // Verify the spot deferred trade exists in the database
            var locationHedgingAccountConfiguration = await GetRepository<LocationHedgingAccountConfiguration, LocationHedgingAccountConfigurationId>().GetByIdOrThrowAsync(
                   id: new LocationHedgingAccountConfigurationId(trade.LocationType),
                   cancellationToken: default,
                   includes: x => x.HedgingAccount);

            var hedgingAccount = locationHedgingAccountConfiguration.HedgingAccount;

            Assert.NotNull(hedgingAccount);

            var spotDeferredTrade = await GetRepository<SpotDeferredTrade, SpotDeferredTradeId>().GetByIdOrThrowAsync(
                   id: new SpotDeferredTradeId(trade.SpotDeferredTradeId),
                   cancellationToken: default,
                   includes: x => x.Items);

            Assert.NotNull(spotDeferredTrade);
            Assert.Equal(hedgingAccount.Id, spotDeferredTrade.HedgingAccountId);
            Assert.NotNull(spotDeferredTrade.TradeConfirmationReference);
            Assert.Equal(addedQuote.Side.ToOppositeSide(), spotDeferredTrade.Side);
            Assert.False(spotDeferredTrade.IsManual);

            var spotDeferredTradeItem = spotDeferredTrade.Items.FirstOrDefault();

            Assert.NotNull(spotDeferredTradeItem);

            var addedQuoteTradeItem = GetQuoteTradeItems(addedQuote).FirstOrDefault();

            Assert.NotNull(addedQuoteTradeItem);
            Assert.Equal(trade.SpotDeferredTradeId, spotDeferredTradeItem.SpotDeferredTradeId);
            Assert.Equal(addedQuoteTradeItem.Metal, spotDeferredTradeItem.Metal);
            Assert.Equal(addedQuoteTradeItem.PricePerOz, spotDeferredTradeItem.PricePerOz);
            Assert.Equal(addedQuoteTradeItem.QuantityOz, spotDeferredTradeItem.QuantityOz);
            Assert.Equal(addedQuoteTradeItem.TotalAmount, spotDeferredTradeItem.TotalAmount);

            // Verify the financial transaction exists in the database
            var transaction = trade.FinancialTransactions.SingleOrDefault();

            Assert.NotNull(transaction);
            Assert.Equal(BalanceType.Effective, transaction.BalanceType);
            Assert.Equal(addedQuote!.Side.ToTransactionSideType(), transaction.SideType);
            Assert.Equal(trade.Type.ToActivityType(), transaction.ActivityType);
            Assert.Equal(trade.GetTotalAmount(), transaction.Amount);

            var privTransaction = await GetRepository<FinancialTransaction, FinancialTransactionId>().StartQuery(readOnly: true)
               .Where(x => x.BalanceType == BalanceType.Effective && transaction.TimestampUtc > x.TimestampUtc)
               .OrderByDescending(x => x.TimestampUtc)
               .FirstOrDefaultAsync(default);

            Assert.NotNull(privTransaction);
            Assert.NotNull(transaction);
            Assert.Equal(privTransaction.Balance + (int)transaction.SideType * transaction.Amount, transaction.Balance);


            // Verify the product location position is changed in the database
            var positions = await GetRepository<ProductLocationPosition, ProductLocationPositionId>().StartQuery(readOnly: true)
                .Where(p => p.LocationType == trade.LocationType && p.TradeId == trade.Id)
                .Include(p => p.Product)
                .GroupBy(p => new { p.Type, p.ProductId })
                .Select(g => g.OrderByDescending(p => p.TimestampUtc).FirstOrDefault())
                .ToListAsync(default);

            var position = positions.FirstOrDefault();

            Assert.NotNull(position);
            Assert.Equal(addedQuoteItem.ProductId, position.ProductId);
            Assert.Equal(trade.Id, position.TradeId);
            Assert.Equal(addedQuote.LocationType, position.LocationType);
            Assert.Equal(addedQuoteItem.QuantityUnits, position.QuantityUnits);
            Assert.Equal(PositionType.AvailableForTrading, position.Type);
            Assert.Equal(addedQuote.Side.ToPositionSideType(), position.SideType);

            var privPosition = await GetRepository<ProductLocationPosition, ProductLocationPositionId>().StartQuery(readOnly: true)
                .Where(p => p.TimestampUtc < position.TimestampUtc && p.ProductId == position.ProductId && p.LocationType == position.LocationType && p.Type == position.Type)
                .GroupBy(p => p.Type)
                .Select(g => g.OrderByDescending(p => p.TimestampUtc).FirstOrDefault())
                .SingleOrDefaultAsync(default);

            Assert.NotNull(privPosition);
            Assert.Equal(privPosition.ProductId, privPosition.ProductId);
            Assert.Equal(privPosition.PositionUnits + (int)position.SideType * position.QuantityUnits, position.PositionUnits);

        }

        private List<SpotDeferredTradeItem> GetQuoteTradeItems(TradeQuote tradeQuote) {

            var quantitiesPerMetalType = tradeQuote.GetQuantityPerMetalType();
            var spotPricesPerMetalType = tradeQuote.GetSpotPricesPerMetalType();

            return quantitiesPerMetalType
                .Join(spotPricesPerMetalType,
                    i => i.Key,
                    p => p.Key,
                    (i, p) => new { Quantity = i, Price = p })
                .Select(item => SpotDeferredTradeItem.Create(
                    item.Quantity.Key,
                    item.Price.Value,
                    item.Quantity.Value)
                )
                .ToList();
        }
    }
}
