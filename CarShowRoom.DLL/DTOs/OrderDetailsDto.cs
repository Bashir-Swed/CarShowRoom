using CarShowRoom.DAL.Enums;

public class OrderDetailsDto
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public int CarId { get; set; }
    public OrderType OrderType { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public decimal TotalPrice { get; set; }
    public string? UserNotes { get; set; }
    public string? AdminNotes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> DocumentUrls { get; set; } = new();

    public RentOrderDetailsDto? RentDetails { get; set; }
    public InstallmentOrderSummaryDto? InstallmentDetails { get; set; }
    // public BuyOrderDetailsDto? BuyDetails { get; set; }
}

