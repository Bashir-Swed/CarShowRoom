using CarShowRoom.DAL.Models;
using CarShowRoom.DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarShowRoomApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Employee")]
    public class AdminController : ControllerBase
    {
        private readonly CarRepository _carRepo;
        private readonly UserRepository _userRepo;

        public AdminController(CarRepository carRepo)
        {
            _carRepo = carRepo;
        }
        public AdminController(UserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        [HttpPatch("approve/{id}")]
        public async Task<IActionResult> ApproveCar(int id, [FromBody] string notes)
        {
            var employeeId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

            var success = await _carRepo.ApproveCarAsync(id, employeeId, notes);

            if (success)
                return Ok(new { Message = "Car approved successfully." });

            return BadRequest(new { Message = "Failed to approve car." });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
            if (currentUserId == id)
            {
                return BadRequest(new { Message = "You cannot delete your own account." });
            }

            var success = await _userRepo.DeleteUserAsync(id);

            if (success)
                return Ok(new { Message = "User and all related data deleted successfully." });

            return BadRequest(new { Message = "Failed to delete user. Check if the user has active orders." });
        }
    }
}
