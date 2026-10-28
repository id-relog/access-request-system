using AccessRequestSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessRequestSystem.DAL.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).HasMaxLength(150).IsRequired();
        builder.Property(s => s.Description).HasColumnType("text");
        builder.Property(s => s.Url).HasMaxLength(500);
        builder.Property(s => s.Domain).HasMaxLength(255);
        builder.Property(s => s.RelatedDomains).HasMaxLength(1000);

        builder.Property(s => s.IsBuiltIn).HasDefaultValue(false);
        builder.Property(s => s.IsActive).HasDefaultValue(true);

        builder.HasIndex(s => s.Name);
        builder.HasIndex(s => s.Domain);
    }
}
