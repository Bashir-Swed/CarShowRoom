using CarShowRoom.DAL.Models;
using CarShowRoom.DAL.Models.CarShowRoom.DAL.Models.CarShowRoom.DAL.Models;
using CarShowRoom.DAL.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarShowRoomApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly CarRepository _carRepo;

        public CarsController(CarRepository carRepo)
        {
            _carRepo = carRepo;
        }

        [HttpGet]
        public async Task<IActionResult> GetApprovedCars()
        {
            var cars = await _carRepo.GetAllApprovedCarsAsync();
            return Ok(cars);
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddCar([FromBody] Car car)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var userRole=User.FindFirst(System.Security.Claims.ClaimTypes.Role);
            if (userIdClaim == null) return Unauthorized();

            car.UserId = int.Parse(userIdClaim.Value);

            try
            {

                int newCarId = await _carRepo.AddCarUsingSPAsync(car);
                if (newCarId > 0)
                {
                    if (User.IsInRole("Admin"))
                    {
                        return Ok(new
                        {
                            Message = "Car added and approved automatically by Admin.",
                            CarId = newCarId,
                            IsApproved = true
                        });
                    }
                    return Ok(new
                    {
                        Message = "Car added successfully. Pending admin approval.",
                        CarId = newCarId,
                        IsApproved = false
                    });
                }

                return BadRequest(new { Message = "Failed to add car." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpGet("pending-cars")]
        public async Task<IActionResult> GetPending()
        {
            var cars = await _carRepo.GetPendingCarsAsync();
            return Ok(cars);
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(int id)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

            var success = await _carRepo.DeleteCarAsync(id, userId);

            if (success)
                return Ok(new { Message = "Car deleted successfully." });

            return BadRequest(new { Message = "Failed to delete car. You might not have permission." });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCar(int id, [FromBody] Car car)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);

            car.CarId = id;
            car.UserId = currentUserId;

            var success = await _carRepo.UpdateCarAsync(car);

            if (success)
                return Ok(new { Message = "Car information updated successfully and is pending re-approval." });

            return BadRequest(new { Message = "Failed to update car information." });
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] int? brandId, [FromQuery] string? model,
                                        [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice,
                                        [FromQuery] int? year, [FromQuery] string? fuelType,
                                        [FromQuery] string? gearType)
        {
            var results = await _carRepo.SearchCarsAsync(brandId, model, minPrice, maxPrice, year, fuelType, gearType);
            return Ok(results);
        }

        [Authorize]
        [HttpGet("my-cars")]
        public async Task<IActionResult> GetMyCars()
        {
            try
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

                if (userIdClaim == null)
                {
                    return Unauthorized(new { Message = "User identity not found in token." });
                }

                int userId = int.Parse(userIdClaim.Value);

                var myCars = await _carRepo.GetUserCarsAsync(userId);

                if (myCars == null || myCars.Count == 0)
                {
                    return Ok(new { Message = "You haven't added any cars yet.", Data = new List<Car>() });
                }

                return Ok(myCars);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching your cars.", Detail = ex.Message });
            }
        }

    }
}
