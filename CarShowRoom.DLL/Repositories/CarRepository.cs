using CarShowRoom.DAL.DTOs;
using CarShowRoom.DAL.Models;
using CarShowRoom.DAL.Models.CarShowRoom.DAL.Models.CarShowRoom.DAL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text;

namespace CarShowRoom.DAL.Repositories
{
    public class CarRepository
    {
        private readonly string _connectionString;

        public CarRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }
        public async Task<List<Car>> GetPublicCarsAsync()
        {
            return await QueryCarsAsync(
                @"
        AND c.approval_status = @approved
        AND c.effective_availability_status = @availability_status",
                cmd =>
                {
                    cmd.Parameters.AddWithValue(
                        "@approved",
                        (int)CarApprovalStatus.Approved
                    );

                    cmd.Parameters.AddWithValue(
                        "@availability_status",
                        (int)CarAvailabilityStatus.Available
                    );
                }
            );
        }
        public async Task<List<Car>> GetCarsForUserAsync(int userId,CarApprovalStatus? approvalStatus,CarAvailabilityStatus? availabilityStatus)
        {
            var where = new StringBuilder(@"
        AND c.user_id = @user_id");

            if (approvalStatus.HasValue)
            {
                where.Append(@"
            AND c.approval_status =
                @approval_status");
            }

            if (availabilityStatus.HasValue)
            {
                where.Append(@"
            AND c.effective_availability_status = @availability_status");
            }

            return await QueryCarsAsync(
                where.ToString(),
                cmd =>
                {
                    cmd.Parameters.AddWithValue(
                        "@user_id",
                        userId
                    );

                    if (approvalStatus.HasValue)
                    {
                        cmd.Parameters.AddWithValue(
                            "@approval_status",
                            (int)approvalStatus.Value
                        );
                    }

                    if (availabilityStatus.HasValue)
                    {
                        cmd.Parameters.AddWithValue(
                            "@availability_status",
                            (int)availabilityStatus.Value
                        );
                    }
                }
            );
        }
        public async Task<List<Car>> GetCarsForAdminAsync(CarApprovalStatus? approvalStatus,CarAvailabilityStatus? availabilityStatus,int? ownerId)
        {
            var where = new StringBuilder();

            if (approvalStatus.HasValue)
            {
                where.Append(@"
            AND c.approval_status =
                @approval_status");
            }

            if (availabilityStatus.HasValue)
            {
                where.Append(@"
            AND c.effective_availability_status = @availability_status");
            }

            if (ownerId.HasValue)
            {
                where.Append(@"
            AND c.user_id = @owner_id");
            }

            return await QueryCarsAsync(
                where.ToString(),
                cmd =>
                {
                    if (approvalStatus.HasValue)
                    {
                        cmd.Parameters.AddWithValue(
                            "@approval_status",
                            (int)approvalStatus.Value
                        );
                    }

                    if (availabilityStatus.HasValue)
                    {
                        cmd.Parameters.AddWithValue(
                            "@availability_status",
                            (int)availabilityStatus.Value
                        );
                    }

                    if (ownerId.HasValue)
                    {
                        cmd.Parameters.AddWithValue(
                            "@owner_id",
                            ownerId.Value
                        );
                    }
                }
            );
        }
        public async Task<Car?> GetCarByIdAsync(int carId)
        {
            var cars = await QueryCarsAsync(
                "AND c.car_id = @car_id",
                cmd => cmd.Parameters.AddWithValue(
                    "@car_id",
                    carId
                )
            );

            return cars.FirstOrDefault();
        }

        public Car MapToCar(SqlDataReader reader)
        {
            return new Car
            {
                CarId = (int)reader["car_id"],
                UserId = (int)reader["user_id"],
                BrandId = (int)reader["brand_id"], 
                Model = reader["model"].ToString()!,
                Year = (int)reader["year"],
                Price = (decimal)reader["price"],
                FuelType = reader["fuel_type"]?.ToString(),
                GearType = reader["gear_type"]?.ToString(),
                Mileage = (int)reader["mileage"],
                RentPricePerDay = reader["rent_price_per_day"] as decimal?,
                ApprovalStatus =(CarApprovalStatus)Convert.ToInt32(reader["approval_status"]),
                AvailabilityStatus =(CarAvailabilityStatus)Convert.ToInt32(reader["effective_availability_status"]),
                CreatedAt = (DateTime)reader["created_at"],
                ApprovedBy = reader["approved_by"] != DBNull.Value ? (int?)reader["approved_by"] : null,
                ApprovalNotes = reader.GetSchemaTable().Select("ColumnName = 'approval_notes'").Length > 0 && reader["approval_notes"] != DBNull.Value
                        ? reader["approval_notes"].ToString() : null,
                ApprovalDate = reader.GetSchemaTable().Select("ColumnName = 'approval_date'").Length > 0 && reader["approval_date"] != DBNull.Value
                        ? (DateTime?)reader["approval_date"] : null,

                Cylinders = reader["cylinders"] != DBNull.Value ? (int?)reader["cylinders"] : null,
                InteriorColor = reader["interior_color"] != DBNull.Value ? reader["interior_color"].ToString() : null,
                KeysCount = reader["keys_count"] != DBNull.Value ? (int?)reader["keys_count"] : null,
                DriveType = reader["drive_type"] != DBNull.Value ? reader["drive_type"].ToString() : null,
                Region = reader["region"] != DBNull.Value ? reader["region"].ToString() : null,
                Horsepower = reader["horsepower"] != DBNull.Value ? (int?)reader["horsepower"] : null,
                TopSpeed = reader["top_speed"] != DBNull.Value ? (int?)reader["top_speed"] : null,

                BrandLogoUrl = reader.GetSchemaTable().Select("ColumnName = 'brand_logo_url'").Length > 0 && reader["brand_logo_url"] != DBNull.Value
               ? reader["brand_logo_url"].ToString() : null,
            };
        }

        public async Task<Int32> AddCarUsingSPAsync(CarCreateDto car,int UserId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_AddCarWithImages", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            string imagesCombined = string.Join(",",car.ImageUrls ?? new List<string>());

            cmd.Parameters.AddWithValue("@user_id", UserId);
            cmd.Parameters.AddWithValue("@BrandId", car.BrandId);
            cmd.Parameters.AddWithValue("@model", car.Model);
            cmd.Parameters.AddWithValue("@year", car.Year);
            cmd.Parameters.AddWithValue("@color", (object?)car.Color ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@price", car.Price);
            cmd.Parameters.AddWithValue("@fuel_type", (object?)car.FuelType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@gear_type", (object?)car.GearType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mileage", car.Mileage);
            cmd.Parameters.AddWithValue("@rent_price_per_day", (object?)car.RentPricePerDay ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@description", (object?)car.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@image_urls", imagesCombined);

            cmd.Parameters.AddWithValue("@cylinders", (object?)car.Cylinders ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@interior_color", (object?)car.InteriorColor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@keys_count", (object?)car.KeysCount ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@drive_type", (object?)car.DriveType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@region", (object?)car.Region ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@horsepower", (object?)car.Horsepower ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@top_speed", (object?)car.TopSpeed ?? DBNull.Value);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            if (result != null && result != DBNull.Value)
            {
                return Convert.ToInt32(result);
            }
            return 0;
        }

        public async Task<string> ApproveCarAsync(int carId,int adminId,string notes)
        {
            using var conn =
                new SqlConnection(_connectionString);

            using var cmd =
                new SqlCommand("sp_ApproveCar", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue(
                "@car_id",
                carId
            );

            cmd.Parameters.AddWithValue(
                "@approved_by_admin_id",
                adminId
            );

            cmd.Parameters.AddWithValue(
                "@notes",
                string.IsNullOrWhiteSpace(notes)
                    ? DBNull.Value
                    : notes.Trim()
            );

            await conn.OpenAsync();

            var result = await cmd.ExecuteScalarAsync();

            return result?.ToString() ?? "Error";
        }

        public async Task<bool> DeleteCarAsync(int carId, int requestedByUserId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_DeleteCar", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@car_id", carId);
            cmd.Parameters.AddWithValue("@requested_by_user_id", requestedByUserId);

            await conn.OpenAsync();
            try
            {
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCarAsync(CarCreateDto car,int UserId,int CarId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_UpdateCar", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            string imagesCombined = string.Join(",",car.ImageUrls ?? new List<string>());

            cmd.Parameters.AddWithValue("@car_id", CarId);
            cmd.Parameters.AddWithValue("@user_id", UserId);
            cmd.Parameters.AddWithValue("@BrandId", car.BrandId);
            cmd.Parameters.AddWithValue("@model", car.Model);
            cmd.Parameters.AddWithValue("@year", car.Year);
            cmd.Parameters.AddWithValue("@image_urls", imagesCombined);
            cmd.Parameters.AddWithValue("@color", (object?)car.Color ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@price", car.Price);
            cmd.Parameters.AddWithValue("@fuel_type", (object?)car.FuelType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@gear_type", (object?)car.GearType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mileage", car.Mileage);
            cmd.Parameters.AddWithValue("@description", (object?)car.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@rent_price_per_day", (object?)car.RentPricePerDay ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cylinders",(object?)car.Cylinders ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@interior_color",(object?)car.InteriorColor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@keys_count",(object?)car.KeysCount ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@drive_type",(object?)car.DriveType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@region",(object?)car.Region ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@horsepower",(object?)car.Horsepower ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@top_speed",(object?)car.TopSpeed ?? DBNull.Value);

            await conn.OpenAsync();
            try
            {
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> AddImageToCarAsync(int carId, string imageUrl)
        {
            using var conn = new SqlConnection(_connectionString);
            string sql = "INSERT INTO Car_Images (car_id, image_url, uploaded_at) VALUES (@car_id, @url, GETDATE())";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@car_id", carId);
            cmd.Parameters.AddWithValue("@url", imageUrl);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteImageAsync(int imageId, int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_DeleteCarImage", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@image_id", imageId);
            cmd.Parameters.AddWithValue("@requested_by_user_id", userId);

            await conn.OpenAsync();
            try
            {
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Car>> SearchCarsAsync(int? brandId, string? model, decimal? minPrice, decimal? maxPrice, int? year, string? fuelType, string? gearType)
        {
            var cars = new List<Car>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_SearchCars", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@brandId", (object?)brandId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@model", (object?)model ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@minPrice", (object?)minPrice ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@maxPrice", (object?)maxPrice ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@year", (object?)year ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@fuelType", (object?)fuelType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@gearType", (object?)gearType ?? DBNull.Value);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int carId = (int)reader["car_id"];
                var existingCar = cars.FirstOrDefault(x => x.CarId == carId);

                if (existingCar == null)
                {
                    var car = MapToCar(reader);
                    if (reader["image_url"] != DBNull.Value)
                        car.ImageUrls.Add(reader["image_url"].ToString()!);
                    cars.Add(car);
                }
                else
                {
                    if (reader["image_url"] != DBNull.Value)
                        existingCar.ImageUrls.Add(reader["image_url"].ToString()!);
                }
            }
            return cars;
        }


        public async Task<List<string>> GetCarImagesAsync(int carId)
        {
            List<string> imageUrls = new List<string>();

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT image_url FROM Car_Images WHERE car_id = @car_id;", conn);

            cmd.Parameters.AddWithValue("@car_id", carId);

            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string url = reader.GetString(reader.GetOrdinal("image_url"));
                imageUrls.Add(url);
            }

            return imageUrls;
        }

        public async Task<string> RejectCarAsync( int carId,int adminId,string notes)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_RejectCar", conn);

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@car_id", carId);
            cmd.Parameters.AddWithValue("@rejected_by_admin_id", adminId);
            cmd.Parameters.AddWithValue("@notes", notes);

            await conn.OpenAsync();

            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString() ?? "Error";
        }
        private async Task<List<Car>> QueryCarsAsync(string additionalWhere,Action<SqlCommand>? configureCommand = null)
        {
            var cars = new Dictionary<int, Car>();

            const string baseQuery = @"
        SELECT
            c.*,
            b.name AS brand_name,
            b.brand_logo_url,
            ci.image_url
        FROM dbo.vw_CarsWithEffectiveAvailability c
        LEFT JOIN Brands b
            ON b.brand_id = c.brand_id
        LEFT JOIN Car_Images ci
            ON ci.car_id = c.car_id
        WHERE 1 = 1 ";

            string query =
                baseQuery +
                Environment.NewLine +
                additionalWhere +
                Environment.NewLine +
                @"
        ORDER BY
            c.created_at DESC,
            ci.uploaded_at ASC;";

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
                int carId = reader.GetInt32(
                    reader.GetOrdinal("car_id")
                );

                if (!cars.TryGetValue(carId, out var car))
                {
                    car = MapToCar(reader);
                    cars.Add(carId, car);
                }

                if (reader["image_url"] != DBNull.Value)
                {
                    string? imageUrl =
                        reader["image_url"].ToString();

                    if (!string.IsNullOrWhiteSpace(imageUrl) &&
                        !car.ImageUrls.Contains(imageUrl))
                    {
                        car.ImageUrls.Add(imageUrl);
                    }
                }
            }

            return cars.Values.ToList();
        }

    }
}