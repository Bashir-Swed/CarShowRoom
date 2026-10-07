/*using CarShowRoom.DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarShowRoomApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CarImagesController : ControllerBase
    {
        private readonly CarRepository _carRepo;

        public CarImagesController(CarRepository carRepo)
        {
            _carRepo = carRepo;
        }

        [HttpPost("{carId}")]
        public async Task<IActionResult> AddImage(int carId, [FromBody] string imageUrl)
        {
            var success = await _carRepo.AddImageToCarAsync(carId, imageUrl);
            if (success) return Ok(new { Message = "Image added successfully." });
            return BadRequest(new { Message = "Failed to add image." });
        }

        [HttpDelete("{imageId}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
            var success = await _carRepo.DeleteImageAsync(imageId, userId);

            if (success) return Ok(new { Message = "Image deleted successfully." });
            return BadRequest(new { Message = "Failed to delete image or unauthorized." });
        }
    }
}
*/