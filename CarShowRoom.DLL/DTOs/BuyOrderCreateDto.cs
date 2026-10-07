using Microsoft.AspNetCore.Http;

public class BuyOrderCreateDto
{
    public int CarId { get; set; }
    public string? UserNotes { get; set; }
    public List<IFormFile>? Documents { get; set; }
}