using CarShowRoom.DAL.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data.Common;
using System.Transactions;

namespace CarShowRoom.DAL.Repositories
{
    public class TransactionRepository
    {
        private readonly string _connectionString;

        public TransactionRepository(
            IConfiguration configuration)
        {
            _connectionString = configuration
                .GetConnectionString("DefaultConnection")!;
        }

        public async Task<int> CreateTransactionAsync(TransactionCreateDto dto,int createdBy,IReadOnlyCollection<string> contractImageUrls)
        {
            if (dto.Amount <= 0)
            {
                throw new InvalidOperationException(
                    "Transaction amount must be greater than zero."
                );
            }

            using var conn = new SqlConnection(_connectionString);

            await conn.OpenAsync();

            using var dbTransaction =conn.BeginTransaction();

            try
            {
                const string orderQuery = @"
                    SELECT order_status
                    FROM Orders
                    WHERE order_id = @order_id;";

                OrderStatus orderStatus;

                using (var cmd = new SqlCommand(orderQuery,conn,dbTransaction))
                {
                    cmd.Parameters.AddWithValue(
                        "@order_id",
                        dto.OrderId
                    );

                    var result =await cmd.ExecuteScalarAsync();

                    if (result == null ||
                        result == DBNull.Value)
                    {
                        throw new InvalidOperationException(
                            "Order was not found."
                        );
                    }

                    orderStatus = (OrderStatus)
                        Convert.ToInt32(result);
                }

                if (orderStatus != OrderStatus.Approved &&orderStatus != OrderStatus.Completed)
                {
                    throw new InvalidOperationException(
                        "A transaction can only be created for an approved order."
                    );
                }

                const string insertQuery = @"
                    INSERT INTO Transactions
                    (
                        order_id,
                        amount,
                        payment_method,
                        transaction_type,
                        status,
                        reference_number,
                        notes,
                        created_by,
                        created_at,
                        is_deleted
                    )
                    OUTPUT INSERTED.transaction_id
                    VALUES
                    (
                        @order_id,
                        @amount,
                        @payment_method,
                        @transaction_type,
                        @status,
                        @reference_number,
                        @notes,
                        @created_by,
                        GETDATE(),
                        0
                    );";

                int transactionId;

                using (var cmd = new SqlCommand(insertQuery,conn,dbTransaction))
                {
                    AddTransactionParameters(
                        cmd,
                        dto.OrderId,
                        dto.Amount,
                        dto.PaymentMethod,
                        dto.TransactionType,
                        dto.Status,
                        dto.ReferenceNumber,
                        dto.Notes
                    );

                    cmd.Parameters.AddWithValue("@created_by",createdBy);

                    var result = await cmd.ExecuteScalarAsync();

                    if (result == null ||result == DBNull.Value)
                    {
                        throw new InvalidOperationException(
                            "Failed to create transaction."
                        );
                    }

                    transactionId =Convert.ToInt32(result);
                }

                await InsertContractImagesAsync(
                    conn,
                    dbTransaction,
                    transactionId,
                    createdBy,
                    contractImageUrls
                );

                if (dto.Status == TransactionStatus.Completed)
                {
                    await CompleteOrderAndFinalizeCarAsync(
                        dto.OrderId,
                        conn,
                        dbTransaction);
                }

                await dbTransaction.CommitAsync();

                return transactionId;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<TransactionSummaryDto?>GetTransactionByIdAsync(int transactionId)
        {
            var transactions =
                await QueryTransactionsAsync(
                    "AND t.transaction_id = @transaction_id",
                    cmd => cmd.Parameters.AddWithValue(
                        "@transaction_id",
                        transactionId
                    )
                );

            return transactions.FirstOrDefault();
        }

        public async Task<List<TransactionSummaryDto>>GetAllTransactionsAsync()
        {
            return await QueryTransactionsAsync(
                string.Empty
            );
        }

        public async Task<List<TransactionSummaryDto>>GetTransactionsByOrderIdAsync(int orderId)
        {
            return await QueryTransactionsAsync(
                "AND t.order_id = @order_id",
                cmd => cmd.Parameters.AddWithValue(
                    "@order_id",
                    orderId
                )
            );
        }

        public async Task<List<TransactionSummaryDto>>GetTransactionsForUserAsync(int userId)
        {
            return await QueryTransactionsAsync(
                @"
                AND
                (
                    o.user_id = @user_id
                    OR c.user_id = @user_id
                )",
                cmd => cmd.Parameters.AddWithValue(
                    "@user_id",
                    userId
                )
            );
        }

        public async Task<TransactionUpdateResult>UpdateTransactionAsync(
                int transactionId,
                TransactionUpdateDto dto,
                int updatedBy,
                IReadOnlyCollection<string> newImageUrls)
        {
            if (dto.Amount <= 0)
            {
                throw new InvalidOperationException(
                    "Transaction amount must be greater than zero."
                );
            }

            var updateResult =
                new TransactionUpdateResult();

            using var conn =
                new SqlConnection(_connectionString);

            await conn.OpenAsync();

            using var dbTransaction =
                conn.BeginTransaction();

            try
            {
                const string updateQuery = @"
                    UPDATE Transactions
                    SET amount = @amount,
                        payment_method = @payment_method,
                        transaction_type = @transaction_type,
                        status = @status,
                        reference_number = @reference_number,
                        notes = @notes,
                        updated_by = @updated_by,
                        updated_at = GETDATE()
                    OUTPUT INSERTED.order_id
                    WHERE transaction_id = @transaction_id
                      AND is_deleted = 0;";

                int orderId;

                using (var cmd = new SqlCommand(
                    updateQuery,
                    conn,
                    dbTransaction))
                {
                    cmd.Parameters.AddWithValue(
                        "@transaction_id",
                        transactionId
                    );

                    cmd.Parameters.AddWithValue(
                        "@amount",
                        dto.Amount
                    );

                    cmd.Parameters.AddWithValue(
                        "@payment_method",
                        dto.PaymentMethod.Trim()
                    );

                    cmd.Parameters.AddWithValue(
                        "@transaction_type",
                        (int)dto.TransactionType
                    );

                    cmd.Parameters.AddWithValue(
                        "@status",
                        (int)dto.Status
                    );

                    cmd.Parameters.AddWithValue(
                        "@reference_number",
                        string.IsNullOrWhiteSpace(
                            dto.ReferenceNumber)
                            ? DBNull.Value
                            : dto.ReferenceNumber.Trim()
                    );

                    cmd.Parameters.AddWithValue(
                        "@notes",
                        string.IsNullOrWhiteSpace(dto.Notes)
                            ? DBNull.Value
                            : dto.Notes.Trim()
                    );

                    cmd.Parameters.AddWithValue(
                        "@updated_by",
                        updatedBy
                    );

                    var result =
                        await cmd.ExecuteScalarAsync();

                    if (result == null ||
                        result == DBNull.Value)
                    {
                        await dbTransaction.RollbackAsync();

                        return updateResult;
                    }

                    orderId = Convert.ToInt32(result);
                }

                if (dto.ContractImageIdsToDelete != null)
                {
                    foreach (int imageId in
                        dto.ContractImageIdsToDelete
                            .Where(id => id > 0)
                            .Distinct())
                    {
                        const string deleteImageQuery = @"
                            UPDATE Transaction_Contract_Images
                            SET is_deleted = 1,
                                deleted_at = GETDATE(),
                                deleted_by = @deleted_by
                            OUTPUT DELETED.image_url
                            WHERE image_id = @image_id
                              AND transaction_id =
                                  @transaction_id
                              AND is_deleted = 0;";

                        using var cmd = new SqlCommand(
                            deleteImageQuery,
                            conn,
                            dbTransaction
                        );

                        cmd.Parameters.AddWithValue(
                            "@image_id",
                            imageId
                        );

                        cmd.Parameters.AddWithValue(
                            "@transaction_id",
                            transactionId
                        );

                        cmd.Parameters.AddWithValue(
                            "@deleted_by",
                            updatedBy
                        );

                        var imageUrl =
                            await cmd.ExecuteScalarAsync();

                        if (imageUrl != null &&
                            imageUrl != DBNull.Value)
                        {
                            updateResult.DeletedImageUrls.Add(
                                imageUrl.ToString()!
                            );
                        }
                    }
                }

                await InsertContractImagesAsync(
                    conn,
                    dbTransaction,
                    transactionId,
                    updatedBy,
                    newImageUrls
                );

                if (dto.Status == TransactionStatus.Completed)
                {
                    await CompleteOrderAndFinalizeCarAsync(
                        orderId,
                        conn,
                        dbTransaction);
                }

                await dbTransaction.CommitAsync();

                updateResult.Success = true;

                return updateResult;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> SoftDeleteTransactionAsync(
            int transactionId,
            int deletedBy)
        {
            using var conn =
                new SqlConnection(_connectionString);

            await conn.OpenAsync();

            using var dbTransaction =
                conn.BeginTransaction();

            try
            {
                const string deleteTransactionQuery = @"
                    UPDATE Transactions
                    SET is_deleted = 1,
                        deleted_at = GETDATE(),
                        deleted_by = @deleted_by
                    WHERE transaction_id = @transaction_id
                      AND is_deleted = 0;";

                int affectedRows;

                using (var cmd = new SqlCommand(
                    deleteTransactionQuery,
                    conn,
                    dbTransaction))
                {
                    cmd.Parameters.AddWithValue(
                        "@transaction_id",
                        transactionId
                    );

                    cmd.Parameters.AddWithValue(
                        "@deleted_by",
                        deletedBy
                    );

                    affectedRows =
                        await cmd.ExecuteNonQueryAsync();
                }

                if (affectedRows == 0)
                {
                    await dbTransaction.RollbackAsync();
                    return false;
                }

                const string deleteImagesQuery = @"
                    UPDATE Transaction_Contract_Images
                    SET is_deleted = 1,
                        deleted_at = GETDATE(),
                        deleted_by = @deleted_by
                    WHERE transaction_id = @transaction_id
                      AND is_deleted = 0;";

                using (var cmd = new SqlCommand(
                    deleteImagesQuery,
                    conn,
                    dbTransaction))
                {
                    cmd.Parameters.AddWithValue(
                        "@transaction_id",
                        transactionId
                    );

                    cmd.Parameters.AddWithValue(
                        "@deleted_by",
                        deletedBy
                    );

                    await cmd.ExecuteNonQueryAsync();
                }

                await dbTransaction.CommitAsync();

                return true;
            }
            catch
            {
                await dbTransaction.RollbackAsync();
                throw;
            }
        }

        private async Task<List<TransactionSummaryDto>>QueryTransactionsAsync(string additionalWhere,Action<SqlCommand>? configureCommand = null)
        {
            var transactions =
                new Dictionary<int, TransactionSummaryDto>();

            const string selectQuery = @"
                SELECT
                    t.transaction_id,
                    t.order_id,
                    o.user_id AS buyer_id,
                    c.user_id AS seller_id,
                    o.car_id,
                    o.order_status,
                    t.amount,
                    t.payment_method,
                    t.transaction_type,
                    t.status,
                    t.reference_number,
                    t.notes,
                    t.created_by,
                    t.created_at,
                    t.updated_by,
                    t.updated_at,
                    i.image_id,
                    i.image_url,
                    i.uploaded_at AS image_uploaded_at
                FROM Transactions t
                INNER JOIN Orders o
                    ON o.order_id = t.order_id
                INNER JOIN Cars c
                    ON c.car_id = o.car_id
                LEFT JOIN Transaction_Contract_Images i
                    ON i.transaction_id =
                        t.transaction_id
                    AND i.is_deleted = 0
                WHERE t.is_deleted = 0 ";

            string query =
                selectQuery +
                Environment.NewLine +
                additionalWhere +
                Environment.NewLine +
                @"ORDER BY
                    t.created_at DESC,
                    i.uploaded_at ASC;";

            using var conn =
                new SqlConnection(_connectionString);

            using var cmd =
                new SqlCommand(query, conn);

            configureCommand?.Invoke(cmd);

            await conn.OpenAsync();

            using var reader =
                await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int transactionId =
                    reader.GetInt32(
                        reader.GetOrdinal(
                            "transaction_id"
                        )
                    );

                if (!transactions.TryGetValue(
                    transactionId,
                    out var transaction))
                {
                    transaction =
                        new TransactionSummaryDto
                        {
                            TransactionId =
                                transactionId,

                            OrderId = reader.GetInt32(
                                reader.GetOrdinal(
                                    "order_id"
                                )
                            ),

                            BuyerId = reader.GetInt32(
                                reader.GetOrdinal(
                                    "buyer_id"
                                )
                            ),

                            SellerId = reader.GetInt32(
                                reader.GetOrdinal(
                                    "seller_id"
                                )
                            ),

                            CarId = reader.GetInt32(
                                reader.GetOrdinal(
                                    "car_id"
                                )
                            ),

                            OrderStatus =
                                (OrderStatus)
                                reader.GetInt32(
                                    reader.GetOrdinal(
                                        "order_status"
                                    )
                                ),

                            Amount = reader.GetDecimal(
                                reader.GetOrdinal(
                                    "amount"
                                )
                            ),

                            PaymentMethod =
                                reader.GetString(
                                    reader.GetOrdinal(
                                        "payment_method"
                                    )
                                ),

                            TransactionType =
                                (TransactionType)
                                reader.GetInt32(
                                    reader.GetOrdinal(
                                        "transaction_type"
                                    )
                                ),

                            Status =
                                (TransactionStatus)
                                reader.GetInt32(
                                    reader.GetOrdinal(
                                        "status"
                                    )
                                ),

                            ReferenceNumber =
                                GetNullableString(
                                    reader,
                                    "reference_number"
                                ),

                            Notes = GetNullableString(
                                reader,
                                "notes"
                            ),

                            CreatedBy =
                                GetNullableInt(
                                    reader,
                                    "created_by"
                                ),

                            CreatedAt =
                                reader.GetDateTime(
                                    reader.GetOrdinal(
                                        "created_at"
                                    )
                                ),

                            UpdatedBy =
                                GetNullableInt(
                                    reader,
                                    "updated_by"
                                ),

                            UpdatedAt =
                                GetNullableDateTime(
                                    reader,
                                    "updated_at"
                                )
                        };

                    transactions.Add(
                        transactionId,
                        transaction
                    );
                }

                int imageOrdinal =
                    reader.GetOrdinal("image_id");

                if (!reader.IsDBNull(imageOrdinal))
                {
                    int imageId =
                        reader.GetInt32(imageOrdinal);

                    if (!transaction.ContractImages.Any(
                        image => image.ImageId == imageId))
                    {
                        transaction.ContractImages.Add(
                            new TransactionContractImageDto
                            {
                                ImageId = imageId,

                                ImageUrl =
                                    reader.GetString(
                                        reader.GetOrdinal(
                                            "image_url"
                                        )
                                    ),

                                UploadedAt =
                                    reader.GetDateTime(
                                        reader.GetOrdinal(
                                            "image_uploaded_at"
                                        )
                                    )
                            }
                        );
                    }
                }
            }

            return transactions.Values.ToList();
        }

        private static async Task InsertContractImagesAsync(
                SqlConnection conn,
                SqlTransaction dbTransaction,
                int transactionId,
                int uploadedBy,
                IEnumerable<string>? imageUrls)
        {
            if (imageUrls == null)
            {
                return;
            }

            const string insertImageQuery = @"
                INSERT INTO Transaction_Contract_Images
                (
                    transaction_id,
                    image_url,
                    uploaded_at,
                    uploaded_by,
                    is_deleted
                )
                VALUES
                (
                    @transaction_id,
                    @image_url,
                    GETDATE(),
                    @uploaded_by,
                    0
                );";

            foreach (string imageUrl in imageUrls
                .Where(url =>
                    !string.IsNullOrWhiteSpace(url))
                .Select(url => url.Trim())
                .Distinct())
            {
                using var cmd = new SqlCommand(
                    insertImageQuery,
                    conn,
                    dbTransaction
                );

                cmd.Parameters.AddWithValue(
                    "@transaction_id",
                    transactionId
                );

                cmd.Parameters.AddWithValue(
                    "@image_url",
                    imageUrl
                );

                cmd.Parameters.AddWithValue(
                    "@uploaded_by",
                    uploadedBy
                );

                await cmd.ExecuteNonQueryAsync();
            }
        }

        private static void AddTransactionParameters(
            SqlCommand cmd,
            int orderId,
            decimal amount,
            string paymentMethod,
            TransactionType transactionType,
            TransactionStatus status,
            string? referenceNumber,
            string? notes)
        {
            cmd.Parameters.AddWithValue(
                "@order_id",
                orderId
            );

            cmd.Parameters.AddWithValue(
                "@amount",
                amount
            );

            cmd.Parameters.AddWithValue(
                "@payment_method",
                paymentMethod.Trim()
            );

            cmd.Parameters.AddWithValue(
                "@transaction_type",
                (int)transactionType
            );

            cmd.Parameters.AddWithValue(
                "@status",
                (int)status
            );

            cmd.Parameters.AddWithValue(
                "@reference_number",
                string.IsNullOrWhiteSpace(referenceNumber)
                    ? DBNull.Value
                    : referenceNumber.Trim()
            );

            cmd.Parameters.AddWithValue(
                "@notes",
                string.IsNullOrWhiteSpace(notes)
                    ? DBNull.Value
                    : notes.Trim()
            );
        }

        private static string? GetNullableString(SqlDataReader reader,string columnName)
        {
            int ordinal =
                reader.GetOrdinal(columnName);

            return reader.IsDBNull(ordinal)
                ? null
                : reader.GetString(ordinal);
        }

        private static int? GetNullableInt(SqlDataReader reader,string columnName)
        {
            int ordinal =
                reader.GetOrdinal(columnName);

            return reader.IsDBNull(ordinal)
                ? null
                : reader.GetInt32(ordinal);
        }

        private static DateTime? GetNullableDateTime(SqlDataReader reader,string columnName)
        {
            int ordinal =
                reader.GetOrdinal(columnName);

            return reader.IsDBNull(ordinal)
                ? null
                : reader.GetDateTime(ordinal);
        }
        private async Task CompleteOrderAndFinalizeCarAsync(
    int orderId,
    SqlConnection connection,
    SqlTransaction transaction)
        {
            const string getOrderQuery = @"
        SELECT
            car_id,
            order_type,
            order_status
        FROM Orders
        WHERE order_id = @order_id;";

            int carId;
            OrderType orderType;
            OrderStatus currentOrderStatus;

            using (var command = new SqlCommand(
                getOrderQuery,
                connection,
                transaction))
            {
                command.Parameters.AddWithValue("@order_id", orderId);

                using var reader = await command.ExecuteReaderAsync();

                if (!await reader.ReadAsync())
                {
                    throw new InvalidOperationException("Order not found.");
                }

                carId = reader.GetInt32(reader.GetOrdinal("car_id"));

                orderType = (OrderType)reader.GetInt32(
                    reader.GetOrdinal("order_type"));

                currentOrderStatus = (OrderStatus)reader.GetInt32(
                    reader.GetOrdinal("order_status"));
            }

            if (currentOrderStatus != OrderStatus.Approved &&
                currentOrderStatus != OrderStatus.Completed)
            {
                throw new InvalidOperationException(
                    "Only an approved order can be completed.");
            }

            const string completeOrderQuery = @"
        UPDATE Orders
        SET order_status = @completed_status,
            updated_at = GETDATE()
        WHERE order_id = @order_id;";

            using (var command = new SqlCommand(
                completeOrderQuery,
                connection,
                transaction))
            {
                command.Parameters.AddWithValue(
                    "@completed_status",
                    (int)OrderStatus.Completed);

                command.Parameters.AddWithValue("@order_id", orderId);

                await command.ExecuteNonQueryAsync();
            }

            // البيع والتقسيط فقط يجعلان السيارة مباعة بشكل دائم.
            if (orderType == OrderType.Buy ||
                orderType == OrderType.Installment)
            {
                const string updateCarQuery = @"
            UPDATE Cars
            SET availability_status = @sold_status,
                updated_at = GETDATE()
            WHERE car_id = @car_id;";

                using var command = new SqlCommand(
                    updateCarQuery,
                    connection,
                    transaction);

                command.Parameters.AddWithValue(
                    "@sold_status",
                    (int)CarAvailabilityStatus.Sold);

                command.Parameters.AddWithValue("@car_id", carId);

                await command.ExecuteNonQueryAsync();
            }

            // لا نغيّر حالة سيارة الإيجار هنا.
            // سنحسب Rented لاحقاً اعتماداً على start_date و end_date.
        }
    }
}