using JabraAssignment.Models;

namespace JabraAssignment.Repositories.Interfaces;

public interface IRoomRepository
{
	Task<List<Room>> GetAllAsync(CancellationToken cancellationToken);

	Task<bool> ExistsAsync(int roomId, CancellationToken cancellationToken);
}
