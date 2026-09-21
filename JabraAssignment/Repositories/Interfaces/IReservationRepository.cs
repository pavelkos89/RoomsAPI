using JabraAssignment.Models;
using Microsoft.EntityFrameworkCore.Storage;

namespace JabraAssignment.Repositories.Interfaces;

public interface IReservationRepository
{
	Task<List<Reservation>> GetByRoomAsync(int roomId, DateTime? from, DateTime? to, int skip, int take, CancellationToken cancellationToken);

	Task<Reservation?> FindOverlappingAsync(int roomId, DateTime start, DateTime end, CancellationToken cancellationToken);

	Task AddAsync(Reservation reservation, CancellationToken cancellationToken);

	Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken);
}
