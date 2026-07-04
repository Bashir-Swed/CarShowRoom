using Microsoft.AspNetCore.Http;

namespace CarShowRoom.DAL.DTOs
{
    public class BrandUpdateDto
    {
        public string BrandName { get; set; } = string.Empty;
        public IFormFile? ImageFile { get; set; }
    }
}