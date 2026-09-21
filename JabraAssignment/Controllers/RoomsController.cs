using JabraAssignment.Models;
using JabraAssignment.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace JabraAssignment.Controllers;

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
