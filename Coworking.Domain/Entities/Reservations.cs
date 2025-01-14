namespace Coworking.Domain.Entities;

public class Reservations
{
    public int Id { get; set; }
    
    //FK a la sala
    public int RoomId { get; set; }
    public Rooms Room { get; set; }
    
    //FK al usuario
    public int UserId { get; set; }
    public Users User { get; set; }
    
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsCancelled { get; set; }
    
    //Auditoria de cambios
    public virtual ICollection<ReservationAuditLog> AuditLogs { get; set; } = new List<ReservationAuditLog>();
    
}