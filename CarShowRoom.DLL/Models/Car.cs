namespace CarShowRoom.DAL.Models
{
    namespace CarShowRoom.DAL.Models
    {
        namespace CarShowRoom.DAL.Models
        {
            public class Car
            {
                public int CarId { get; set; }           // PK car_id [cite: 30]
                public int UserId { get; set; }          // FK user_id (صاحب السيارة) [cite: 32]
                public string Brand { get; set; } = string.Empty; // brand [cite: 33]
                public string Model { get; set; } = string.Empty; // model [cite: 34]
                public int Year { get; set; }            // year [cite: 53]
                public string? Color { get; set; }       // color [cite: 6]
                public decimal Price { get; set; }       // price [cite: 54]
                public string? FuelType { get; set; }    // fuel_type [cite: 55]
                public string? GearType { get; set; }    // gear_type [cite: 56]
                public int Mileage { get; set; }         // mileage [cite: 57]
                public string? Description { get; set; } // description [cite: 58]
                public bool IsApproved { get; set; }     // is_approved 
                public decimal? RentPricePerDay { get; set; } // rent_price_per_day 
                public string Status { get; set; } = "Available"; // status [cite: 61]
                public DateTime CreatedAt { get; set; }  // created_at [cite: 62]
                public int? ApprovedBy { get; set; }     // FK approved_by [cite: 63]
                public string? ApprovalNotes { get; set; } // approval_notes [cite: 64]
                public DateTime? ApprovalDate { get; set; } // approval_date [cite: 64]

                public List<string> ImageUrls { get; set; } = new List<string>(); 
            }
        }
    }
}
