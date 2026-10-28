using AccessRequestSystem.UI.Models;

namespace AccessRequestSystem.UI.Services;

public interface IApiClient
{
    Task<AuthResponse> LoginAsync(string email, string password);
    Task<AuthResponse> RegisterAsync(string email, string password);

    Task<EmployeeDto?> GetMyProfileAsync();
    Task<EmployeeDto> UpsertProfileAsync(EmployeeFormViewModel dto);

    Task<List<ServiceDto>> GetActiveServicesAsync();
    Task<List<ServiceDto>> GetAllServicesAsync();
    Task<ServiceDto> CreateServiceAsync(ServiceFormViewModel dto);
    Task<ServiceDto> UpdateServiceAsync(Guid id, ServiceFormViewModel dto);

    Task<AccessRequestDto> CreateRequestAsync(CreateAccessRequestBody body);
    Task<List<AccessRequestDto>> GetMyRequestsAsync();
    Task<List<AccessRequestDto>> GetAllRequestsAsync(RequestStatus? status);
    Task<AccessRequestDto> DecideAsync(Guid id, DecisionBody body);

    Task<List<AccessLogDto>> GetAccessLogsAsync(int count = 200, string? search = null);

    Task<AuthResponse?> ExchangeSsoTicketAsync(string ticket);
}
