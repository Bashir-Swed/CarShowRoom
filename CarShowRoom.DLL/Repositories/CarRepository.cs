using CarShowRoom.DAL.Models;
using CarShowRoom.DAL.Models.CarShowRoom.DAL.Models.CarShowRoom.DAL.Models;
using Microsoft.Data.SqlClient;
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

            string sql = @"SELECT c.*, ci.image_url 
                   FROM Cars c 
                   LEFT JOIN Car_Images ci ON c.car_id = ci.car_id 
                   WHERE c.is_approved = 1";

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
                Brand = reader["brand"].ToString()!,
                Model = reader["model"].ToString()!,
                Year = (int)reader["year"],
                Price = (decimal)reader["price"],
                FuelType = reader["fuel_type"]?.ToString(),
                GearType = reader["gear_type"]?.ToString(),
                Mileage = (int)reader["mileage"],
                IsApproved = (bool)reader["is_approved"],
                RentPricePerDay = reader["rent_price_per_day"] as decimal?,
                Status = reader["status"].ToString()!,
                CreatedAt = (DateTime)reader["created_at"]
            };
        }

        public async Task<bool> AddCarUsingSPAsync(Car car)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_AddCarWithImages", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            string imagesCombined = string.Join(",", car.ImageUrls);

            cmd.Parameters.AddWithValue("@user_id", car.UserId);
            cmd.Parameters.AddWithValue("@brand", car.Brand);
            cmd.Parameters.AddWithValue("@model", car.Model);
            cmd.Parameters.AddWithValue("@year", car.Year);
            cmd.Parameters.AddWithValue("@color", (object?)car.Color ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@price", car.Price);
            cmd.Parameters.AddWithValue("@fuel_type", (object?)car.FuelType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@gear_type", (object?)car.GearType ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@mileage", car.Mileage);
            cmd.Parameters.AddWithValue("@description", (object?)car.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@image_urls", imagesCombined);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result != null;
        }

        public async Task<bool> ApproveCarAsync(int carId, int employeeId, string notes)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_ApproveCar", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@car_id", carId);
            cmd.Parameters.AddWithValue("@approved_by", employeeId);
            cmd.Parameters.AddWithValue("@notes", (object?)notes ?? DBNull.Value);

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
        public async Task<List<Car>> GetPendingCarsAsync()
        {
            var cars = new List<Car>();
            using var conn = new SqlConnection(_connectionString);

            string sql = @"SELECT c.*, ci.image_url 
                   FROM Cars c 
                   LEFT JOIN Car_Images ci ON c.car_id = ci.car_id 
                   WHERE c.is_approved = 0";

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

        public async Task<bool> UpdateCarAsync(Car car)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_UpdateCar", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@car_id", car.CarId);
            cmd.Parameters.AddWithValue("@user_id", car.UserId);
            cmd.Parameters.AddWithValue("@brand", car.Brand);
            cmd.Parameters.AddWithValue("@model", car.Model);
            cmd.Parameters.AddWithValue("@year", car.Year);
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

        public async Task<List<Car>> SearchCarsAsync(string? brand, string? model, decimal? minPrice, decimal? maxPrice, int? year, string? fuelType, string? gearType)
        {
            var cars = new List<Car>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_SearchCars", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@brand", (object?)brand ?? DBNull.Value);
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

            string sql = @"SELECT c.*, ci.image_url 
                   FROM Cars c 
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

    }
}