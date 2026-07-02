using System;
using System.Collections.Generic;
using System.Text;

namespace EmailServices.Application.Interfaces.Services;

public interface IEmailService
{
    Task SendOtpEmailAsync(string toEmail, string userName, string otpCode);
    Task SendPasswordResetEmailAsync(string toEmail, string userName, string resetLink);
}
