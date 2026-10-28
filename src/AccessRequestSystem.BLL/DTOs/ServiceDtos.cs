namespace AccessRequestSystem.BLL.DTOs;

public record ServiceDto(
    Guid Id,
    string Name,
    string? Description,
    string? Url,
    string? Domain,
    string? RelatedDomains,
    bool IsBuiltIn,
    bool IsActive);

public record UpsertServiceDto(
    string Name,
    string? Description,
    string? Url,
    string? RelatedDomains,
    bool IsActive);
