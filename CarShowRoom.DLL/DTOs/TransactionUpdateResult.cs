public class TransactionUpdateResult
{
    public bool Success { get; set; }

    public List<string> DeletedImageUrls { get; set; } = new();
}