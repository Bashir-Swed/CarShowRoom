using CarShowRoom.DAL.Models;
using CarShowRoom.DAL.Models.CarShowRoom.DAL.Models.CarShowRoom.DAL.Models;
using Microsoft.Data.SqlClient;
using CarShowRoom.DAL.DTOs;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace CarShowRoom.DAL.Repositories
{
    public class CarRepository
    {
        private readonly string _connectionString;

        public CarRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<List<Car>> GetAllApprovedCarsAsync()
        {
            var cars = new List<Car>();
            using var conn = new SqlConnection(_connectionString);

            string sql = @"SELECT c.*, b.name AS brand_name,b.brand_logo_url ,ci.image_url 
                   FROM Cars c 
                   JOIN Brands b ON c.brand_id = b.brand_id
                   LEFT JOIN Car_Images ci ON c.car_id = ci.car_id 
                   WHERE c.is_approved = 1 and c.status = 2";

            using var cmd = new SqlCommand(sql, conn);
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
                IsApproved = (bool)reader["is_approved"],
                RentPricePerDay = reader["rent_price_per_day"] as decimal?,
                Status = (CarStatus)(int)reader["status"],
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

            string imagesCombined = string.Join(",", car.ImageUrls);

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
            cmd.Parameters.AddWithValue("@status",(int)CarStatus.Pending);
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

        public async Task<string> ApproveCarAsync(int carId, int employeeId, string notes)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_ApproveCar", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@car_id", carId);
            cmd.Parameters.AddWithValue("@approved_by_admin_id", employeeId);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString() ?? "Error";
        }
        public async Task<List<Car>> GetPendingCarsAsync()
        {
            var cars = new List<Car>();
            using var conn = new SqlConnection(_connectionString);

            string sql = @"SELECT c.*, b.name AS brand_name,b.brand_logo_url ,ci.image_url 
                   FROM Cars c 
                   JOIN Brands b ON c.brand_id = b.brand_id
                   LEFT JOIN Car_Images ci ON c.car_id = ci.car_id 
                   WHERE c.is_approved=0 and c.status = 1";

            using var cmd = new SqlCommand(sql, conn);
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
            string imagesCombined = string.Join(",", car.ImageUrls);

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

        public async Task<List<Car>> GetUserCarsAsync(int userId)
        {
            var cars = new List<Car>();
            using var conn = new SqlConnection(_connectionString);

            string sql = @"SELECT c.*, b.name AS brand_name,b.brand_logo_url ,ci.image_url 
                   FROM Cars c 
                   JOIN Brands b ON c.brand_id = b.brand_id
                   LEFT JOIN Car_Images ci ON c.car_id = ci.car_id 
                   WHERE c.user_id = @userId
                   ORDER BY c.created_at DESC";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@userId", userId);

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
                    {
                        car.ImageUrls.Add(reader["image_url"].ToString()!);
                    }
                    cars.Add(car);
                }
                else
                {
                    if (reader["image_url"] != DBNull.Value)
                    {
                        existingCar.ImageUrls.Add(reader["image_url"].ToString()!);
                    }
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

        public async Task<Car?> GetCarInfoOnlyByIdAsync(int carId)
        {
            string query = "SELECT * FROM Cars WHERE car_id = @car_id;";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);

            cmd.CommandType = CommandType.Text;
            cmd.Parameters.AddWithValue("@car_id", carId);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var car = MapToCar(reader);
                return car;
            }

            return null;
        }

    }
}