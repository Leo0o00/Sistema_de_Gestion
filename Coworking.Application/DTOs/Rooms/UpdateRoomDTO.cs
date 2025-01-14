namespace Coworking.Application.DTOs.Rooms;

public class UpdateRoomDto
{
    public string Name { get; set; }
    public string Location { get; set; }
    public int Capacity { get; set; }
    public bool isActive { get; set; }
}