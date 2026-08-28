using CarShowRoom.DAL.Enums;

public class TransactionSummaryDto
{
    public int TransactionId { get; set; }

    public int OrderId { get; set; }

    public int BuyerId { get; set; }

    public int SellerId { get; set; }

    public int CarId { get; set; }

    public OrderStatus OrderStatus { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; }
        = string.Empty;

    public TransactionType TransactionType { get; set; }

    public TransactionStatus Status { get; set; }

    public string? ReferenceNumber { get; set; }

    public string? Notes { get; set; }

    public List<TransactionContractImageDto> ContractImages{get;set;} = new();

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
}