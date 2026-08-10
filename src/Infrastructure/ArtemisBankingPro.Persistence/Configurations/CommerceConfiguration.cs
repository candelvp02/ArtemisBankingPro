using ArtemisBankingPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArtemisBankingPro.Persistence.Configurations;

public class CommerceConfiguration : IEntityTypeConfiguration<Commerce>
{
    public void Configure(EntityTypeBuilder<Commerce> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Rnc).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(150);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(c => c.Rnc).IsUnique();
        builder.HasIndex(c => c.Email).IsUnique();

        builder.HasOne(c => c.ApplicationUser)
            .WithMany()
            .HasForeignKey(c => c.ApplicationUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.SavingsAccount)
            .WithMany()
            .HasForeignKey(c => c.SavingsAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}