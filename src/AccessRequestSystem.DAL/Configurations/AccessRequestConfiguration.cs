using AccessRequestSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessRequestSystem.DAL.Configurations;

public class AccessRequestConfiguration : IEntityTypeConfiguration<AccessRequest>
{
    public void Configure(EntityTypeBuilder<AccessRequest> builder)
    {
        builder.ToTable("AccessRequests", t =>
        {
            // доступ не может заканчиваться раньше, чем начинается
            t.HasCheckConstraint("CK_AccessRequests_AccessPeriod", "\"AccessTo\" > \"AccessFrom\"");

            // должно быть заполнено либо ServiceId, либо CustomServiceName — но не оба сразу и не оба пустые
            t.HasCheckConstraint("CK_AccessRequests_ServiceOrCustom",
                "(\"ServiceId\" IS NOT NULL AND \"CustomServiceName\" IS NULL) " +
                "OR (\"ServiceId\" IS NULL AND \"CustomServiceName\" IS NOT NULL)");
        });

        builder.HasKey(r => r.Id);

        builder.Property(r => r.CustomServiceName).HasMaxLength(200);
        builder.Property(r => r.CustomServiceUrl).HasMaxLength(500);
        builder.Property(r => r.CustomServiceDomain).HasMaxLength(255);
        builder.Property(r => r.Justification).HasColumnType("text");
        builder.Property(r => r.ReviewComment).HasColumnType("text");

        builder.Property(r => r.AccessFrom).HasColumnType("date").IsRequired();
        builder.Property(r => r.AccessTo).HasColumnType("date").IsRequired();

        builder.Property(r => r.DurationPreset)
            .HasConversion<int>()
            .HasColumnType("smallint")
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<int>()
            .HasColumnType("smallint")
            .IsRequired();

        builder.Property(r => r.RequestedAt).IsRequired();

        // ReviewedByUserId ссылается на AspNetUsers.Id, но Domain-слой сознательно
        // не зависит от типов Identity — поэтому здесь просто колонка без
        // навигационного свойства, а не полноценный FK через EF-навигацию.
        builder.Property(r => r.ReviewedByUserId);

        builder.HasOne(r => r.Service)
            .WithMany(s => s.AccessRequests)
            .HasForeignKey(r => r.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.History)
            .WithOne(h => h.AccessRequest)
            .HasForeignKey(h => h.AccessRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => new { r.EmployeeId, r.Status });
        builder.HasIndex(r => new { r.EmployeeId, r.CustomServiceDomain });
    }
}
