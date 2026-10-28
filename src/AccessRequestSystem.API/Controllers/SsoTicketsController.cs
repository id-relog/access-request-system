using System.Security.Claims;
using AccessRequestSystem.API.Auth;
using AccessRequestSystem.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessRequestSystem.API.Controllers;

[ApiController]
[Route("api/auth/sso-tickets")]
public class SsoTicketsController : ControllerBase
{
    private readonly ISsoTicketStore _tickets;

    public SsoTicketsController(ISsoTicketStore tickets)
    {
        _tickets = tickets;
    }

    /// <summary>Прокси после Basic-логина: выпустить одноразовый билет для входа в UI.</summary>
    [HttpPost]
    [Authorize]
    public ActionResult<CreateSsoTicketResponse> Create()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authHeader) ||
            !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return BadRequest("Нужен Bearer JWT.");

        var token = authHeader["Bearer ".Length..].Trim();
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? string.Empty;
        var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        // JWT уже проверен middleware — expires берём с запасом из claim, иначе 8ч по умолчанию
        var expClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Exp)?.Value;
        var expiresAt = DateTime.UtcNow.AddHours(8);
        if (long.TryParse(expClaim, out var expUnix))
            expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;

        var ticket = _tickets.Create(new SsoTicketPayload(userId, email, token, expiresAt, roles));
        return Ok(new CreateSsoTicketResponse(ticket, DateTime.UtcNow.AddMinutes(2)));
    }

    /// <summary>UI обменивает билет на AuthResponse и ставит cookie.</summary>
    [HttpPost("exchange")]
    [AllowAnonymous]
    public ActionResult<AuthResponse> Exchange([FromBody] ExchangeSsoTicketRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Ticket))
            return BadRequest("Не указан ticket.");

        var payload = _tickets.Consume(request.Ticket);
        if (payload is null)
            return Unauthorized("Билет недействителен или уже использован.");

        return Ok(new AuthResponse(
            payload.UserId,
            payload.Token,
            payload.ExpiresAt,
            payload.Roles.ToList(),
            payload.Email));
    }
}

public record CreateSsoTicketResponse(string Ticket, DateTime TicketExpiresAt);
public record ExchangeSsoTicketRequest(string Ticket);
