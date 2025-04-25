using PreciousMetalsTradingSystem.Application.Common.CustomAttributes;
using PreciousMetalsTradingSystem.Application.ProductsCatalog.Models;
using PreciousMetalsTradingSystem.Domain.Enums;
using MediatR;
using System.Text.Json.Serialization;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Commands.Update
{
    public class ProductUpdateCommand : IRequest
    {
        [OpenApiExclude]
        public Guid Id { get; set; }
        public required string ProductSKU { get; init; }
        public required string ProductName { get; init; }
        public required MetalType MetalType { get; init; }
        public required decimal WeightInOz { get; init; }
        public required bool IsAvailableForTrading { get; init; }
        public IEnumerable<ProductLocationConfiguration>? LocationConfigurations { get; init; }
    }
}