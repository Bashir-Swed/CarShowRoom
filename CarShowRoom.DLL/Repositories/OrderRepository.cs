using CarShowRoom.DAL.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarShowRoom.DAL.Repositories
{
    public class OrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }
        //Rent Order Creation with Document Uploads
        public async Task<Int32> AddRentOrderAsync(RentOrderCreateDto dto, int userId, List<string> documentUrls)
        {
            await ValidateCarAvailabilityAsync(dto.CarId, OrderType.Rent, dto.StartDate, dto.EndDate);
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var transaction = conn.BeginTransaction();

            try
            {
                string getPriceQuery = "SELECT rent_price_per_day FROM Cars WHERE car_id = @car_id";
                using var cmdPrice = new SqlCommand(getPriceQuery, conn, transaction);
                cmdPrice.Parameters.AddWithValue("@car_id", dto.CarId);

                var priceResult = await cmdPrice.ExecuteScalarAsync();
                if (priceResult == null|| priceResult == DBNull.Value)
                    throw new Exception("Car not found or rental price per day is not set for this car.");

                decimal rentPricePerDay = Convert.ToDecimal(priceResult);
                int totalDays = (dto.EndDate - dto.StartDate).Days;

                if (totalDays <= 0)
                    throw new Exception("End date must be after start date.");

                decimal totalPrice = rentPricePerDay * totalDays;

                string insertOrderQuery = @"
            INSERT INTO Orders (user_id, car_id, order_type, order_status, total_price, user_notes, created_at)
            OUTPUT INSERTED.order_id
            VALUES (@user_id, @car_id, 1, 1, @total_price, @user_notes, GETDATE());";
                // order_type = 1 (Rent) | order_status = 1 (Pending)

                using var cmdOrder = new SqlCommand(insertOrderQuery, conn, transaction);
                cmdOrder.Parameters.AddWithValue("@user_id", userId);
                cmdOrder.Parameters.AddWithValue("@car_id", dto.CarId);
                cmdOrder.Parameters.AddWithValue("@total_price", totalPrice);
                cmdOrder.Parameters.AddWithValue("@user_notes", (object?)dto.UserNotes ?? DBNull.Value);

                int newOrderId = (int)await cmdOrder.ExecuteScalarAsync();

                string insertRentQuery = @"
            INSERT INTO Rent_Orders (order_id, start_date, end_date)
            VALUES (@order_id, @start_date, @end_date);";

                using var cmdRent = new SqlCommand(insertRentQuery, conn, transaction);
                cmdRent.Parameters.AddWithValue("@order_id", newOrderId);
                cmdRent.Parameters.AddWithValue("@start_date", dto.StartDate);
                cmdRent.Parameters.AddWithValue("@end_date", dto.EndDate);
                await cmdRent.ExecuteNonQueryAsync();

                if (documentUrls != null && documentUrls.Count > 0)
                {
                    string insertDocsQuery = "INSERT INTO Order_Documents (order_id, document_url) VALUES (@order_id, @url);";
                    foreach (var url in documentUrls)
                    {
                        using var cmdDoc = new SqlCommand(insertDocsQuery, conn, transaction);
                        cmdDoc.Parameters.AddWithValue("@order_id", newOrderId);
                        cmdDoc.Parameters.AddWithValue("@url", url);
                        await cmdDoc.ExecuteNonQueryAsync();
                    }
                }

                transaction.Commit();
                return newOrderId;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
        public async Task<bool> ReviewOrderAsync(OrderReviewDto dto)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                string getOrderDetailsQuery = "SELECT car_id, order_type FROM Orders WHERE order_id = @order_id;";
                int carId = 0;
                OrderType orderType;

                using (var cmdGet = new SqlCommand(getOrderDetailsQuery, conn, transaction))
                {
                    cmdGet.Parameters.AddWithValue("@order_id", dto.OrderId);
                    using var reader = await cmdGet.ExecuteReaderAsync();
                    if (!await reader.ReadAsync())
                    {
                        return false;
                    }
                    carId = reader.GetInt32(reader.GetOrdinal("car_id"));
                    orderType = (OrderType)reader.GetInt32(reader.GetOrdinal("order_type"));
                }

                string updateOrderQuery = @"
            UPDATE Orders
            SET order_status = @order_status,
                admin_notes = @admin_notes,
                updated_at = GETDATE()
            WHERE order_id = @order_id;";

                using (var cmdOrder = new SqlCommand(updateOrderQuery, conn, transaction))
                {
                    cmdOrder.Parameters.AddWithValue("@order_id", dto.OrderId);
                    cmdOrder.Parameters.AddWithValue("@order_status", (int)dto.Status);
                    cmdOrder.Parameters.AddWithValue("@admin_notes", (object?)dto.AdminNotes ?? DBNull.Value);

                    int rowsAffected = await cmdOrder.ExecuteNonQueryAsync();
                    if (rowsAffected == 0) return false;
                }

                if (dto.Status == OrderStatus.Approved)
                {
                    CarStatus newCarStatus = orderType switch
                    {
                        OrderType.Buy => CarStatus.Sold,
                        OrderType.Rent => CarStatus.Rented,
                        OrderType.Installment => CarStatus.Sold,
                        _ => CarStatus.Available
                    };

                    string updateCarQuery = "UPDATE Cars SET status = @car_status WHERE car_id = @car_id;";
                    using (var cmdCar = new SqlCommand(updateCarQuery, conn, transaction))
                    {
                        cmdCar.Parameters.AddWithValue("@car_id", carId);
                        cmdCar.Parameters.AddWithValue("@car_status", (int)newCarStatus);
                        await cmdCar.ExecuteNonQueryAsync();
                    }
                }

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<OrderDetailsDto?> GetOrderDetailsAsync(int orderId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string queryOrder = @"
        SELECT order_id, user_id, car_id, order_type, order_status, 
               total_price, user_notes, admin_notes, created_at
        FROM Orders 
        WHERE order_id = @order_id;";

            OrderDetailsDto? order = null;
            int orderTypeInt = 0;

            using (var cmd = new SqlCommand(queryOrder, conn))
            {
                cmd.Parameters.AddWithValue("@order_id", orderId);
                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    orderTypeInt = reader.GetInt32(reader.GetOrdinal("order_type"));
                    order = new OrderDetailsDto
                    {
                        OrderId = reader.GetInt32(reader.GetOrdinal("order_id")),
                        UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                        CarId = reader.GetInt32(reader.GetOrdinal("car_id")),
                        OrderType = (OrderType)orderTypeInt,
                        OrderStatus = (OrderStatus)reader.GetInt32(reader.GetOrdinal("order_status")),
                        TotalPrice = reader.GetDecimal(reader.GetOrdinal("total_price")),
                        UserNotes = reader.IsDBNull(reader.GetOrdinal("user_notes")) ? null : reader.GetString(reader.GetOrdinal("user_notes")),
                        AdminNotes = reader.IsDBNull(reader.GetOrdinal("admin_notes")) ? null : reader.GetString(reader.GetOrdinal("admin_notes")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                    };
                }
            }

            if (order == null) return null;

            switch (order.OrderType)        
            {                
                case OrderType.Rent:           
                    order.RentDetails = await FetchRentSpecificDetailsAsync(conn, orderId);
                    break;
                case OrderType.Installment:
                    order.InstallmentDetails = await FetchInstallmentSummaryDetailsAsync(conn, order.OrderId);
                    break;

                // future cases:
                // case OrderType.Buy:
                //     order.BuyDetails = await FetchBuySpecificDetailsAsync(conn, orderId);
                //     break;

                default:
                    // no extra details for other types
                    break;
            }

            order.DocumentUrls = await FetchOrderDocumentsAsync(conn, orderId);
            return order;
        }
        public async Task<List<OrderDetailsDto>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = new List<OrderDetailsDto>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
        SELECT order_id, car_id, order_type, order_status,user_notes, admin_notes, total_price, created_at
        FROM Orders
        WHERE user_id = @user_id
        ORDER BY created_at DESC;";

            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@user_id", userId);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    orders.Add(new OrderDetailsDto
                    {
                        OrderId = reader.GetInt32(reader.GetOrdinal("order_id")),
                        CarId = reader.GetInt32(reader.GetOrdinal("car_id")),
                        UserId=userId,
                        OrderType = (OrderType)reader.GetInt32(reader.GetOrdinal("order_type")),
                        OrderStatus = (OrderStatus)reader.GetInt32(reader.GetOrdinal("order_status")),
                        UserNotes = reader.IsDBNull(reader.GetOrdinal("user_notes")) ? null : reader.GetString(reader.GetOrdinal("user_notes")),
                        AdminNotes = reader.IsDBNull(reader.GetOrdinal("admin_notes")) ? null : reader.GetString(reader.GetOrdinal("admin_notes")),
                        TotalPrice = reader.GetDecimal(reader.GetOrdinal("total_price")),
                        CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                    });
                }
            }

            foreach (var order in orders)
            {
                switch (order.OrderType)
                {
                    case OrderType.Rent:
                        order.RentDetails = await FetchRentSpecificDetailsAsync(conn, order.OrderId);
                        break;
                    case OrderType.Installment:
                        order.InstallmentDetails = await FetchInstallmentSummaryDetailsAsync(conn, order.OrderId);
                        break;
                        // case OrderType.Buy:
                        //     order.BuyDetails = await FetchBuySpecificDetailsAsync(conn, order.OrderId);
                        //     break;
                }
            }

            return orders;
        }
        public async Task<List<OrderDetailsDto>> GetOrdersForAdminAsync(OrderStatus? status, OrderType? type)
        {
            var orders = new List<OrderDetailsDto>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var queryBuilder = new StringBuilder(@"
       SELECT order_id, user_id, car_id, order_type, order_status, 
               total_price, user_notes, admin_notes, created_at
        FROM Orders
        WHERE 1=1 ");

            if (status.HasValue)
            {
                queryBuilder.Append(" AND order_status = @status");
            }

            if (type.HasValue)
            {
                queryBuilder.Append(" AND order_type = @type");
            }

            queryBuilder.Append(" ORDER BY created_at DESC;");

            using (var cmd = new SqlCommand(queryBuilder.ToString(), conn))
            {
                if (status.HasValue)
                    cmd.Parameters.AddWithValue("@status", (int)status.Value);

                if (type.HasValue)
                    cmd.Parameters.AddWithValue("@type", (int)type.Value);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        orders.Add(new OrderDetailsDto
                        {
                        
                            OrderId = reader.GetInt32(reader.GetOrdinal("order_id")),
                            UserId = reader.GetInt32(reader.GetOrdinal("user_id")),
                            CarId = reader.GetInt32(reader.GetOrdinal("car_id")),
                            OrderType =(OrderType) reader.GetInt32(reader.GetOrdinal("order_type")),
                            OrderStatus = (OrderStatus)reader.GetInt32(reader.GetOrdinal("order_status")),
                            TotalPrice = reader.GetDecimal(reader.GetOrdinal("total_price")),
                            UserNotes = reader.IsDBNull(reader.GetOrdinal("user_notes")) ? null : reader.GetString(reader.GetOrdinal("user_notes")),
                            AdminNotes = reader.IsDBNull(reader.GetOrdinal("admin_notes")) ? null : reader.GetString(reader.GetOrdinal("admin_notes")),
                            CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at"))
                        });
                    }
                }
            }
            foreach (var order in orders)
            {
                switch (order.OrderType)
                {
                    case OrderType.Rent:
                        order.RentDetails = await FetchRentSpecificDetailsAsync(conn, order.OrderId);
                        break;
                    case OrderType.Installment:
                        order.InstallmentDetails = await FetchInstallmentSummaryDetailsAsync(conn, order.OrderId);
                        break;
                        // case OrderType.Buy:
                        //     order.BuyDetails = await FetchBuySummaryDetailsAsync(conn, order.OrderId);
                        //     break;
                }
            }

            return orders;
        }
        public async Task<bool> CancelOrderAsync(int orderId, int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
        UPDATE Orders
        SET order_status = @canceled_status,
            updated_at = GETDATE()
        WHERE order_id = @order_id 
          AND user_id = @user_id 
          AND order_status = @pending_status;";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@order_id", orderId);
            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.Parameters.AddWithValue("@canceled_status", (int)OrderStatus.Canceled);
            cmd.Parameters.AddWithValue("@pending_status", (int)OrderStatus.Pending);

            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        /*public async Task<bool> IsCarAvailableAsync(int carId, DateTime startDate, DateTime endDate)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = @"
        SELECT COUNT(1)
        FROM Rent_Orders ro
        INNER JOIN Orders o ON ro.order_id = o.order_id
        WHERE o.car_id = @car_id
          AND o.order_status IN (@pending, @approved)
          AND ro.start_date < @end_date
          AND ro.end_date > @start_date;";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@car_id", carId);
            cmd.Parameters.AddWithValue("@pending", (int)OrderStatus.Pending);
            cmd.Parameters.AddWithValue("@approved", (int)OrderStatus.Approved);
            cmd.Parameters.AddWithValue("@start_date", startDate);
            cmd.Parameters.AddWithValue("@end_date", endDate);

            int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());

            return count == 0;
        }*/
        public async Task<bool> IsCarAvailableAsync(int carId, OrderType orderType, DateTime? startDate = null, DateTime? endDate = null)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string checkSoldQuery = @"
        SELECT COUNT(1)
        FROM Orders
        WHERE car_id = @car_id
          AND order_status = @approved_status
          AND order_type IN (@buy_type , @installment_type);";

            using (var cmdSold = new SqlCommand(checkSoldQuery, conn))
            {
                cmdSold.Parameters.AddWithValue("@car_id", carId);
                cmdSold.Parameters.AddWithValue("@approved_status", (int)OrderStatus.Approved);
                cmdSold.Parameters.AddWithValue("@buy_type", (int)OrderType.Buy);
                cmdSold.Parameters.AddWithValue("@installment_type", (int)OrderType.Installment);

                int soldCount = Convert.ToInt32(await cmdSold.ExecuteScalarAsync());
                if (soldCount > 0)
                {
                    return false;
                }
            }

            if (orderType == OrderType.Rent)
            {
                if (!startDate.HasValue || !endDate.HasValue)
                {
                    throw new ArgumentException("Start date and End date are required when checking Rent availability.");
                }

                string checkRentOverlapQuery = @"
            SELECT COUNT(1)
            FROM Rent_Orders ro
            INNER JOIN Orders o ON ro.order_id = o.order_id
            WHERE o.car_id = @car_id
              AND o.order_status IN (@completed_status, @approved_status)
              AND ro.start_date < @end_date
              AND ro.end_date > @start_date;";

                using var cmdRent = new SqlCommand(checkRentOverlapQuery, conn);
                cmdRent.Parameters.AddWithValue("@car_id", carId);
                cmdRent.Parameters.AddWithValue("@completed_status", (int)OrderStatus.Completed);
                cmdRent.Parameters.AddWithValue("@approved_status", (int)OrderStatus.Approved);
                cmdRent.Parameters.AddWithValue("@start_date", startDate.Value);
                cmdRent.Parameters.AddWithValue("@end_date", endDate.Value);

                int overlapCount = Convert.ToInt32(await cmdRent.ExecuteScalarAsync());
                if (overlapCount > 0)
                {
                    return false;
                }
            }

            if (orderType == OrderType.Buy)
            {
                string checkPendingBuyQuery = @"
            SELECT COUNT(1)
            FROM Orders
            WHERE car_id = @car_id
              AND order_type = @buy_type
              AND order_status IN( @approved_status,@completed_status);";

                using var cmdBuy = new SqlCommand(checkPendingBuyQuery, conn);
                cmdBuy.Parameters.AddWithValue("@car_id", carId);
                cmdBuy.Parameters.AddWithValue("@buy_type", (int)OrderType.Buy);
                cmdBuy.Parameters.AddWithValue("@completed_status", (int)OrderStatus.Completed);
                cmdBuy.Parameters.AddWithValue("@approved_status", (int)OrderStatus.Approved);

                int pendingBuyCount = Convert.ToInt32(await cmdBuy.ExecuteScalarAsync());
                if (pendingBuyCount > 0)
                {
                    return false;
                }
            }
            if (orderType == OrderType.Installment)
            {
                string checkPendingInstallmentQuery = @"
        SELECT COUNT(1)
        FROM Orders
        WHERE car_id = @car_id
          AND order_type = @installment_type
          AND order_status IN(@completed_status,@approved_status);";

                using var cmdInstallment = new SqlCommand(checkPendingInstallmentQuery, conn);
                cmdInstallment.Parameters.AddWithValue("@car_id", carId);
                cmdInstallment.Parameters.AddWithValue("@installment_type", (int)OrderType.Installment);
                cmdInstallment.Parameters.AddWithValue("@completed_status", (int)OrderStatus.Completed);
                cmdInstallment.Parameters.AddWithValue("@approved_status", (int)OrderStatus.Approved);

                int pendingInstallmentCount = Convert.ToInt32(await cmdInstallment.ExecuteScalarAsync());
                if (pendingInstallmentCount > 0)
                {
                    return false;
                }
            }

            return true;
        }

        //Buy Order Creation 
        public async Task<int> AddBuyOrderAsync(BuyOrderCreateDto dto, int userId, List<string> documentUrls)
        {
            await ValidateCarAvailabilityAsync(dto.CarId, OrderType.Buy);
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                string getPriceQuery = "SELECT price FROM Cars WHERE car_id = @car_id;";
                decimal carPrice = 0;

                using (var cmdPrice = new SqlCommand(getPriceQuery, conn, transaction))
                {
                    cmdPrice.Parameters.AddWithValue("@car_id", dto.CarId);
                    var result = await cmdPrice.ExecuteScalarAsync();

                    if (result == null || result == DBNull.Value)
                    {
                        throw new InvalidOperationException("Car not found or price is invalid.");
                    }

                    carPrice = Convert.ToDecimal(result);
                }

                string insertBaseOrder = @"
            INSERT INTO Orders (car_id, user_id, order_type, order_status,user_notes, total_price, created_at)
            OUTPUT INSERTED.order_id
            VALUES (@car_id, @user_id, @order_type, @order_status, @user_notes, @total_price, GETDATE());";

                int orderId;
                using (var cmd = new SqlCommand(insertBaseOrder, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@car_id", dto.CarId);
                    cmd.Parameters.AddWithValue("@user_id", userId);
                    cmd.Parameters.AddWithValue("@order_type", (int)OrderType.Buy);
                    cmd.Parameters.AddWithValue("@order_status", (int)OrderStatus.Pending);
                    cmd.Parameters.AddWithValue("@total_price", carPrice);
                    cmd.Parameters.AddWithValue("@user_notes", (object?)dto.UserNotes ?? DBNull.Value);

                    orderId = (int)await cmd.ExecuteScalarAsync();
                }

                string insertBuyOrder = @"
            INSERT INTO Buy_Orders (order_id)
            VALUES (@order_id);";

                using (var cmd = new SqlCommand(insertBuyOrder, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@order_id", orderId);

                    await cmd.ExecuteNonQueryAsync();
                }

                if (documentUrls != null && documentUrls.Any())
                {
                    string insertDocQuery = @"
                INSERT INTO Order_Documents (order_id, document_url)
                VALUES (@order_id, @document_url);";

                    foreach (var url in documentUrls)
                    {
                        using var cmdDoc = new SqlCommand(insertDocQuery, conn, transaction);
                        cmdDoc.Parameters.AddWithValue("@order_id", orderId);
                        cmdDoc.Parameters.AddWithValue("@document_url", url);
                        await cmdDoc.ExecuteNonQueryAsync();
                    }
                }

                await transaction.CommitAsync();
                return orderId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // Installment Order
        public async Task<int> AddInstallmentOrderAsync(InstallmentOrderCreateDto dto, int userId, List<string> documentUrls)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                await ValidateCarAvailabilityAsync(dto.CarId, OrderType.Installment);

                string getPriceQuery = "SELECT price FROM Cars WHERE car_id = @car_id;";
                decimal carPrice = 0;

                using (var cmdPrice = new SqlCommand(getPriceQuery, conn, transaction))
                {
                    cmdPrice.Parameters.AddWithValue("@car_id", dto.CarId);
                    var result = await cmdPrice.ExecuteScalarAsync();

                    if (result == null || result == DBNull.Value)
                    {
                        throw new InvalidOperationException("Car not found or price is invalid.");
                    }

                    carPrice = Convert.ToDecimal(result);
                }

                if (dto.InstallmentMonths <= 0)
                {
                    throw new ArgumentException("Installment months must be greater than zero.");
                }

                decimal monthlyPayment = Math.Round(carPrice / dto.InstallmentMonths, 2);

                string insertBaseOrder = @"
            INSERT INTO Orders (car_id, user_id, order_type, order_status, total_price, created_at)
            OUTPUT INSERTED.order_id
            VALUES (@car_id, @user_id, @order_type, @order_status, @total_price, GETDATE());";

                int orderId;
                using (var cmd = new SqlCommand(insertBaseOrder, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@car_id", dto.CarId);
                    cmd.Parameters.AddWithValue("@user_id", userId);
                    cmd.Parameters.AddWithValue("@order_type", (int)OrderType.Installment);
                    cmd.Parameters.AddWithValue("@order_status", (int)OrderStatus.Pending);
                    cmd.Parameters.AddWithValue("@total_price", carPrice);

                    orderId = (int)await cmd.ExecuteScalarAsync();
                }

                string insertInstallmentOrder = @"
            INSERT INTO Installment_Orders (order_id, installment_months, monthly_payment)
            VALUES (@order_id, @installment_months, @monthly_payment);";

                using (var cmd = new SqlCommand(insertInstallmentOrder, conn, transaction))
                {
                    cmd.Parameters.AddWithValue("@order_id", orderId);
                    cmd.Parameters.AddWithValue("@installment_months", dto.InstallmentMonths);
                    cmd.Parameters.AddWithValue("@monthly_payment", monthlyPayment);

                    await cmd.ExecuteNonQueryAsync();
                }

                if (documentUrls != null && documentUrls.Any())
                {
                    string insertDocQuery = @"
                INSERT INTO Order_Documents (order_id, document_url)
                VALUES (@order_id, @document_url);";

                    foreach (var url in documentUrls)
                    {
                        using var cmdDoc = new SqlCommand(insertDocQuery, conn, transaction);
                        cmdDoc.Parameters.AddWithValue("@order_id", orderId);
                        cmdDoc.Parameters.AddWithValue("@document_url", url);
                        await cmdDoc.ExecuteNonQueryAsync();
                    }
                }

                await transaction.CommitAsync();
                return orderId;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<RentOrderDetailsDto?> FetchRentSpecificDetailsAsync(SqlConnection conn, int orderId)
        {
            string query = "SELECT start_date, end_date FROM Rent_Orders WHERE order_id = @order_id;";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@order_id", orderId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new RentOrderDetailsDto
                {
                    StartDate = reader.GetDateTime(reader.GetOrdinal("start_date")),
                    EndDate = reader.GetDateTime(reader.GetOrdinal("end_date"))
                };
            }
            return null;
        }
        private async Task<List<string>> FetchOrderDocumentsAsync(SqlConnection conn, int orderId)
        {
            var docs = new List<string>();
            string query = "SELECT document_url FROM Order_Documents WHERE order_id = @order_id;";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@order_id", orderId);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                docs.Add(reader.GetString(0));
            }
            return docs;
        }
        private async Task<InstallmentOrderSummaryDto?> FetchInstallmentSummaryDetailsAsync(SqlConnection conn, int orderId)
        {
            string query = "SELECT installment_months, monthly_payment FROM Installment_Orders WHERE order_id = @order_id;";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@order_id", orderId);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new InstallmentOrderSummaryDto
                {
                    InstallmentMonths = reader.GetInt32(reader.GetOrdinal("installment_months")),
                    MonthlyPayment = reader.GetDecimal(reader.GetOrdinal("monthly_payment"))
                };
            }
            return null;
        }
        public async Task ValidateCarAvailabilityAsync(int carId, OrderType orderType, DateTime? startDate = null, DateTime? endDate = null)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            string checkCarQuery = "SELECT status FROM Cars WHERE car_id = @car_id;";
            using (var cmdCar = new SqlCommand(checkCarQuery, conn))
            {
                cmdCar.Parameters.AddWithValue("@car_id", carId);
                var result = await cmdCar.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                {
                    throw new InvalidOperationException("Car not found in the system.");
                }

                int carStatus = Convert.ToInt32(result);

                if (carStatus == (int)CarStatus.Sold)
                {
                    throw new InvalidOperationException("Car is already sold and unavailable for any new orders.");
                }

                if (carStatus == (int)CarStatus.Pending)
                {
                    throw new InvalidOperationException("Car listing is pending admin approval and cannot accept orders yet.");
                }
            }

            if (orderType == OrderType.Rent)
            {
                if (!startDate.HasValue || !endDate.HasValue)
                {
                    throw new ArgumentException("Start date and End date are required for rent availability check.");
                }

                if (startDate >= endDate || startDate < DateTime.UtcNow.Date)
                {
                    throw new ArgumentException("Invalid date range specified.");
                }

                string checkOverlapQuery = @"
            SELECT COUNT(1)
            FROM Rent_Orders ro
            INNER JOIN Orders o ON ro.order_id = o.order_id
            WHERE o.car_id = @car_id
              AND o.order_status = @approved_status
              AND ro.start_date < @end_date
              AND ro.end_date > @start_date;";

                using var cmdRent = new SqlCommand(checkOverlapQuery, conn);
                cmdRent.Parameters.AddWithValue("@car_id", carId);
                cmdRent.Parameters.AddWithValue("@approved_status", (int)OrderStatus.Approved);
                cmdRent.Parameters.AddWithValue("@start_date", startDate.Value);
                cmdRent.Parameters.AddWithValue("@end_date", endDate.Value);

                int overlapCount = Convert.ToInt32(await cmdRent.ExecuteScalarAsync());
                if (overlapCount > 0)
                {
                    throw new InvalidOperationException("Car is already rented for the selected date range.");
                }
            }
        }
    }
}
