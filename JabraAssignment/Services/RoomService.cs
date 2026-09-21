using JabraAssignment.Models;
using JabraAssignment.Repositories.Interfaces;
using JabraAssignment.Services.Interfaces;

namespace JabraAssignment.Services;

public class RoomService(IRoomRepository roomRepository) : IRoomService
{
	public Task<List<Room>> GetRoomsAsync(CancellationToken cancellationToken) =>
		roomRepository.GetAllAsync(cancellationToken);
}
