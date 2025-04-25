using PreciousMetalsTradingSystem.Application.Common.Models;
using PreciousMetalsTradingSystem.Domain.Enums;
using MediatR;

namespace PreciousMetalsTradingSystem.Application.Trading.Activity.Queries.GetCollection
{
    public class GetActivityQuery : PaginatedQuery, IRequest<GetActivityQueryResult>
    {
        public DateTime? FromDate { get; init; }
        public DateTime? ToDate { get; init; }
        public LocationType? Location { get; init; }
        public string? ProductSKU { get; init; }
        public string? TradeNumber  { get; init; }
        public bool? IsPositionSettled { get; init; }
        public SideType? SideType { get; init; }
        public DateTime? FromLastUpdatedUtc { get; init; }
        public DateTime? ToLastUpdatedUtc { get; init; }
        public DateTime? FromFinancialSettleOnDate { get; init; }
        public DateTime? ToFinancialSettleOnDate { get; init; }
    }
}
