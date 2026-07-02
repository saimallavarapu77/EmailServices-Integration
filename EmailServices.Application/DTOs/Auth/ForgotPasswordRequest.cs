using System;
using System.Collections.Generic;
using System.Text;

namespace EmailServices.Application.DTOs.Auth;

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}
