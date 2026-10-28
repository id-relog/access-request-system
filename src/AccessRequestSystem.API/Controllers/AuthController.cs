using AccessRequestSystem.API.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AccessRequestSystem.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser<Guid>> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _config;

    public AuthController(UserManager<IdentityUser<Guid>> userManager, ITokenService tokenService, IConfiguration config)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _config = config;
    }

    
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var existing = await _userManager.FindByEmailAsync(request.Email);
        if (existing is not null)
            return Conflict("Пользователь с таким email уже зарегистрирован.");

        var user = new IdentityUser<Guid>
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        await _userManager.AddToRoleAsync(user, Roles.Employee);

        return await BuildAuthResponse(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return Unauthorized("Неверный email или пароль.");

        return await BuildAuthResponse(user);
    }

    private async Task<ActionResult<AuthResponse>> BuildAuthResponse(IdentityUser<Guid> user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);
        var minutes = int.Parse(_config["Jwt:AccessTokenMinutes"] ?? "60");

        return Ok(new AuthResponse(user.Id, token, DateTime.UtcNow.AddMinutes(minutes), roles.ToList(), user.Email));
    }
}
