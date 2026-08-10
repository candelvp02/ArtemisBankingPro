using ArtemisBankingPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBankingPro.Persistence.Configurations;

public class BeneficiaryConfiguration : IEntityTypeConfiguration<Beneficiary>
{
    public void Configure(EntityTypeBuilder<Beneficiary> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Alias).HasMaxLength(100);

        builder.HasOne(b => b.ApplicationUser)
            .WithMany(u => u.Beneficiaries)
            .HasForeignKey(b => b.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.SavingsAccount)
            .WithMany()
            .HasForeignKey(b => b.SavingsAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}