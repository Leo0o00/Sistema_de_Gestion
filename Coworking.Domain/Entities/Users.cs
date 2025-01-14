using System.ComponentModel.DataAnnotations;

namespace Coworking.Domain.Entities;

public class Users
{
    public int Id { get; set; }
    
    [Required]
    public string Username { get; set; }
    
    [Required]
    public string Password { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    //El rol por defecto del usuario sera User
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Reservations> Reservations { get; set; } = new List<Reservations>();

    
}