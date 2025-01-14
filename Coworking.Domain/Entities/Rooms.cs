namespace Coworking.Domain.Entities;
public class Rooms 
{
        public int Id { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<Reservations> Reservations { get; set; } = new List<Reservations>();
}
    


