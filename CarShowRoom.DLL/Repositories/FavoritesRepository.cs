using CarShowRoom.DAL.Models.CarShowRoom.DAL.Models.CarShowRoom.DAL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace CarShowRoom.DAL.Repositories
{
    public class FavoritesRepository
    {
        private readonly string _connectionString;
        private readonly CarRepository _carRepo;

        public FavoritesRepository(IConfiguration configuration, CarRepository carRepo)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
            _carRepo = carRepo;
        }

        public async Task<string> ToggleFavoriteAsync(int userId, int carId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_ToggleFavorite", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@user_id", userId);
            cmd.Parameters.AddWithValue("@car_id", carId);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString() ?? "Error";
        }

        /*public async Task<List<Car>> GetUserFavoritesAsync(int userId)
        {
            var cars = new List<Car>();
            using var conn = new SqlConnection(_connectionString);

            string sql = @"SELECT c.*, ci.image_url 
                       FROM Favorites f
                       JOIN Cars c ON f.car_id = c.car_id
                       LEFT JOIN Car_Images ci ON c.car_id = ci.car_id
                       WHERE f.user_id = @userId";

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
                    var car = _carRepo.MapToCar(reader);
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
        }*/
        public async Task<List<Car>> GetUserFavoritesAsync(int userId)
        {
            string additionalWhere = @"
        AND c.car_id IN (
            SELECT car_id 
            FROM Favorites 
            WHERE user_id = @userId
        )";

            return await _carRepo.QueryCarsAsync(additionalWhere, cmd =>
            {
                cmd.Parameters.AddWithValue("@userId", userId);
            });
        }
    }
}
