using System;
using System.Collections.Generic;
using System.Text;

namespace EmailServices.Application.DTOs.Auth;

public class VerifyOtpRequest
{
    public string Email { get; set; } = string.Empty;
    public string OtpCode { get; set; } = string.Empty;
}