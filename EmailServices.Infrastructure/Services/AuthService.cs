using EmailServices.Application.DTOs.Auth;
using EmailServices.Application.Interfaces.Repositories;
using EmailServices.Application.Interfaces.Services;
using EmailServices.Domain.Entities;
using EmailServices.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace EmailServices.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
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

        return (true, "User registered successfully.");
    }
}