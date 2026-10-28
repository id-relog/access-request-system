using AccessRequestSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AccessRequestSystem.DAL.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");

        builder.HasKey(e => e.Id);

        // один пользователь Identity — одна анкета
        builder.HasIndex(e => e.UserId).IsUnique();

        builder.Property(e => e.LastName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.MiddleName).HasMaxLength(100);

        builder.Property(e => e.Position).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Department).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Organization).HasMaxLength(200).IsRequired();

        builder.Property(e => e.PersonnelNumber).HasMaxLength(50);
        builder.Property(e => e.Phone).HasMaxLength(20);
        builder.Property(e => e.Email).HasMaxLength(150).IsRequired();

        builder.HasMany(e => e.AccessRequests)
            .WithOne(r => r.Employee)
            .HasForeignKey(r => r.EmployeeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
