using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;

namespace PreciousMetalsTradingSystem.Application.ProductsCatalog.Exceptions
{
    public class ProductSkuIsNotUniqueException : ConflictException
    {
        public ProductSkuIsNotUniqueException(string message, string code = "CONFLICT_ERROR") 
            : base(message, code)
        {
        }

        public ProductSkuIsNotUniqueException(SKU sku)
            : base($"A product with SKU '{sku.Value}' already exists.")
        {
        }
    }
}
