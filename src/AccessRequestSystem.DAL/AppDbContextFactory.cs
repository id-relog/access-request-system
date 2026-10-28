using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AccessRequestSystem.DAL;

/// <summary>
/// Нужен только инструменту dotnet-ef, чтобы уметь создавать миграции
/// без запуска всего API-проекта: dotnet ef migrations add InitialCreate
/// -p src/AccessRequestSystem.DAL -s src/AccessRequestSystem.DAL
///
/// Строка подключения берётся из переменной окружения ARS_CONNECTION_STRING,
/// либо используется локальный дефолт для разработки.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ARS_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=access_request_system;Username=postgres;Password=1111";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
