using PreciousMetalsTradingSystem.Application.ProductsCatalog.Commands.Create;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.WebApi.Common;
using System.Net.Http.Json;

namespace PreciousMetalsTradingSystem.IntegrationTests.Controllers
{
    [Collection("IntegrationTests Collection")]
    public class ProductsControllerTests : IntegrationTestBase
    {
        private const string URL = "/Products";

        public ProductsControllerTests()
        {
        }

        [Fact]
        public async Task CreateProduct_ShouldReturnCreatedProductId_AndProductShouldExistInDb()
        {
            // Arrange: Create a new product request
            var newProductRequest = new ProductCreateCommand
            {
                ProductSKU = "TESTSKU123",
                ProductName = "Test Product",
                MetalType = MetalType.XAU,
                WeightInOz = 1.0m,
                IsAvailableForTrading = true,
                LocationConfigurations =
                [
                    new Application.ProductsCatalog.Models.ProductLocationConfiguration
                    {
                        Location = LocationType.SLC,
                        PremiumUnitType = PremiumUnitType.Dollars,
                        BuyPremium = 1.5m,
                        SellPremium = 2.0m,
                        IsAvailableForBuy = true,
                        IsAvailableForSell = true
                    }
                ]
            };

            // Act: Send the request to create a product
            var response = await Client.PostAsJsonAsync(URL, newProductRequest);

            // Assert: Verify the response and product creation
            response.EnsureSuccessStatusCode();
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse>();
            var createdProductId = Guid.Parse(apiResponse!.Data!.ToString()!);

            Assert.NotEqual(Guid.Empty, createdProductId);

            // Verify the product exists in the database
            var createdProduct = await GetRepository<Product, ProductId>()
                .GetByIdAsync(
                    id: new ProductId(createdProductId),
                    includes: x => x.LocationConfigurations);

            Assert.NotNull(createdProduct);
            Assert.Equal(newProductRequest.ProductName, createdProduct!.Name);
            Assert.Equal(newProductRequest.ProductSKU, createdProduct.SKU.Value);
            Assert.Equal(newProductRequest.MetalType, createdProduct.MetalType);
            Assert.Equal(newProductRequest.WeightInOz, createdProduct.WeightInOz.Value);
            Assert.Equal(newProductRequest.IsAvailableForTrading, createdProduct.IsAvailable);

            var configuration = createdProduct.LocationConfigurations.SingleOrDefault();

            Assert.NotNull(configuration);
            Assert.Equal(LocationType.SLC, configuration!.LocationType);
            Assert.Equal(PremiumUnitType.Dollars, configuration.PremiumUnitType);
            Assert.Equal(1.5m, configuration.BuyPremium.Value);
            Assert.Equal(2.0m, configuration.SellPremium.Value);
            Assert.True(configuration.IsAvailableForBuy);
            Assert.True(configuration.IsAvailableForSell);
        }
    }
}
