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

    public AuthService(
     IUserRepository userRepository,
     IEmailOtpRepository emailOtpRepository,
     IEmailService emailService,
     IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _emailOtpRepository = emailOtpRepository;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }
    private static string GenerateSecureOtp()
    {
        var number = RandomNumberGenerator.GetInt32(100000, 1000000);
        return number.ToString();
    }
    public async Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);

        if (existingUser != null)
        {
            return (false, "Email already exists.");
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
}