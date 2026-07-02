using System;
using System.Collections.Generic;
using System.Text;

using EmailServices.Domain.Entities;

namespace EmailServices.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int userId);
    Task AddAsync(User user);
    Task UpdateUserAsync(User user);

}