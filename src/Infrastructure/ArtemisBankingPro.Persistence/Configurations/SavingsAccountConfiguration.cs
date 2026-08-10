using ArtemisBankingPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBankingPro.Persistence.Configurations;

public class SavingsAccountConfiguration : IEntityTypeConfiguration<SavingsAccount>
{
    public void Configure(EntityTypeBuilder<SavingsAccount> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AccountNumber)
            .IsRequired()
            .HasMaxLength(9);

        builder.HasIndex(a => a.AccountNumber).IsUnique();

        builder.Property(a => a.Balance)
            .HasColumnType("decimal(18,2)");

        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(a => a.ApplicationUser)
            .WithMany(u => u.SavingsAccounts)
            .HasForeignKey(a => a.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(a => a.Transactions)
            .WithOne(t => t.SavingsAccount)
            .HasForeignKey(t => t.SavingsAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}