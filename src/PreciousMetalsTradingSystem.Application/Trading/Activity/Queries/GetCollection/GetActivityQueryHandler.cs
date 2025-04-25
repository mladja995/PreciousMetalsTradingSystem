using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using Microsoft.EntityFrameworkCore;
using MediatR;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Trading.Activity.Models;

namespace PreciousMetalsTradingSystem.Application.Trading.Activity.Queries.GetCollection
{
    public class GetActivityQueryHandler : IRequestHandler<GetActivityQuery, GetActivityQueryResult>
    {
        private readonly IRepository<Trade, TradeId> _repository;
        public GetActivityQueryHandler(IRepository<Trade, TradeId> repository)
        {
            _repository = repository;
        }

        public async Task<GetActivityQueryResult> Handle(GetActivityQuery request, CancellationToken cancellationToken)
        {
            var fromDate = DateOnly.FromDateTime(request.FromDate ?? DateTime.MinValue);
            var toDate = DateOnly.FromDateTime(request.ToDate ?? DateTime.MaxValue);
            var fromFinancialSettledDate = DateOnly.FromDateTime(request.FromFinancialSettleOnDate ?? DateTime.MinValue);
            var toFinancialSettledDate = DateOnly.FromDateTime(request.ToFinancialSettleOnDate ?? DateTime.MaxValue);
            var fromLastUpdatedDate = request.FromLastUpdatedUtc ?? DateTime.MinValue;
            var toLastUpdatedDate = request.ToLastUpdatedUtc ?? DateTime.MaxValue;
            var tradesQuery = _repository.StartQuery(readOnly: true)
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .Where(x =>
                    x.TradeDate >= fromDate
                    && x.TradeDate <= toDate
                    && (request.Location == null || x.LocationType == request.Location)
                    && (request.IsPositionSettled == null || x.IsPositionSettled == request.IsPositionSettled)
                    && (request.SideType == null || x.Side == request.SideType)
                    && x.FinancialSettleOn >= fromFinancialSettledDate
                    && x.FinancialSettleOn <= toFinancialSettledDate
                    && x.LastUpdatedOnUtc >= fromLastUpdatedDate
                    && x.LastUpdatedOnUtc <= toLastUpdatedDate
                    && (request.TradeNumber == null || x.TradeNumber.Contains(request.TradeNumber.Trim(), StringComparison.OrdinalIgnoreCase))
                   )
                .SelectMany(t => t.Items
                    .Where(item => request.ProductSKU == null || item.Product.SKU == request.ProductSKU)
                    .Select(item => new ActivityItemResult
                    {
                        Id = t.Id.Value,
                        TradeDate = t.TradeDate,
                        TimestampUtc = t.TimestampUtc,
                        TradeNumber = t.TradeNumber,
                        Type = t.Type,
                        SideType = t.Side,
                        Location = t.LocationType,
                        LastUpdatedOnUtc = t.LastUpdatedOnUtc,

                        ProductSKU = item.Product.SKU,
                        ProductName = item.Product.Name,
                        IsPositionSettled = t.IsPositionSettled,
                        IsFinancialSettled = t.IsFinancialSettled,
                        FinancialSettledOn = t.FinancialSettleOn,
                        UnitQuantity = item.QuantityUnits,
                        PricePerUnit = item.TotalEffectivePrice / item.QuantityUnits,
                        Amount = item.TotalEffectivePrice,
                    }));

            int totalCount = await tradesQuery.CountAsync(cancellationToken);

            var itemsDto = await tradesQuery
                .Sort(!string.IsNullOrWhiteSpace(request.Sort) ? request.Sort : "-TradeDate") // TODO: Refactor default sort param
                .Skip((request.PageNumber - 1) * request.PageSize!.Value)
                .Take(request.PageSize!.Value)
                .ToListAsync(cancellationToken);

            return new GetActivityQueryResult(itemsDto, totalCount, request.PageNumber, request.PageSize!.Value);
        }
    }
}
