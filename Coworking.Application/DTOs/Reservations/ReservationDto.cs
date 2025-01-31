﻿namespace Coworking.Application.DTOs.Reservations;

public class ReservationDto
{
    public int Id { get; set; }
    public int RoomId { get; set; }
    public int UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsCancelled { get; set; }

}