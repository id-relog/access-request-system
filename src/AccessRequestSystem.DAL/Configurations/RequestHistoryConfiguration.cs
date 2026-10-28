using AccessRequestSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessRequestSystem.DAL.Configurations;

public class RequestHistoryConfiguration : IEntityTypeConfiguration<RequestHistory>
{
    public void Configure(EntityTypeBuilder<RequestHistory> builder)
    {
        builder.ToTable("RequestHistories");

        builder.HasKey(h => h.Id);

        builder.Property(h => h.OldStatus).HasConversion<int>().HasColumnType("smallint").IsRequired();
        builder.Property(h => h.NewStatus).HasConversion<int>().HasColumnType("smallint").IsRequired();
        builder.Property(h => h.ChangedByUserId).IsRequired();
        builder.Property(h => h.ChangedAt).IsRequired();
        builder.Property(h => h.Comment).HasColumnType("text");
    }
}
