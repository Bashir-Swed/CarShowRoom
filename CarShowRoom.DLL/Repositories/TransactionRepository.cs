using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
namespace CarShowRoom.DAL.Repositories
{
    public class TransactionRepository
    {
        private readonly string _connectionString;

        public TransactionRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<int> CreateTransactionAsync(TransactionCreateDto dto)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
            INSERT INTO Transactions (order_id, amount, payment_method, transaction_type, status, reference_number, notes, created_at)
            OUTPUT INSERTED.transaction_id
            VALUES (@order_id, @amount, @payment_method, @transaction_type, @status, @reference_number, @notes, GETDATE());";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@order_id", dto.OrderId);
            cmd.Parameters.AddWithValue("@amount", dto.Amount);
            cmd.Parameters.AddWithValue("@payment_method", dto.PaymentMethod);
            cmd.Parameters.AddWithValue("@transaction_type", (int)dto.TransactionType);
            cmd.Parameters.AddWithValue("@status", (int)dto.Status);
            cmd.Parameters.AddWithValue("@reference_number", (object?)dto.ReferenceNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@notes", (object?)dto.Notes ?? DBNull.Value);

            return (int)await cmd.ExecuteScalarAsync();
        }

        public async Task<List<TransactionSummaryDto>> GetTransactionsByOrderIdAsync(int orderId)
        {
            var list = new List<TransactionSummaryDto>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
            SELECT transaction_id, order_id, amount, payment_method, transaction_type, status, reference_number, notes, created_at
            FROM Transactions
            WHERE order_id = @order_id
            ORDER BY created_at DESC;";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@order_id", orderId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new TransactionSummaryDto
                {
                    TransactionId = (int)reader["transaction_id"],
                    OrderId = (int)reader["order_id"],
                    Amount = (decimal)reader["amount"],
                    PaymentMethod = reader["payment_method"].ToString()!,
                    TransactionType = (TransactionType)(int)reader["transaction_type"],
                    Status = (TransactionStatus)(int)reader["status"],
                    ReferenceNumber = reader["reference_number"] != DBNull.Value ? reader["reference_number"].ToString() : null,
                    Notes = reader["notes"] != DBNull.Value ? reader["notes"].ToString() : null,
                    CreatedAt = (DateTime)reader["created_at"]
                });
            }

            return list;
        }
    }
}
