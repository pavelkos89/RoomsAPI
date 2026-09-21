using RoomsAPI.Data;
using RoomsAPI.Models;
using RoomsAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace RoomsAPI.Repositories;

public class ReservationRepository(AppDbContext db) : IReservationRepository
{
	public Task<List<Reservation>> GetByRoomAsync(int roomId, DateTime? from, DateTime? to, int skip, int take, CancellationToken cancellationToken)
	{
		var query = db.Reservations.AsNoTracking().Where(r => r.RoomId == roomId);

		// return reservations that overlap the requested range
		if (from.HasValue)
		{
			query = query.Where(r => r.End > from.Value);
		}

		if (to.HasValue)
		{
			query = query.Where(r => r.Start < to.Value);
		}

		return query
			.OrderBy(r => r.Start)
			.Skip(skip)
			.Take(take)
			.ToListAsync(cancellationToken);
	}

	public Task<Reservation?> FindOverlappingAsync(int roomId, DateTime start, DateTime end, CancellationToken cancellationToken) =>
		db.Reservations
			.AsNoTracking()
			.Where(r => r.RoomId == roomId && r.Start < end && start < r.End)
			.OrderBy(r => r.Start)
			.FirstOrDefaultAsync(cancellationToken);

	public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken)
	{
		db.Reservations.Add(reservation);
		await db.SaveChangesAsync(cancellationToken);
	}

	public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
		db.Database.BeginTransactionAsync(cancellationToken);
}
