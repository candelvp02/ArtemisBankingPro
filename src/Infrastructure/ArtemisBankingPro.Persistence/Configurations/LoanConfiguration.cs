using ArtemisBankingPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBankingPro.Persistence.Configurations;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.LoanNumber)
            .IsRequired()
            .HasMaxLength(9);

        builder.HasIndex(l => l.LoanNumber).IsUnique();

        builder.Property(l => l.Amount).HasColumnType("decimal(18,2)");
        builder.Property(l => l.AnnualInterestRate).HasColumnType("decimal(5,2)");
        builder.Property(l => l.MonthlyPayment).HasColumnType("decimal(18,2)");
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasOne(l => l.ApplicationUser)
            .WithMany(u => u.Loans)
            .HasForeignKey(l => l.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.Installments)
            .WithOne(i => i.Loan)
            .HasForeignKey(i => i.LoanId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}