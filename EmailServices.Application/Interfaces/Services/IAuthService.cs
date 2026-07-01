using System;
using System.Collections.Generic;
using System.Text;

using EmailServices.Application.DTOs.Auth;

namespace EmailServices.Application.Interfaces.Services;

public interface IAuthService
{
    Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpRequest request);
    Task<(bool Success, string Message, string? Token)> LoginAsync(LoginRequest request);
}
