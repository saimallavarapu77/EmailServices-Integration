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
    }

    public Task UpdateUserAsync(User user)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }
    
}