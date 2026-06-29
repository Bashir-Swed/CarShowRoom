namespace CarShowRoom.DAL.Models
{
    namespace CarShowRoom.DAL.Models
    {
        namespace CarShowRoom.DAL.Models
        {
            public class Car
            {
                public int CarId { get; set; }           
                public int UserId { get; set; }          
                public int BrandId { get; set; }
                public string Model { get; set; } = string.Empty; 
                public int Year { get; set; }           
                public string? Color { get; set; }       
                public decimal Price { get; set; }       
                public string? FuelType { get; set; }    
                public string? GearType { get; set; }    
                public int Mileage { get; set; }         
                public string? Description { get; set; } 
                public bool IsApproved { get; set; }     
                public decimal? RentPricePerDay { get; set; } 
                public string Status { get; set; } = "Pending";
                public DateTime CreatedAt { get; set; }  
                public int? ApprovedBy { get; set; }    
                public string? ApprovalNotes { get; set; } 
                public DateTime? ApprovalDate { get; set; } 

                public List<string> ImageUrls { get; set; } = new List<string>();

                public int? Cylinders { get; set; }
                public string? InteriorColor { get; set; }
                public int? KeysCount { get; set; }
                public string? DriveType { get; set; }
                public string? Region { get; set; }
                public int? Horsepower { get; set; }
                public int? TopSpeed { get; set; }

                public string? BrandLogoUrl { get; set; }
            }
        }
    }
}
