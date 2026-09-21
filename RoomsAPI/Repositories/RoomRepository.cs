using RoomsAPI.Data;
using RoomsAPI.Models;
using RoomsAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace RoomsAPI.Repositories;

public class RoomRepository(AppDbContext db) : IRoomRepository
{
	public Task<List<Room>> GetAllAsync(CancellationToken cancellationToken) =>
		db.Rooms
			.AsNoTracking()
			.OrderBy(r => r.Name)
			.ToListAsync(cancellationToken);

	public Task<bool> ExistsAsync(int roomId, CancellationToken cancellationToken) =>
		db.Rooms.AnyAsync(r => r.Id == roomId, cancellationToken);
}
