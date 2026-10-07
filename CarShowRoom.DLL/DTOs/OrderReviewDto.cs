using CarShowRoom.DAL.Enums;

public class OrderReviewDto
{
    public int OrderId { get; set; }
    public OrderStatus Status { get; set; } // Approved (2) or Rejected (3)
    public string? AdminNotes { get; set; }
}