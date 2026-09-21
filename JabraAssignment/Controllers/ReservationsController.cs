using JabraAssignment.Dtos;
using JabraAssignment.Models;
using JabraAssignment.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JabraAssignment.Controllers;

[ApiController]
[Route("api/rooms/{roomId:int}/reservations")]
public class ReservationsController(IReservationService reservationService) : ControllerBase
{
	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<List<Reservation>>> GetReservations(
		int roomId, [FromQuery] ReservationQuery query, CancellationToken cancellationToken)
	{
		return await reservationService.GetReservationsAsync(roomId, query, cancellationToken);
	}

	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	[ProducesResponseType(StatusCodes.Status409Conflict)]
	public async Task<ActionResult<Reservation>> CreateReservation(
		int roomId, CreateReservationRequest request, CancellationToken cancellationToken)
	{
		var reservation = await reservationService.CreateReservationAsync(roomId, request, cancellationToken);

		return StatusCode(StatusCodes.Status201Created, reservation);
	}
}
