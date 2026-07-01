using System;
using System.Collections.Generic;
using System.Text;

using EmailServices.Application.Interfaces.Repositories;
using EmailServices.Domain.Entities;
using EmailServices.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmailServices.Infrastructure.Repositories;

public class EmailOtpRepository : IEmailOtpRepository
{
    private readonly AppDbContext _context;

    public EmailOtpRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(EmailOtp otp)
    {
        await _context.EmailOtps.AddAsync(otp);
    }

    public async Task<EmailOtp?> GetValidOtpAsync(int userId, string otpCode)
    {
        return await _context.EmailOtps
            .FirstOrDefaultAsync(x =>
                x.UserId == userId &&
                x.OtpCode == otpCode);
    }

    public Task UpdateAsync(EmailOtp otp)
    {
        _context.EmailOtps.Update(otp);
        return Task.CompletedTask;
    }

    public async Task MarkOldOtpsAsUsedAsync(int userId)
    {
        var oldOtps = await _context.EmailOtps
            .Where(x => x.UserId == userId && !x.IsUsed)
            .ToListAsync();

        foreach (var otp in oldOtps)
        {
            otp.IsUsed = true;
        }
    }
}
