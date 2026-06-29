using System;
using System.Collections.Generic;
using System.Text;

using EmailServices.Application.Interfaces.Repositories;
using EmailServices.Domain.Entities;
using EmailServices.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EmailServices.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }
}