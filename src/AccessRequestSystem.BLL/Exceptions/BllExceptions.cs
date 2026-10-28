namespace AccessRequestSystem.BLL.Exceptions;

/// <summary>Сущность не найдена — контроллер должен превратить это в 404.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}

/// <summary>Нарушение бизнес-правила (например, AccessTo раньше AccessFrom) — контроллер превращает в 400.</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}
