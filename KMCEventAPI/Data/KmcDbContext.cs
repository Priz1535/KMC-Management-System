using KMCEventAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KMCEventAPI.Data;

public class KmcDbContext : DbContext
{
    public KmcDbContext(DbContextOptions<KmcDbContext> options) : base(options)
    {
    }

    public DbSet<EventOrganizer> EventOrganizers => Set<EventOrganizer>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<PublicUser> PublicUsers => Set<PublicUser>();
    public DbSet<Registration> Registrations => Set<Registration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EventOrganizer>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<PublicUser>()
            .HasIndex(x => x.Email)
            .IsUnique();

        modelBuilder.Entity<Registration>()
            .HasIndex(x => new { x.PublicUserID, x.EventID })
            .IsUnique();

        modelBuilder.Entity<Event>()
            .HasOne(x => x.Organizer)
            .WithMany(x => x.Events)
            .HasForeignKey(x => x.OrganizerID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Event>()
            .HasOne(x => x.Venue)
            .WithMany(x => x.Events)
            .HasForeignKey(x => x.VenueID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Registration>()
            .HasOne(x => x.PublicUser)
            .WithMany(x => x.Registrations)
            .HasForeignKey(x => x.PublicUserID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Registration>()
            .HasOne(x => x.Event)
            .WithMany(x => x.Registrations)
            .HasForeignKey(x => x.EventID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Venue>().HasData(
            new Venue
            {
                VenueID = 1,
                VenueName = "Kandy City Centre Hall",
                Address = "Dalada Veediya",
                City = "Kandy",
                Capacity = 450,
                Description = "Indoor venue suitable for community and cultural events."
            },
            new Venue
            {
                VenueID = 2,
                VenueName = "Kandy Municipal Grounds",
                Address = "Municipal Grounds",
                City = "Kandy",
                Capacity = 2500,
                Description = "Outdoor municipal venue for larger public events."
            },
            new Venue
            {
                VenueID = 3,
                VenueName = "Community Auditorium",
                Address = "Peradeniya Road",
                City = "Kandy",
                Capacity = 300,
                Description = "General-purpose auditorium for meetings and local programmes."
            }
        );
    }
}
