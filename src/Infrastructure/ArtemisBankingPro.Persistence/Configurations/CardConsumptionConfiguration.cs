using ArtemisBankingPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBankingPro.Persistence.Configurations;

public class CardConsumptionConfiguration : IEntityTypeConfiguration<CardConsumption>
{
    public void Configure(EntityTypeBuilder<CardConsumption> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Amount).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.RejectionReason).HasMaxLength(250);

        builder.HasOne(c => c.Commerce)
            .WithMany(m => m.Consumptions)
            .HasForeignKey(c => c.CommerceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}