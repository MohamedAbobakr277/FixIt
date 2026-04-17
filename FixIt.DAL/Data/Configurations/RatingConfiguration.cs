using FixIt.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FixIt.DAL.Data.Configurations
{
    public class RatingConfiguration : IEntityTypeConfiguration<Rating>
    {
        public void Configure(EntityTypeBuilder<Rating> builder)
        {
            builder.ToTable("Ratings");

            builder.HasKey(r => r.RatingId);

            builder.Property(r => r.RatingId)
                .ValueGeneratedOnAdd();

            builder.Property(r => r.Stars)
                .IsRequired();

            // Enforce 1–5 star range at DB level
            builder.ToTable(t => t.HasCheckConstraint("CK_Rating_Stars", "[Stars] >= 1 AND [Stars] <= 5"));

            builder.Property(r => r.Comment)
                .HasMaxLength(1000)
                .IsRequired(false);

            builder.Property(r => r.SubmittedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Unique constraint on IssueId — one rating per issue
            builder.HasIndex(r => r.IssueId)
                .IsUnique();
        }
    }
}
