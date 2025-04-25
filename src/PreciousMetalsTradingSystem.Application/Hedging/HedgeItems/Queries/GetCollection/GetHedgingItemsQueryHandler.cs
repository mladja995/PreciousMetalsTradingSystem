using MediatR;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using DomainEntities = PreciousMetalsTradingSystem.Domain.Entities;
using HedgeItemsModels = PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Models;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PreciousMetalsTradingSystem.Domain.Enums;

namespace PreciousMetalsTradingSystem.Application.Hedging.HedgeItems.Queries.GetCollection
{
    public class GetHedgingItemsQueryHandler : IRequestHandler<GetHedgingItemsQuery, GetHedgingItemsQueryResult>
    {
        private readonly IRepository<DomainEntities.HedgingItem, HedgingItemId> _repository;

        public GetHedgingItemsQueryHandler(IRepository<DomainEntities.HedgingItem, HedgingItemId> repository) 
        {
            _repository = repository;
        }

        public async Task<GetHedgingItemsQueryResult> Handle(GetHedgingItemsQuery request, CancellationToken cancellationToken)
        {
            var isSortByAmount = !string.IsNullOrWhiteSpace(request.Sort) && request.Sort.Contains("Amount");

            var hedgingItemsQuery = _repository.StartQuery(readOnly: true)
                .Where(x =>
                    x.HedgingAccountId.Equals(new HedgingAccountId(request.AccountId!))
                    && x.Type == request.Type)
                .Select(x => new HedgeItemsModels.HedgingItem
                {
                    Id = x.Id,
                    Date = x.HedgingItemDate,
                    Type = x.Type,
                    SideType = x.SideType,
                    Amount = isSortByAmount ? ((int)x.SideType) * x.Amount : x.Amount,
                    Note = x.Note,
                }).Sort(!string.IsNullOrWhiteSpace(request.Sort) ? request.Sort : "-Date"); // Apply sorting // TODO: Refactor default sort param


            // Fetch the total count
            int totalCount = await hedgingItemsQuery.CountAsync(cancellationToken);

            // Apply pagination and fetch the result
            var hedgingItemsDtos = await hedgingItemsQuery
                .Skip((request.PageNumber - 1) * request.PageSize!.Value)
                .Take(request.PageSize.Value!)
                .ToListAsync(cancellationToken);

            if (isSortByAmount)
            {
                hedgingItemsDtos = hedgingItemsDtos.Select(x => new HedgeItemsModels.HedgingItem
                {
                    Id = x.Id,
                    Date = x.Date,
                    Type = x.Type,
                    SideType = x.SideType,
                    Amount = Math.Abs(x.Amount),
                    Note = x.Note,
                }).ToList();
            }

            return new GetHedgingItemsQueryResult(
                hedgingItemsDtos,
                totalCount,
                request.PageNumber,
                request.PageSize.Value!);
        }
    }
}
