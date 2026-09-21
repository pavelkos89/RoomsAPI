using JabraAssignment.Dtos;
using JabraAssignment.Models;

namespace JabraAssignment.Services.Interfaces;

public interface IReservationService
{
	Task<List<Reservation>> GetReservationsAsync(int roomId, ReservationQuery query, CancellationToken cancellationToken);

	Task<Reservation> CreateReservationAsync(int roomId, CreateReservationRequest request, CancellationToken cancellationToken);
}
