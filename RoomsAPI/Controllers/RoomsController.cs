using RoomsAPI.Models;
using RoomsAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace RoomsAPI.Controllers;

[ApiController]
[Route("api/rooms")]
public class RoomsController(IRoomService roomService) : ControllerBase
{
	[HttpGet]
	public async Task<ActionResult<List<Room>>> GetRooms(CancellationToken cancellationToken)
	{
		return await roomService.GetRoomsAsync(cancellationToken);
	}
}
