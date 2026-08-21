public class TransactionCreateDto
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "Cash";
    public TransactionType TransactionType { get; set; }
    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}