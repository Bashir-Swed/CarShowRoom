using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace CarShowRoom.DAL.DTOs
{
    public class CarCreateDto
    {
        public int? BrandId { get; set; }
        public string? Model { get; set; } 
        public int? Year { get; set; }
        public string? Color { get; set; }
        public decimal? Price { get; set; } = 0;
        public string? FuelType { get; set; }
        public string? GearType { get; set; }
        public int? Mileage { get; set; }
        public string? Description { get; set; }
        public decimal? RentPricePerDay { get; set; } = 0;
        public CarStatus Status { get; set; } = CarStatus.Pending;

        public List<string>? ImageUrls { get; set; } = new List<string>();

        public int? Cylinders { get; set; }
        public string? InteriorColor { get; set; }
        public int? KeysCount { get; set; }
        public string? DriveType { get; set; }
        public string? Region { get; set; }
        public int? Horsepower { get; set; }
        public int? TopSpeed { get; set; }
        public List<IFormFile>? Images { get; set; }
    }
}