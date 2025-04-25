using System.Net.Http.Json;
using PreciousMetalsTradingSystem.Application.Financials.Adjustments.Commands.Create;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.WebApi.Common;
using Microsoft.EntityFrameworkCore;

namespace PreciousMetalsTradingSystem.IntegrationTests.Controllers
{
    [Collection("IntegrationTests Collection")]
    public class FinancialsControllerTests : IntegrationTestBase
    {
        private const string URL = "/Financials/adjustments";

        public FinancialsControllerTests()
        {
        }

        [Fact]
        public async Task SubmitFinancialAdjustment()
        {

            // Arrange:
            var previousBalances = await GetBalances();


            // Create a new financial adjustment request
            var financialsAdjustmentCreateCommand = new FinancialsAdjustmentCreateCommand
            {
               Amount = 135000m,
               Date = DateTime.Now,
               Note = "Test",
               SideType = TransactionSideType.Debit
            };

            // Act: Send the request to create a product
            var response = await Client.PostAsJsonAsync($"{URL}", financialsAdjustmentCreateCommand);

            // Assert: Verify the response and product creation
            response.EnsureSuccessStatusCode();
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
            var financialAdjustmentId = Guid.Parse(apiResponse!.Data!.ToString()!);

            Assert.NotEqual(Guid.Empty, financialAdjustmentId);

            var submittedFinancialAdjustment = await GetRepository<FinancialAdjustment, FinancialAdjustmentId>()
              .GetByIdAsync(
                  id: new FinancialAdjustmentId(financialAdjustmentId),
                  includes: x => x.FinancialTransactions);
            

            Assert.NotNull(submittedFinancialAdjustment);
            Assert.Equal(2, submittedFinancialAdjustment.FinancialTransactions.Count);

            var actualTransaction = submittedFinancialAdjustment.FinancialTransactions.Single(x => x.BalanceType == BalanceType.Actual);
            Assert.NotNull(actualTransaction);
            Assert.Equal(financialsAdjustmentCreateCommand.Amount, actualTransaction.Amount);

            var transactionEffective = submittedFinancialAdjustment.FinancialTransactions.Single(x => x.BalanceType == BalanceType.Effective);
            Assert.NotNull(transactionEffective);
            Assert.Equal(financialsAdjustmentCreateCommand.Amount, transactionEffective.Amount);

            var balances = await GetBalances();
            Assert.Equal(actualTransaction.Balance, balances.Item1);
            Assert.Equal(transactionEffective.Balance, balances.Item2);

            Assert.Equal(previousBalances.Item1 - actualTransaction.Amount, balances.Item1);
            Assert.Equal(previousBalances.Item2 - actualTransaction.Amount, balances.Item2);
        }

        private async Task<Tuple<decimal, decimal>> GetBalances()
        {
            var balances = await GetRepository<FinancialTransaction, FinancialTransactionId>()
                      .StartQuery(readOnly: true, asSplitQuery: true)
                      .Where(t => t.BalanceType == Domain.Enums.BalanceType.Actual ||
                                  t.BalanceType == Domain.Enums.BalanceType.Effective)
                      .GroupBy(t => t.BalanceType)
                      .Select(g => new
                      {
                          BalanceType = g.Key,
                          LatestBalance = g.OrderByDescending(t => t.TimestampUtc).Select(t => t.Balance.Value).FirstOrDefault()
                      })
                      .ToListAsync();

            var actualBalance = balances.FirstOrDefault(b => b.BalanceType == Domain.Enums.BalanceType.Actual)?.LatestBalance ?? 0;
            var availableForTradingBalance = balances.FirstOrDefault(b => b.BalanceType == Domain.Enums.BalanceType.Effective)?.LatestBalance ?? 0;
            return new Tuple<decimal, decimal>(actualBalance, availableForTradingBalance); 
        }
    }
}
