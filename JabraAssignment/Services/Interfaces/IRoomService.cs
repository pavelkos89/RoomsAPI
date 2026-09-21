using JabraAssignment.Models;

namespace JabraAssignment.Services.Interfaces;

public interface IRoomService
{
	Task<List<Room>> GetRoomsAsync(CancellationToken cancellationToken);
}
