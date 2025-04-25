using PreciousMetalsTradingSystem.Application.ProductsCatalog.Models;
using PreciousMetalsTradingSystem.Domain.Enums;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Commands.Create
{
    public class ProductCreateCommand : IRequest<Guid>
    {
        public required string ProductSKU { get; init; }
        public required string ProductName { get; init; }
        public required MetalType MetalType { get; init; }
        public required decimal WeightInOz { get; init; }
        public required bool IsAvailableForTrading { get; init; }
        public IEnumerable<ProductLocationConfiguration>? LocationConfigurations { get; init; }
    }
}