namespace AccessRequestSystem.UI.Services;

/// <summary>API вернул ошибку (4xx/5xx) — сообщение уже готово для показа пользователю.</summary>
public class ApiException : Exception
{
    public ApiException(string message) : base(message)
    {
    }
}
