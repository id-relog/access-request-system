using AccessRequestSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessRequestSystem.DAL.Configurations;

public class AccessLogConfiguration : IEntityTypeConfiguration<AccessLog>
{
    public void Configure(EntityTypeBuilder<AccessLog> builder)
    {
        builder.ToTable("AccessLogs");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Domain).HasMaxLength(255).IsRequired();
        builder.Property(l => l.CheckedAt).IsRequired();

        // UserId ссылается на AspNetUsers.Id — без EF-навигации, по той же причине,
        // что и AccessRequest.ReviewedByUserId (Domain не зависит от типов Identity).
        builder.Property(l => l.UserId).IsRequired();

        builder.HasIndex(l => l.CheckedAt);
        builder.HasIndex(l => new { l.UserId, l.CheckedAt });
    }
}
