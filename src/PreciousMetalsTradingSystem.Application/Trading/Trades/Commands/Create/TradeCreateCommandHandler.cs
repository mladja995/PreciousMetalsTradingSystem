using PreciousMetalsTradingSystem.Application.Database;
using PreciousMetalsTradingSystem.Application.Financials.Services;
using PreciousMetalsTradingSystem.Application.Heding.SpotDeferredTrades.Services;
using PreciousMetalsTradingSystem.Application.Inventory.Services;
using PreciousMetalsTradingSystem.Application.Trading.Calendar.Services;
using PreciousMetalsTradingSystem.Application.Trading.Services;
using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;

namespace PreciousMetalsTradingSystem.Application.Trading.Trades.Commands.Create
{
    public partial class TradeCreateCommandHandler 
    {
        private readonly IRepository<Trade, TradeId> _tradeRepository;
        private readonly IRepository<SpotDeferredTrade, SpotDeferredTradeId> _spotDeferredTradeRepository;
        private readonly IRepository<Product, ProductId> _productRepository;
        private readonly IRepository<LocationHedgingAccountConfiguration, LocationHedgingAccountConfigurationId> _locationHedgingAccountRepository;
        private readonly IRepository<FinancialTransaction, FinancialTransactionId> _financialTransactionRepository;
        private readonly IRepository<ProductLocationPosition, ProductLocationPositionId> _productLocationPositionRespository;
        private readonly IInventoryService _inventoryService;
        private readonly IFinancialsService _financialsService;
        private readonly IHedgingService _hedgingService;
        private readonly ICalendarService _calendarService;
        private readonly ITradingService _tradingService;
        private readonly IUnitOfWork _unitOfWork;

        public TradeCreateCommandHandler(
            IRepository<Trade, TradeId> tradeRepository,
            IRepository<SpotDeferredTrade, SpotDeferredTradeId> spotDeferredTradeRepository,
            IRepository<Product, ProductId> productRepository,
            IRepository<LocationHedgingAccountConfiguration, LocationHedgingAccountConfigurationId> locationHedgingAccountRepository,
            IRepository<FinancialTransaction, FinancialTransactionId> financialTransactionRepository,
            IRepository<ProductLocationPosition, ProductLocationPositionId> productLocationPositionRepository,
            IInventoryService inventoryService,
            IFinancialsService financialsService,
            IHedgingService hedgingService,
            ICalendarService calendarService,
            IUnitOfWork unitOfWork,
            ITradingService tradingService)
        {
            _tradeRepository = tradeRepository;
            _spotDeferredTradeRepository = spotDeferredTradeRepository;
            _productRepository = productRepository;
            _locationHedgingAccountRepository = locationHedgingAccountRepository;
            _financialTransactionRepository = financialTransactionRepository;
            _productLocationPositionRespository = productLocationPositionRepository;
            _inventoryService = inventoryService;
            _financialsService = financialsService;
            _hedgingService = hedgingService;
            _calendarService = calendarService;
            _unitOfWork = unitOfWork;
            _tradingService = tradingService;
        }
    }
}
