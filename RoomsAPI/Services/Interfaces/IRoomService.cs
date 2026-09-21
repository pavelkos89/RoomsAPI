using RoomsAPI.Models;

namespace RoomsAPI.Services.Interfaces;

public interface IRoomService
{
	Task<List<Room>> GetRoomsAsync(CancellationToken cancellationToken);
}
