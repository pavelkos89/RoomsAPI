using RoomsAPI.Configuration;
using RoomsAPI.Dtos;
using RoomsAPI.Exceptions;
using RoomsAPI.Models;
using RoomsAPI.Repositories.Interfaces;
using RoomsAPI.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace RoomsAPI.Services;

public class ReservationService(
	IRoomRepository roomRepository,
	IReservationRepository reservationRepository,
	IOptions<PagingOptions> pagingOptions,
	ILogger<ReservationService> logger) : IReservationService
{
	public async Task<List<Reservation>> GetReservationsAsync(int roomId, ReservationQuery query, CancellationToken cancellationToken)
	{
		await EnsureRoomExistsAsync(roomId, cancellationToken);

		var pageSize = query.PageSize ?? pagingOptions.Value.DefaultPageSize;

		return await reservationRepository.GetByRoomAsync(
			roomId,
			query.From?.UtcDateTime,
			query.To?.UtcDateTime,
			skip: (query.Page - 1) * pageSize,
			take: pageSize,
			cancellationToken);
	}

	public async Task<Reservation> CreateReservationAsync(int roomId, CreateReservationRequest request, CancellationToken cancellationToken)
	{
		await EnsureRoomExistsAsync(roomId, cancellationToken);

		var reservation = new Reservation
		{
			RoomId = roomId,
			Start = request.Start!.Value.UtcDateTime,
			End = request.End!.Value.UtcDateTime,
			Title = request.Title,
			CreatedAt = DateTime.UtcNow
		};

		// check and insert in one transaction, otherwise two parallel requests could both pass the check
		await using var transaction = await reservationRepository.BeginTransactionAsync(cancellationToken);

		var conflict = await reservationRepository.FindOverlappingAsync(roomId, reservation.Start, reservation.End, cancellationToken);
		if (conflict != null)
		{
			logger.LogInformation("Reservation for room {RoomId} rejected, overlaps with reservation {ConflictId}", roomId, conflict.Id);

			throw new ConflictException(
				$"Room {roomId} is already booked from {conflict.Start:yyyy-MM-dd HH:mm} to {conflict.End:yyyy-MM-dd HH:mm} UTC (reservation {conflict.Id}).");
		}

		await reservationRepository.AddAsync(reservation, cancellationToken);
		await transaction.CommitAsync(cancellationToken);

		logger.LogInformation("Reservation {ReservationId} created for room {RoomId} from {Start:u} to {End:u}",
			reservation.Id, roomId, reservation.Start, reservation.End);

		return reservation;
	}

	private async Task EnsureRoomExistsAsync(int roomId, CancellationToken cancellationToken)
	{
		if (!await roomRepository.ExistsAsync(roomId, cancellationToken))
		{
			throw new NotFoundException($"Room {roomId} was not found.");
		}
	}
}
