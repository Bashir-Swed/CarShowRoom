using CarShowRoom.DAL.DTOs;
using CarShowRoom.DAL.Models;
using CarShowRoom.DAL.Models.CarShowRoom.DAL.Models.CarShowRoom.DAL.Models;
using CarShowRoom.DAL.Repositories;
using CarShowRoomApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarShowRoomApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarsController : ControllerBase
    {
        private readonly CarRepository _carRepo;
        private readonly ImageService _imageService;

        public CarsController(CarRepository carRepo,ImageService imageService)
        {
            _carRepo = carRepo;
            _imageService = imageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetApprovedCars()
        {
            var cars = await _carRepo.GetAllApprovedCarsAsync();
            return Ok(cars);
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddCar([FromForm] CarCreateDto car)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var userRole=User.FindFirst(System.Security.Claims.ClaimTypes.Role);
            if (userIdClaim == null) return Unauthorized();

            int UserId = int.Parse(userIdClaim.Value);

            try
            {
                List<string> uploadedImagesUrls = new List<string>();

                if (car.Images != null && car.Images.Count > 0)
                {
                    foreach (var imageFile in car.Images)
                    {
                        string imageUrl = await _imageService.SaveImageAsync(imageFile, "cars");

                        uploadedImagesUrls.Add(imageUrl);
                    }
                }
                car.ImageUrls = uploadedImagesUrls;

                int newCarId = await _carRepo.AddCarUsingSPAsync(car,UserId);
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
            try
            {
                List<string> imageUrls = await _carRepo.GetCarImagesAsync(id);

                var success = await _carRepo.DeleteCarAsync(id, userId);

                if (success)
                {
                    if (imageUrls != null && imageUrls.Count > 0)
                    {
                        foreach (string url in imageUrls)
                        {
                            _imageService.DeleteImage(url);
                        }
                    }
                    return Ok(new { Message = "Car and all its related images deleted successfully." });
                }
                else
                    return BadRequest(new { Message = "Failed to delete car. You might not have permission." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }

        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCar(int id, [FromForm] CarCreateDto car)
        {
            var currentUserId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value!);
            
            int UserId = currentUserId;
            try
            {
             
                List<string> oldImageUrls = await _carRepo.GetCarImagesAsync(id);
                List<string> newImageUrls =  new List<string>();

                if(car.Images != null && car.Images.Count > 0)
{
                    foreach (var imageFile in car.Images)
                    {
                        string imageUrl = await _imageService.SaveImageAsync(imageFile, "cars");
                        newImageUrls.Add(imageUrl);
                    }
                    car.ImageUrls = newImageUrls;
                }

                var success = await _carRepo.UpdateCarAsync(car,UserId,id);
                if (success)
                {
                    if (car.Images != null && car.Images.Count > 0 && oldImageUrls != null)
                    {
                        foreach (var url in oldImageUrls)
                        {
                            _imageService.DeleteImage(url);
                        }
                    }

                    return Ok(new { Message = "Car information updated successfully and is pending re-approval." });
                }
                else
                {
                    if (newImageUrls.Count > 0)
                    {
                        foreach (var url in newImageUrls)
                        {
                            _imageService.DeleteImage(url);
                        }
                    }

                    return BadRequest(new { Message = "Failed to update car information." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCarById(int id)
        {
            try
            {
                var car = await _carRepo.GetCarInfoOnlyByIdAsync(id);

                if (car == null)
                {
                    return NotFound(new { Message = $"Car with ID {id} was not found." });
                }

                return Ok(car);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while retrieving the car data.", Details = ex.Message });
            }
        }

        [HttpGet("{id}/images")]
        public async Task<IActionResult> GetCarImages(int id)
        {
            try
            {
                List<string> imageUrls = await _carRepo.GetCarImagesAsync(id);
                return Ok(imageUrls);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching car images.", Details = ex.Message });
            }
        }

    }
}
