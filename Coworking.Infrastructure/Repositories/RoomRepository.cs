using Coworking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Coworking.Infrastructure.Repositories;

    public interface IRoomRepository
    {
        Task<List<Rooms>> GetAllAsync();
        Task<List<Rooms>> GetAllActiveAsync();
        Task<Rooms?> GetByIdAsync(int id);
        Task<Rooms?> GetByNameAsync(string name);
        Task<bool> ExistingRoomName(string name);
        Task<int> AddAsync(Rooms room);
        Task<bool> UpdateAsync(Rooms room);
        Task<bool> DeleteAsync(Rooms room);
        Task SaveChangesAsync(CancellationToken cancellationToken);
    }

    public class RoomRepository : IRoomRepository
    {
        private readonly CoworkingDbContext _context;
        public RoomRepository(CoworkingDbContext context)
        {
            _context = context;
        }

        public async Task<List<Rooms>> GetAllAsync() => await _context.Rooms.ToListAsync();

        public async Task<List<Rooms>> GetAllActiveAsync() =>
            await _context.Rooms.Where(r => r.IsActive).ToListAsync();
        public async Task<Rooms?> GetByIdAsync(int id) => 
            await _context.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        
        public async Task<Rooms?> GetByNameAsync(string name) => 
            await _context.Rooms.FirstOrDefaultAsync(r => r.Name == name);

        public async Task<bool> ExistingRoomName(string name) =>
            await _context.Rooms.AnyAsync(r => r.Name == name); 

        public async Task<int> AddAsync(Rooms room)
        {
            var addedEntity = _context.Rooms.Add(room);
            var entityId = -1;
            if (await _context.SaveChangesAsync() > -1)
            {
                entityId = Convert.ToInt32(addedEntity.Property("Id").CurrentValue);
            }

            return entityId;

        }

        public async Task<bool> UpdateAsync(Rooms roomData)
        {
            _context.Rooms.Attach(roomData);
            return (await _context.SaveChangesAsync() > 0);
        }

        public async Task<bool> DeleteAsync(Rooms room)
        {
            _context.Rooms.Remove(room);
            return (await _context.SaveChangesAsync() > 0);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken) => await _context.SaveChangesAsync(cancellationToken);
    }
