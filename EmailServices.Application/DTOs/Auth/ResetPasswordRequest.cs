using System;
using System.Collections.Generic;
using System.Text;

namespace EmailServices.Application.DTOs.Auth;

public class ResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;
}