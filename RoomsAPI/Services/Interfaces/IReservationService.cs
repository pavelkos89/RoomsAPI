using RoomsAPI.Dtos;
using RoomsAPI.Models;

namespace RoomsAPI.Services.Interfaces;

public interface IReservationService
{
	Task<List<Reservation>> GetReservationsAsync(int roomId, ReservationQuery query, CancellationToken cancellationToken);

	Task<Reservation> CreateReservationAsync(int roomId, CreateReservationRequest request, CancellationToken cancellationToken);
}
