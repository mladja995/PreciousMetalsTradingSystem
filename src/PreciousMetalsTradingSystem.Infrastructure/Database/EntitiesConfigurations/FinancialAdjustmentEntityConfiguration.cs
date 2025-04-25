using PreciousMetalsTradingSystem.Domain.Entities;
using PreciousMetalsTradingSystem.Domain.Primitives.EntityIds;
using PreciousMetalsTradingSystem.Domain.Primitives.ValueObjects;
using PreciousMetalsTradingSystem.Infrastructure.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PreciousMetalsTradingSystem.Infrastructure.Database.EntitiesConfigurations
{
    public class FinancialAdjustmentEntityConfiguration : IEntityTypeConfiguration<FinancialAdjustment>
    {
        public void Configure(EntityTypeBuilder<FinancialAdjustment> builder)
        {
            builder.ToTable(TableNames.FinancialAdjustments);

            // Conversion for FinancialAdjustmentId
            builder.Property(fa => fa.Id)
                .HasConversion(
                    id => id.Value,  // Convert FinancialAdjustmentId to Guid
                    value => new FinancialAdjustmentId(value))  // Convert Guid back to FinancialAdjustmentId
                .IsRequired();

            // Explicitly map the value object using the shadow property pattern
            builder.Property(fa => fa.Amount)
                .HasConversion(
                    amount => amount.Value,
                    value => new Money(value))
                .HasPrecision(18, 2)
                .IsRequired();
        }
    }
}
