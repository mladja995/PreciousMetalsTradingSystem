using PreciousMetalsTradingSystem.Application.Common.Exceptions;
using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Inventory.Services;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Enums;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Throw;
using PreciousMetalsTradingSystem.Application.Common.Extensions;
using PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Settle;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Create
{
    public class TradeSettleCommandHandler : IRequestHandler<TradeSettleCommand>
    {
        private readonly IRepository<Trade, TradeId> _tradeRepository;
        private readonly IRepository<ProductLocationPosition, ProductLocationPositionId> _productLocationRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IUnitOfWork _unitOfWork;

        public TradeSettleCommandHandler(
            IUnitOfWork unitOfWork,
            IRepository<Trade, TradeId> tradeRepository,
            IRepository<ProductLocationPosition, ProductLocationPositionId> productLocationRepository,
            IInventoryService inventoryService)
        {
            _unitOfWork = unitOfWork;
            _tradeRepository = tradeRepository;
            _productLocationRepository = productLocationRepository;
            _inventoryService = inventoryService;            
        }

        public async Task Handle(TradeSettleCommand request, CancellationToken cancellationToken)
        {
            var trade = await _tradeRepository.StartQuery(readOnly: false)
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
            .SingleOrDefaultAsync(x => x.Id.Equals(new TradeId(request.Id)));

            trade.ThrowIfNull(() => new NotFoundException(nameof(Trade), request.Id));

            trade.MarkAsSettled();
            await CreateAndSubmitPositionsAsync(trade, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        private async Task<IEnumerable<ProductLocationPosition>> CreateAndSubmitPositionsAsync(
            Trade trade,
            CancellationToken cancellationToken)
        {
            List<ProductLocationPosition> positions = [];

            foreach (var item in trade.Items)
            {
                var position = await _inventoryService.CreatePositionAsync(
                    item.ProductId,
                    trade.Id,
                    trade.LocationType,
                    PositionType.Settled,
                    trade.Side.ToPositionSideType(),
                    item.QuantityUnits,
                    cancellationToken);

                await _productLocationRepository.AddAsync(position, cancellationToken);
                positions.Add(position);
            }

            return positions;
        }
    }
}
