using AccessRequestSystem.BLL.DTOs;
using AccessRequestSystem.BLL.Exceptions;
using AccessRequestSystem.DAL.Repositories;
using AccessRequestSystem.Domain.Entities;

namespace AccessRequestSystem.BLL.Services;

public class ServiceCatalogService : IServiceCatalogService
{
    private readonly IServiceRepository _services;

    public ServiceCatalogService(IServiceRepository services)
    {
        _services = services;
    }

    public async Task<List<ServiceDto>> GetActiveAsync(CancellationToken ct = default)
        => (await _services.GetActiveAsync(ct)).Select(ToDto).ToList();

    public async Task<List<ServiceDto>> GetAllAsync(CancellationToken ct = default)
        => (await _services.GetAllAsync(ct)).Select(ToDto).ToList();

    public async Task<ServiceDto> CreateAsync(UpsertServiceDto dto, CancellationToken ct = default)
    {
        var service = new Service
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Url = dto.Url,
            Domain = ExtractDomain(dto.Url),
            RelatedDomains = NormalizeRelatedDomains(dto.RelatedDomains),
            IsActive = dto.IsActive,
            IsBuiltIn = false,
            CreatedAt = DateTime.UtcNow
        };

        await _services.AddAsync(service, ct);
        await _services.SaveChangesAsync(ct);
        return ToDto(service);
    }

    public async Task<ServiceDto> UpdateAsync(Guid id, UpsertServiceDto dto, CancellationToken ct = default)
    {
        var service = await _services.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Сервис {id} не найден.");

        service.Name = dto.Name;
        service.Description = dto.Description;
        service.Url = dto.Url;
        service.Domain = ExtractDomain(dto.Url);
        service.RelatedDomains = NormalizeRelatedDomains(dto.RelatedDomains);
        service.IsActive = dto.IsActive;

        _services.Update(service);
        await _services.SaveChangesAsync(ct);
        return ToDto(service);
    }

    /// <summary>Достаёт хост из ссылки (например, "https://music.yandex.by/artist" → "music.yandex.by") — по этому полю прокси-агент сверяет запросы.</summary>
    internal static string? ExtractDomain(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var candidate = url.Contains("://") ? url : $"https://{url}";
        return Uri.TryCreate(candidate, UriKind.Absolute, out var uri) ? uri.Host.ToLowerInvariant() : null;
    }

    /// <summary>
    /// Принимает список доменов через запятую/перенос строки/пробел от пользователя, чистит и приводит
    /// к единому формату "a.com,b.com,c.com" — так их проще парсить обратно при проверке доступа.
    /// </summary>
    internal static string? NormalizeRelatedDomains(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        var domains = raw
            .Split(new[] { ',', '\n', '\r', ' ', ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(d => ExtractDomain(d) ?? d.Trim().ToLowerInvariant())
            .Where(d => !string.IsNullOrWhiteSpace(d))
            .Distinct()
            .ToList();

        return domains.Count == 0 ? null : string.Join(",", domains);
    }

    private static ServiceDto ToDto(Service s) => new(s.Id, s.Name, s.Description, s.Url, s.Domain, s.RelatedDomains, s.IsBuiltIn, s.IsActive);
}
