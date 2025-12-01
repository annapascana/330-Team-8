using BCrypt.Net;
using CrimsonBookStore.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CrimsonBookStore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] DTOs.RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        if (result == null)
        {
            return BadRequest(new { message = "Email or username already exists" });
        }
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] DTOs.LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result == null)
        {
            return Unauthorized(new { message = "Invalid email or password" });
        }
        return Ok(result);
    }

    [HttpPost("generate-hash")]
    public IActionResult GenerateHash([FromBody] DTOs.HashRequest request)
    {
        // Helper endpoint to generate BCrypt hash for fixing existing users
        // Only use in development!
        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password, 11);
        return Ok(new { password = request.Password, hash = hash });
    }
}

