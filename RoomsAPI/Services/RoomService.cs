using RoomsAPI.Models;
using RoomsAPI.Repositories.Interfaces;
using RoomsAPI.Services.Interfaces;

namespace RoomsAPI.Services;

public class RoomService(IRoomRepository roomRepository) : IRoomService
{
	public Task<List<Room>> GetRoomsAsync(CancellationToken cancellationToken) =>
		roomRepository.GetAllAsync(cancellationToken);
}
