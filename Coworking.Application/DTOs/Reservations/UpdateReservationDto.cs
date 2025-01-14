namespace Coworking.Application.DTOs.Reservations;

public class UpdateReservationDto
{
    public int RoomId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}