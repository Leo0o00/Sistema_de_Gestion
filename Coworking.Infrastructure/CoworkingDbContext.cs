using Coworking.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Coworking.Infrastructure;

public class CoworkingDbContext : DbContext
{
    public CoworkingDbContext(DbContextOptions<CoworkingDbContext> options) : base(options)
    {
    }


    public DbSet<Users> Users => Set<Users>();
    public DbSet<Rooms> Rooms => Set<Rooms>();
    public DbSet<Reservations> Reservations => Set<Reservations>();
    public DbSet<ReservationAuditLog> ReservationAuditLog => Set<ReservationAuditLog>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Un User -> muchos Reservations
        modelBuilder.Entity<Users>()
            .HasMany(u => u.Reservations)   
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId);

        // Un Room -> muchas Reservations
        modelBuilder.Entity<Rooms>()
            .HasMany(r => r.Reservations)
            .WithOne(res => res.Room)
            .HasForeignKey(res => res.RoomId);
        // Indice unico para Room => Name
        modelBuilder.Entity<Rooms>()
            .HasIndex(r => r.Name)
            .IsUnique();

        // Indice único para Username y para Email
        modelBuilder.Entity<Users>()
            .HasIndex(u => new { u.Username, u.Email })
            .IsUnique();
        
        // Reservation -> ReservationAuditLog (1 a muchos)
        modelBuilder.Entity<Reservations>()
            .HasMany(r => r.AuditLogs)
            .WithOne(a => a.Reservation)
            .HasForeignKey(a => a.ReservationId);
    }
    
}