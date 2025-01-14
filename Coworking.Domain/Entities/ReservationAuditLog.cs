namespace Coworking.Domain.Entities;

public class ReservationAuditLog
{
    public int Id { get; set; }
    
    //FK a la reserva
    public int ReservationId { get; set; }
    public Reservations Reservation { get; set; } = default!;
    
    public string Action { get; set; } = default!; // "Created", "Edited", "Cancelled".
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }
}