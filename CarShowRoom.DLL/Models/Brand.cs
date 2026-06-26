namespace CarShowRoom.DAL.Models
{
    public class Brand
    {
        public int BrandId { get; set; }
        public string Name { get; set; } = string.Empty;

        public string? BrandLogoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
