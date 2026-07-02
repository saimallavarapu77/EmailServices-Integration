using EmailServices.Application.Common;
using EmailServices.Application.DTOs.Auth;
using EmailServices.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        {
            return BadRequest(
                ApiResponse<object>.FailureResponse(result.Message));
        }

        return Ok(
            ApiResponse<object>.SuccessResponse(
                null,
                result.Message));
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (!result.Success)
        {
            return BadRequest(
                ApiResponse<object>.FailureResponse(result.Message));
        }

        var data = new LoginResponse
        {
            Token = result.Token!
        };

        return Ok(
            ApiResponse<LoginResponse>.SuccessResponse(
                data,
                result.Message));
    }

    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp(VerifyOtpRequest request)
    {
        var result = await _authService.VerifyOtpAsync(request);

        if (!result.Success)
        {
            return BadRequest(
                ApiResponse<object>.FailureResponse(result.Message));
        }

        return Ok(
            ApiResponse<object>.SuccessResponse(
                null,
                result.Message));
    }

    [Authorize]
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var name = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;

        return Ok(new
        {
            UserId = userId,
            Name = name,
            Email = email
        });
    }
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);

        if (!result.Success)
        {
            return BadRequest(
                ApiResponse<object>.FailureResponse(result.Message));
        }

        return Ok(
            ApiResponse<object>.SuccessResponse(
                null,
                result.Message));
    }
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);

        if (!result.Success)
        {
            return BadRequest(
                ApiResponse<object>.FailureResponse(result.Message));
        }

        return Ok(
            ApiResponse<object>.SuccessResponse(
                null,
                result.Message));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(
                ApiResponse<object>.FailureResponse("Invalid token."));
        }

        var result = await _authService.ChangePasswordAsync(userId, request);

        if (!result.Success)
        {
            return BadRequest(
                ApiResponse<object>.FailureResponse(result.Message));
        }

        return Ok(
            ApiResponse<object>.SuccessResponse(
                null,
                result.Message));
    }
}
