namespace Coworking.Domain.Entities;

public class Users
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    
    //El rol por defecto del usuario sera User
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Reservations> Reservations { get; set; } = new List<Reservations>();
}