using System.ComponentModel.DataAnnotations;

namespace AccessRequestSystem.UI.Models;

public record ServiceDto(Guid Id, string Name, string? Description, string? Url, string? Domain, string? RelatedDomains, bool IsBuiltIn, bool IsActive);

public class ServiceFormViewModel
{
    [Required(ErrorMessage = "Укажите название сервиса")]
    [Display(Name = "Название")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [Url(ErrorMessage = "Некорректная ссылка")]
    [Display(Name = "Ссылка")]
    public string? Url { get; set; }

    [Display(Name = "Связанные домены (через запятую)")]
    public string? RelatedDomains { get; set; }

    [Display(Name = "Активен (доступен для заявок)")]
    public bool IsActive { get; set; } = true;
}
