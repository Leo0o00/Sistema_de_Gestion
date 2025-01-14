using Coworking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Infrastructure.Repositories;

public interface IReservationsRepository
{
    // Crear
    Task AddAsync(Reservations reservation);
        
    // Obtener por Id
    Task<Reservations?> GetByIdAsync(int reservationId);
        
    // Listar todas
    Task<List<Reservations>> GetAllAsync();
    
    Task<bool> FindOverlap(int roomId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken);
        
    // (Opcional) Listar por usuario
    Task<List<Reservations>> GetByUserIdAsync(int userId);
        
    // Actualizar
    Task UpdateAsync(Reservations reservation);
        
    // Eliminar físicamente
    void Delete(Reservations reservation);
        
    // Guardar cambios
    Task SaveChangesAsync(CancellationToken cancellationToken);
}


public class ReservationsRepository : IReservationsRepository
{
    private readonly CoworkingDbContext _context;

    public ReservationsRepository(CoworkingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Reservations reservation)
    {
        await _context.Reservations.AddAsync(reservation);
    }

    public async Task<Reservations?> GetByIdAsync(int reservationId)
    {
        
        return await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == reservationId);
    }

    public async Task<List<Reservations>> GetAllAsync()
    {
        return await _context.Reservations
            .ToListAsync();
    }

    
    public async Task<bool> FindOverlap(int roomId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken)
    {
        return await _context.Reservations.AnyAsync(r =>
                r.RoomId == roomId &&
                !r.IsCancelled &&
                (
                    (startTime >= r.StartTime && startTime < r.EndTime) ||
                    (endTime > r.StartTime && endTime <= r.EndTime)
                ),
            cancellationToken
        );
    }
    public async Task<List<Reservations>> GetByUserIdAsync(int userId)
    {
        return await _context.Reservations
            .Where(r => r.UserId == userId)
            .ToListAsync();
    }

    public Task UpdateAsync(Reservations reservation)
    {
        _context.Reservations.Update(reservation);
        return Task.CompletedTask;
        
    }

    public void Delete(Reservations reservation)
    {
        _context.Reservations.Remove(reservation);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}