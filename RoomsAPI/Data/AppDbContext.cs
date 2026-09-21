using RoomsAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RoomsAPI.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
	public DbSet<Room> Rooms => Set<Room>();

	public DbSet<Reservation> Reservations => Set<Reservation>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<Room>(room =>
		{
			room.Property(r => r.Name).HasMaxLength(100);
			room.HasIndex(r => r.Name).IsUnique();
			room.ToTable(t => t.HasCheckConstraint("CK_Rooms_Capacity", "\"Capacity\" > 0"));

			room.HasData(
				new Room { Id = 1, Name = "Small Room", Capacity = 4 },
				new Room { Id = 2, Name = "Conference Room", Capacity = 10 },
				new Room { Id = 3, Name = "Auditorium", Capacity = 50 });
		});

		modelBuilder.Entity<Reservation>(reservation =>
		{
			reservation.Property(r => r.Title).HasMaxLength(200);

			reservation.HasOne<Room>()
				.WithMany()
				.HasForeignKey(r => r.RoomId)
				.OnDelete(DeleteBehavior.Restrict);

			// used by overlap check and range queries
			reservation.HasIndex(r => new { r.RoomId, r.Start, r.End });

			reservation.ToTable(t => t.HasCheckConstraint("CK_Reservations_Start_Before_End", "\"Start\" < \"End\""));
		});
	}

	protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
	{
		// SQLite stores dates as text and loses DateTimeKind, all our dates are UTC
		configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
	}

	private sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
		toDb => toDb,
		fromDb => DateTime.SpecifyKind(fromDb, DateTimeKind.Utc));
}
