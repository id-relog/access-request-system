namespace AccessRequestSystem.UI.Services;

/// <summary>API вернул 401 — значит JWT в cookie истёк или невалиден. UI должен разлогинить и отправить на логин.</summary>
public class ApiUnauthorizedException : ApiException
{
    public ApiUnauthorizedException(string message) : base(message)
    {
    }
}
