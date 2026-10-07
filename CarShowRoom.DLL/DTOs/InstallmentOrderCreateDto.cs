using Microsoft.AspNetCore.Http;

public class InstallmentOrderCreateDto
{
    public int CarId { get; set; }
    public int InstallmentMonths { get; set; }
    public string? Notes { get; set; }
    public List<IFormFile>? Documents { get; set; }
}