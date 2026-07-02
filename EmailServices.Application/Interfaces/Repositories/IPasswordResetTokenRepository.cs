using System;
using System.Collections.Generic;
using System.Text;

using EmailServices.Domain.Entities;

namespace EmailServices.Application.Interfaces.Repositories;

public interface IPasswordResetTokenRepository
{
    Task AddAsync(PasswordResetToken token);
    Task<PasswordResetToken?> GetByTokenAsync(string token);

    Task UpdateAsync(PasswordResetToken token);
}