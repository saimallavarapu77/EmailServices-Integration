using Microsoft.AspNetCore.Mvc;

using EmailServices.Application.DTOs.Auth;
using EmailServices.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmailServices_API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }
}
