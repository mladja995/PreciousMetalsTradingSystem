using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using PreciousMetalsTradingSystem.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PreciousMetalsTradingSystem.Infrastructure.Database.EntitiesConfigurations
{
    public class FinancialTransactionEntityConfiguration : IEntityTypeConfiguration<FinancialTransaction>
    {
        public void Configure(EntityTypeBuilder<FinancialTransaction> builder)
        {
            builder.ToTable(TableNames.FinancialTransactions);

            builder.Property(ft => ft.Id)
               .HasConversion(
                   fId => fId.Value,
                   value => new FinancialTransactionId(value)) 
               .IsRequired();

            // Conversion for nullable TradeId
            builder.Property(ft => ft.TradeId)
                .HasConversion(
                    id => id != null ? id.Value: (Guid?)null,  // Convert TradeId to nullable Guid
                    value => value.HasValue ? new TradeId(value.Value) : null)  // Convert nullable Guid back to TradeId
                .IsRequired(false);

            // Conversion for nullable FinancialAdjustmentId
            builder.Property(ft => ft.FinancialAdjustmentId)
                .HasConversion(
                    id => id != null ? id.Value : (Guid?)null,
                    value => value.HasValue ? new FinancialAdjustmentId(value.Value) : null)
                .IsRequired(false);

            builder.Property(ft => ft.Amount)
                .HasConversion(
                    amount => amount.Value,
                    value => new Money(value))
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(ft => ft.Balance)
                .HasConversion(
                    balance => balance.Value,
                    value => new FinancialBalance(value))
                .HasPrecision(18, 2)
                .IsRequired();
        }
    }
}
