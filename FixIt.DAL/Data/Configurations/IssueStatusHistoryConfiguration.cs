using FixIt.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixIt.DAL.Data.Configurations;

public class IssueStatusHistoryConfiguration : IEntityTypeConfiguration<IssueStatusHistory>
{
    public void Configure(EntityTypeBuilder<IssueStatusHistory> builder)
    {
        builder.ToTable("IssueStatusHistories");

        builder.HasKey(h => h.IssueStatusHistoryId);

        builder.Property(h => h.IssueStatusHistoryId)
            .ValueGeneratedOnAdd();

        builder.Property(h => h.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(h => h.ChangedAt)
            .IsRequired();

        builder.Property(h => h.Note)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.HasOne(h => h.Issue)
            .WithMany(i => i.StatusHistory)
            .HasForeignKey(h => h.IssueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(h => h.ChangedBy)
            .WithMany()
            .HasForeignKey(h => h.ChangedById)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(h => new { h.IssueId, h.ChangedAt });
    }
}
