using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PreciousMetalsTradingSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinancialAdjustments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdjustmentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SideType = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialAdjustments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HedgingAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HedgingAccounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    WeightInOz = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    MetalType = table.Column<int>(type: "int", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TradeQuotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DealerQuoteId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IssuedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Side = table.Column<int>(type: "int", nullable: false),
                    LocationType = table.Column<int>(type: "int", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeQuotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HedgingItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HedgingAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HedgingItemDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    SideType = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HedgingItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HedgingItems_HedgingAccounts_HedgingAccountId",
                        column: x => x.HedgingAccountId,
                        principalTable: "HedgingAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocationHedgingAccountConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    HedgingAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationHedgingAccountConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocationHedgingAccountConfigurations_HedgingAccounts_HedgingAccountId",
                        column: x => x.HedgingAccountId,
                        principalTable: "HedgingAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpotDeferredTrades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HedgingAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TradeConfirmationReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Side = table.Column<int>(type: "int", nullable: false),
                    SpotDeferredTradeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsManual = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpotDeferredTrades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpotDeferredTrades_HedgingAccounts_HedgingAccountId",
                        column: x => x.HedgingAccountId,
                        principalTable: "HedgingAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductLocationConfigurations",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationType = table.Column<int>(type: "int", nullable: false),
                    PremiumUnitType = table.Column<int>(type: "int", nullable: false),
                    BuyPremium = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SellPremium = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsAvailableForBuy = table.Column<bool>(type: "bit", nullable: false),
                    IsAvailableForSell = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductLocationConfigurations", x => new { x.ProductId, x.LocationType });
                    table.ForeignKey(
                        name: "FK_ProductLocationConfigurations_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TradeQuoteItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TradeQuoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityUnits = table.Column<int>(type: "int", nullable: false),
                    SpotPricePerOz = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PremiumPricePerOz = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EffectivePricePerOz = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeQuoteItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradeQuoteItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TradeQuoteItems_TradeQuotes_TradeQuoteId",
                        column: x => x.TradeQuoteId,
                        principalTable: "TradeQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SpotDeferredTradeItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SpotDeferredTradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Metal = table.Column<int>(type: "int", nullable: false),
                    PricePerOz = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    QuantityOz = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpotDeferredTradeItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpotDeferredTradeItems_SpotDeferredTrades_SpotDeferredTradeId",
                        column: x => x.SpotDeferredTradeId,
                        principalTable: "SpotDeferredTrades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TradeQuoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SpotDeferredTradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OffsetTradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TradeNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Side = table.Column<int>(type: "int", nullable: false),
                    LocationType = table.Column<int>(type: "int", nullable: false),
                    TradeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsPositionSettled = table.Column<bool>(type: "bit", nullable: false),
                    PositionSettledOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsFinancialSettled = table.Column<bool>(type: "bit", nullable: false),
                    FinancialSettledOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FinancialSettleOn = table.Column<DateOnly>(type: "date", nullable: false),
                    ConfirmedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastUpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trades_SpotDeferredTrades_SpotDeferredTradeId",
                        column: x => x.SpotDeferredTradeId,
                        principalTable: "SpotDeferredTrades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trades_TradeQuotes_TradeQuoteId",
                        column: x => x.TradeQuoteId,
                        principalTable: "TradeQuotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Trades_Trades_OffsetTradeId",
                        column: x => x.OffsetTradeId,
                        principalTable: "Trades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FinancialAdjustmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BalanceType = table.Column<int>(type: "int", nullable: false),
                    SideType = table.Column<int>(type: "int", nullable: false),
                    ActivityType = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_FinancialAdjustments_FinancialAdjustmentId",
                        column: x => x.FinancialAdjustmentId,
                        principalTable: "FinancialAdjustments",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Trades_TradeId",
                        column: x => x.TradeId,
                        principalTable: "Trades",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ProductLocationPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LocationType = table.Column<int>(type: "int", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SideType = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    QuantityUnits = table.Column<int>(type: "int", nullable: false),
                    PositionUnits = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductLocationPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductLocationPositions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductLocationPositions_Trades_TradeId",
                        column: x => x.TradeId,
                        principalTable: "Trades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TradeItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TradeQuoteItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityUnits = table.Column<int>(type: "int", nullable: false),
                    SpotPricePerOz = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TradePricePerOz = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PremiumPerOz = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EffectivePricePerOz = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalRevenue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalEffectivePrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradeItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TradeItems_TradeQuoteItems_TradeQuoteItemId",
                        column: x => x.TradeQuoteItemId,
                        principalTable: "TradeQuoteItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TradeItems_Trades_TradeId",
                        column: x => x.TradeId,
                        principalTable: "Trades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_FinancialAdjustmentId",
                table: "FinancialTransactions",
                column: "FinancialAdjustmentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_TradeId",
                table: "FinancialTransactions",
                column: "TradeId");

            migrationBuilder.CreateIndex(
                name: "IX_HedgingItems_HedgingAccountId",
                table: "HedgingItems",
                column: "HedgingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LocationHedgingAccountConfigurations_HedgingAccountId",
                table: "LocationHedgingAccountConfigurations",
                column: "HedgingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductLocationPositions_ProductId",
                table: "ProductLocationPositions",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductLocationPositions_TradeId",
                table: "ProductLocationPositions",
                column: "TradeId");

            migrationBuilder.CreateIndex(
                name: "IX_SpotDeferredTradeItems_SpotDeferredTradeId",
                table: "SpotDeferredTradeItems",
                column: "SpotDeferredTradeId");

            migrationBuilder.CreateIndex(
                name: "IX_SpotDeferredTrades_HedgingAccountId",
                table: "SpotDeferredTrades",
                column: "HedgingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeItems_ProductId",
                table: "TradeItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeItems_TradeId",
                table: "TradeItems",
                column: "TradeId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeItems_TradeQuoteItemId",
                table: "TradeItems",
                column: "TradeQuoteItemId",
                unique: true,
                filter: "[TradeQuoteItemId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TradeQuoteItems_ProductId",
                table: "TradeQuoteItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeQuoteItems_TradeQuoteId",
                table: "TradeQuoteItems",
                column: "TradeQuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_OffsetTradeId",
                table: "Trades",
                column: "OffsetTradeId",
                unique: true,
                filter: "[OffsetTradeId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_SpotDeferredTradeId",
                table: "Trades",
                column: "SpotDeferredTradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_TradeQuoteId",
                table: "Trades",
                column: "TradeQuoteId",
                unique: true,
                filter: "[TradeQuoteId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinancialTransactions");

            migrationBuilder.DropTable(
                name: "HedgingItems");

            migrationBuilder.DropTable(
                name: "LocationHedgingAccountConfigurations");

            migrationBuilder.DropTable(
                name: "ProductLocationConfigurations");

            migrationBuilder.DropTable(
                name: "ProductLocationPositions");

            migrationBuilder.DropTable(
                name: "SpotDeferredTradeItems");

            migrationBuilder.DropTable(
                name: "TradeItems");

            migrationBuilder.DropTable(
                name: "FinancialAdjustments");

            migrationBuilder.DropTable(
                name: "TradeQuoteItems");

            migrationBuilder.DropTable(
                name: "Trades");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "SpotDeferredTrades");

            migrationBuilder.DropTable(
                name: "TradeQuotes");

            migrationBuilder.DropTable(
                name: "HedgingAccounts");
        }
    }
}
