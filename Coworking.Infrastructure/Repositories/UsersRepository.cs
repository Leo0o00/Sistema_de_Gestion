using Coworking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Infrastructure.Repositories;

public interface IUsersRepository
{
    Task<Users?> GetByUsernameAsync(string username);
    Task<Users?> GetByEmailAsync(string email);
    Task<Users?> GetByIdAsync(int userId);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task AddUserAsync(Users user);
    Task UpdateUserAsync(Users user);
    Task SaveChangesAsync();
}

public class UserRepository: IUsersRepository
{
    private readonly CoworkingDbContext _context;

    public UserRepository(CoworkingDbContext context)
    {
        _context = context;
    }

    public async Task<Users?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<Users?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<Users?> GetByIdAsync(int userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.Username == username);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task AddUserAsync(Users user)
    {
        await _context.Users.AddAsync(user);
    }

    public Task UpdateUserAsync(Users user)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}