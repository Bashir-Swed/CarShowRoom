using CarShowRoom.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarShowRoom.DAL.DTOs;
using CarShowRoomApp.Services;


namespace CarShowRoomApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly BrandRepository _brandRepo;
        private readonly ImageService _imageService;

        public BrandsController(BrandRepository brandRepo, ImageService imageService)
        {
            _brandRepo = brandRepo;
            _imageService = imageService;
        }

        // GET: api/Brands
        [HttpGet("All-Brands")]
        public async Task<IActionResult> GetBrands()
        {
            var brands = await _brandRepo.GetAllBrandsAsync();
            return Ok(brands);
        }
        [HttpGet]
        public async Task<IActionResult>GetBrandByID(int id)
        {
            var brand = await _brandRepo.GetBrandByIDAsync(id);
            return Ok(brand);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            try
            {
                string? imageUrl = await _brandRepo.GetBrandImageUrlAsync(id);

                bool success = await _brandRepo.DeleteBrandAsync(id);

                if (!success)
                    return NotFound(new { Message = "Brand not found or could not be deleted." });

                if (!string.IsNullOrEmpty(imageUrl))
                {
                    _imageService.DeleteImage(imageUrl);
                }

                return Ok(new { Message = "Brand and its image deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

/*        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddBrand([FromBody] BrandAddDto brand)
        {
            if (string.IsNullOrWhiteSpace(brand.Name) || brand == null)
            {
                return BadRequest(new { Message = "Brand name cannot be empty." });
            }

            try
            {
                var newBrand = await _brandRepo.AddBrandAsync(brand);
                return Ok(new { Message = "Brand added successfully.", Data = newBrand });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }*/
        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateBrand([FromForm] BrandCreateDto dto)
        {
            try
            {
                string imageUrl = string.Empty;

                if (dto.ImageFile != null)
                {
                    imageUrl = await _imageService.SaveImageAsync(dto.ImageFile, "brands");
                }

                BrandAddDto brand = new BrandAddDto();
                brand.BrandLogoUrl = imageUrl;
                brand.Name = dto.BrandName;
                var success = await _brandRepo.AddBrandAsync(brand);

                if (success!=null)
                    return Ok(new { Message = "Brand created successfully", ImagePath = imageUrl, BrandId = success.BrandId, BrandName = success.Name });

                return BadRequest(new { Message = "Failed to create brand." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBrand(int id, [FromForm] BrandUpdateDto dto)
        {
            try
            {
                string? newImageUrl = null;

                string? oldImageUrl = await _brandRepo.GetBrandImageUrlAsync(id);

                if (dto.ImageFile != null)
                {
                    newImageUrl = await _imageService.SaveImageAsync(dto.ImageFile, "brands");

                }

                bool success = await _brandRepo.UpdateBrandAsync(id, dto.BrandName, newImageUrl);
                if (success)
                {
                    if (!string.IsNullOrEmpty(oldImageUrl))
                    {
                        _imageService.DeleteImage(oldImageUrl);
                    }
                    return Ok(new { Message = "Brand updated successfully", NewImagePath = newImageUrl });

                }
                else
                {
                    if (!string.IsNullOrEmpty(newImageUrl))
                    {
                        _imageService.DeleteImage(newImageUrl);
                    }
                    return BadRequest(new { Message = "Failed to update brand information." });
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }
    }
}
