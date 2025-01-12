using Coworking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Infrastructure.Repositories;

    public interface IRoomRepository
    {
        Task<Rooms?> GetByIdAsync(int id);
        Task<List<Rooms>> GetAllActiveAsync();
        Task AddAsync(Rooms room);
        Task SaveChangesAsync();
    }

    public class RoomRepository : IRoomRepository
    {
        private readonly CoworkingDbContext _context;
        public RoomRepository(CoworkingDbContext context)
        {
            _context = context;
        }

        public async Task<Rooms?> GetByIdAsync(int id) => await _context.Rooms.FindAsync(id);

        public async Task<List<Rooms>> GetAllActiveAsync() =>
            await _context.Rooms.Where(r => r.IsActive).ToListAsync();

        public async Task AddAsync(Rooms room) => await _context.Rooms.AddAsync(room);

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
