using FixIt.Common.Enums;
using FixIt.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixIt.DAL.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.PaymentId);

        builder.Property(p => p.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(p => p.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(p => p.StripeSessionId)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(p => p.StripePaymentIntentId)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.CitizenId)
            .HasMaxLength(450)
            .IsRequired();

        // FK → Issue (no cascade to avoid multiple cascade paths)
        builder.HasOne(p => p.Issue)
            .WithMany()
            .HasForeignKey(p => p.IssueId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK → Citizen (no cascade)
        builder.HasOne(p => p.Citizen)
            .WithMany()
            .HasForeignKey(p => p.CitizenId)
            .OnDelete(DeleteBehavior.Restrict);

        // One payment per issue
        builder.HasIndex(p => p.IssueId).IsUnique();
        builder.HasIndex(p => p.StripeSessionId);
    }
}
