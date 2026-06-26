using CarShowRoom.DAL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarShowRoom.DAL.DTOs;


namespace CarShowRoomApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly BrandRepository _brandRepo;

        public BrandsController(BrandRepository brandRepo)
        {
            _brandRepo = brandRepo;
        }

        // GET: api/Brands
        [HttpGet]
        public async Task<IActionResult> GetBrands()
        {
            var brands = await _brandRepo.GetAllBrandsAsync();
            return Ok(brands);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBrand(int id)
        {
            var success = await _brandRepo.DeleteBrandAsync(id);

            if (success)
                return Ok(new { Message = "Brand deleted successfully." });

            return BadRequest(new { Message = "Failed to delete brand. Make sure no cars are linked to this brand." });
        }

        [Authorize(Roles = "Admin")]
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
        }
    }
}
