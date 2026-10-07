using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

public class TransactionUpdateDto
{
    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99"
    )]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(50)]
    public string PaymentMethod { get; set; }
        = string.Empty;

    [EnumDataType(typeof(TransactionType))]
    public TransactionType TransactionType { get; set; }

    [EnumDataType(typeof(TransactionStatus))]
    public TransactionStatus Status { get; set; }

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(4000)]
    public string? Notes { get; set; }

    public List<IFormFile>? NewContractImages { get; set; }
        = new();

    public List<int>? ContractImageIdsToDelete { get; set; }
        = new();
}