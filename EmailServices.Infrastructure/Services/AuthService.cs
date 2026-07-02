using EmailServices.Application.DTOs.Auth;
using EmailServices.Application.Interfaces;
using EmailServices.Application.Interfaces.Repositories;
using EmailServices.Application.Interfaces.Services;
using EmailServices.Domain.Entities;
using EmailServices.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace EmailServices.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailOtpRepository _emailOtpRepository;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    public AuthService(
     IUserRepository userRepository,
     IEmailOtpRepository emailOtpRepository,
     IEmailService emailService,
     IUnitOfWork unitOfWork, 
     IJwtService jwtService,
     IPasswordResetTokenRepository passwordResetTokenRepository)
    {
        _userRepository = userRepository;
        _emailOtpRepository = emailOtpRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordResetTokenRepository = passwordResetTokenRepository;
    }
    private static string GenerateSecureOtp()
    {
        var number = RandomNumberGenerator.GetInt32(100000, 1000000);
        return number.ToString();
    } 

    private static string GeneratePasswordResetToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
    }
    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            if (existingUser.IsEmailVerified)
                return (false, "Email already registered. Please login.");

            // Update latest details because account is not verified yet
            existingUser.Name = request.Name;
            existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            await _userRepository.UpdateUserAsync(existingUser);
            await _emailOtpRepository.MarkOldOtpsAsUsedAsync(existingUser.Id);

            var newOtpCode = GenerateSecureOtp();

            var newOtp = new EmailOtp
            {
                UserId = existingUser.Id,
                OtpCode = newOtpCode,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false
            };

            await _emailOtpRepository.AddAsync(newOtp);
            await _unitOfWork.SaveChangesAsync();

            await _emailService.SendOtpEmailAsync(
                existingUser.Email,
                existingUser.Name,
                newOtpCode);

            return (true, "Your email is not verified. A new OTP has been sent.");
        }

        var user = new User
        {
            Name = request.Name,
            Email = request.Email,

            // Temporary - we'll hash the password later
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),

            IsEmailVerified = false
        };

        await _userRepository.AddAsync(user);
        await _unitOfWork.SaveChangesAsync(); // important: generates user.Id

        var otpCode = GenerateSecureOtp();

        var otp = new EmailOtp
        {
            UserId = user.Id, // now correct Id comes here
            OtpCode = otpCode,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        };

        await _emailOtpRepository.AddAsync(otp);
        await _unitOfWork.SaveChangesAsync();

        await _emailService.SendOtpEmailAsync(user.Email, user.Name, otpCode);

        return (true, "OTP sent successfully to your email.");
    }


    public async Task<(bool Success, string Message)> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
            return (false, "User not found.");

        var otp = await _emailOtpRepository.GetValidOtpAsync(user.Id, request.OtpCode);

        if (otp == null)
            return (false, "Invalid OTP.");

        if (otp.IsUsed)
            return (false, "OTP already used.");

        if (otp.ExpiresAt < DateTime.UtcNow)
            return (false, "OTP has expired.");

        user.IsEmailVerified = true;
        otp.IsUsed = true;

        await _userRepository.UpdateUserAsync(user);
        await _emailOtpRepository.UpdateAsync(otp);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Email verified successfully.");
    }

    public async Task<(bool Success, string Message, string? Token)> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null)
            return (false, "Invalid email or password.", null);

        if (!user.IsEmailVerified)
            return (false, "Please verify your email before login.", null);

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

        if (!isPasswordValid)
            return (false, "Invalid email or password.", null);

        var token = _jwtService.GenerateToken(user.Id, user.Name, user.Email);

        return (true, "Login successful.", token);
    }

    public async Task<(bool Success, string Message)> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        var responseMessage = "If an account exists for this email, a password reset link has been sent.";

        if (user == null)
            return (true, responseMessage);

        var token = GeneratePasswordResetToken();

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false
        };

        await _passwordResetTokenRepository.AddAsync(resetToken);
        await _unitOfWork.SaveChangesAsync();

        var resetLink = $"https://localhost:3000/reset-password?token={token}";

        await _emailService.SendPasswordResetEmailAsync(
            user.Email,
            user.Name,
            resetLink);

        return (true, responseMessage);
    }

    public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var resetToken = await _passwordResetTokenRepository.GetByTokenAsync(request.Token);

        if (resetToken == null)
            return (false, "Invalid reset token.");

        if (resetToken.IsUsed)
            return (false, "Reset token already used.");

        if (resetToken.ExpiresAt < DateTime.UtcNow)
            return (false, "Reset token has expired.");

        resetToken.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        resetToken.IsUsed = true;

        await _passwordResetTokenRepository.UpdateAsync(resetToken);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Password reset successfully.");
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(
    int userId,
    ChangePasswordRequest request)
    {
        var user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
            return (false, "User not found.");

        var isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(
            request.CurrentPassword,
            user.PasswordHash);

        if (!isCurrentPasswordValid)
            return (false, "Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

        await _userRepository.UpdateUserAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Password changed successfully.");
    }
}