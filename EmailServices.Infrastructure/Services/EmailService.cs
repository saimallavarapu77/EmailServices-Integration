using System;
using System.Collections.Generic;
using System.Text;

using EmailServices.Application.Interfaces.Services;
using EmailServices.Infrastructure.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EmailServices.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _smtpSettings;

    public EmailService(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }

    public async Task SendOtpEmailAsync(string toEmail, string userName, string otpCode)
    {
        var email = new MimeMessage();

        email.From.Add(new MailboxAddress(
            _smtpSettings.SenderName,
            _smtpSettings.SenderEmail));

        email.To.Add(MailboxAddress.Parse(toEmail));

        email.Subject = "Verify Your Email";

        email.Body = new TextPart("html")
        {
            Text = $@"
<!DOCTYPE html>
<html>
<head>
<style>
body {{
    font-family: Arial, Helvetica, sans-serif;
    background-color:#f5f5f5;
    margin:0;
    padding:20px;
}}

.container {{
    max-width:600px;
    margin:auto;
    background:#ffffff;
    border-radius:10px;
    overflow:hidden;
    box-shadow:0 0 10px rgba(0,0,0,.1);
}}

.header {{
    background:#2563eb;
    color:white;
    text-align:center;
    padding:20px;
    font-size:26px;
    font-weight:bold;
}}

.content {{
    padding:30px;
    color:#333;
    line-height:1.8;
}}

.otp {{
    text-align:center;
    font-size:36px;
    font-weight:bold;
    letter-spacing:8px;
    color:#2563eb;
    margin:30px 0;
    padding:15px;
    border:2px dashed #2563eb;
    border-radius:8px;
    background:#f8fbff;
}}

.footer {{
    text-align:center;
    font-size:13px;
    color:#777;
    padding:20px;
    background:#fafafa;
}}
</style>
</head>

<body>

<div class='container'>

<div class='header'>
Verify Email
</div>

<div class='content'>

<h2>Hello {userName},</h2>

<p>
Thank you for registering with us.
Use the verification code below to verify your email address.
</p>

<div class='otp'>
{otpCode}
</div>

<p>
This verification code will expire in <b>5 minutes</b>.
</p>

<p>
If you didn't request this verification, you can safely ignore this email.
</p>

</div>

<div class='footer'>
© 2026 Verify Email Team
</div>

</div>

</body>
</html>"
        };

        using var smtp = new SmtpClient();

        await smtp.ConnectAsync(
            _smtpSettings.Host,
            _smtpSettings.Port,
            SecureSocketOptions.StartTls);

        await smtp.AuthenticateAsync(
            _smtpSettings.SenderEmail,
            _smtpSettings.Password);

        await smtp.SendAsync(email);

        await smtp.DisconnectAsync(true);
    }
}
