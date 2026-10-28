using AccessRequestSystem.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AccessRequestSystem.DAL;

/// <summary>
/// Основной контекст БД. Наследуется от IdentityDbContext, чтобы таблицы
/// Identity (AspNetUsers, AspNetRoles и т.д.) и таблицы предметной области
/// жили в одной базе PostgreSQL и их можно было связывать внешними ключами.
/// </summary>
public class AppDbContext : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<AccessRequest> AccessRequests => Set<AccessRequest>();
    public DbSet<RequestHistory> RequestHistories => Set<RequestHistory>();
    public DbSet<AccessLog> AccessLogs => Set<AccessLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // обязательно вызываем base — иначе Identity не настроит свои таблицы
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
