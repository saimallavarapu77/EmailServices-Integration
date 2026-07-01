using System;
using System.Collections.Generic;
using System.Text;

using EmailServices.Domain.Entities;

namespace EmailServices.Application.Interfaces.Repositories;

public interface IEmailOtpRepository
{
    Task AddAsync(EmailOtp otp);

    Task<EmailOtp?> GetValidOtpAsync(int userId, string otpCode);

    Task UpdateAsync(EmailOtp otp);
    Task MarkOldOtpsAsUsedAsync(int userId);
}