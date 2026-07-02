using System;
using System.Collections.Generic;
using System.Text;

namespace EmailServices.Application.DTOs.Auth;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
}
