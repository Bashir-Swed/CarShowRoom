using CarShowRoom.DAL.Models;
using CarShowRoom.DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarShowRoomApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FavoritesController : ControllerBase
    {
        private readonly FavoritesRepository _favRepo;

        public FavoritesController(FavoritesRepository favRepo)
        {
            _favRepo = favRepo;
        }

        // POST: api/Favorites/toggle/5
        [HttpPost("toggle/{carId}")]
        public async Task<IActionResult> Toggle(int carId)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
            var result = await _favRepo.ToggleFavoriteAsync(userId, carId);

            return Ok(new { Message = $"Car {result} favorites successfully." });
        }

        // GET: api/Favorites/my-favorites
        [HttpGet("my-favorites")]
        public async Task<IActionResult> GetMyFavorites()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
            var favorites = await _favRepo.GetUserFavoritesAsync(userId);
            return Ok(favorites);
        }
    }
}
