using ArtemisBankingPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBankingPro.Persistence.Configurations;

public class CreditCardConfiguration : IEntityTypeConfiguration<CreditCard>
{
    public void Configure(EntityTypeBuilder<CreditCard> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.CardNumber)
            .IsRequired()
            .HasMaxLength(16);

        builder.HasIndex(c => c.CardNumber).IsUnique();

        builder.Property(c => c.CvcHash).IsRequired();
        builder.Property(c => c.CreditLimit).HasColumnType("decimal(18,2)");
        builder.Property(c => c.CurrentDebt).HasColumnType("decimal(18,2)");
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);

        builder.Ignore(c => c.AvailableCredit);
        builder.Ignore(c => c.MaskedNumber);

        builder.HasOne(c => c.ApplicationUser)
            .WithMany(u => u.CreditCards)
            .HasForeignKey(c => c.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Consumptions)
            .WithOne(co => co.CreditCard)
            .HasForeignKey(co => co.CreditCardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}