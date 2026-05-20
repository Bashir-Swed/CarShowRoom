using CarShowRoom.DAL.DTOs;
using CarShowRoom.DAL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarShowRoom.DAL.Repositories
{
    public class AuthRepository
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public AuthRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<int> RegisterAsync(User user)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_RegisterUser", connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@FullName", user.FullName);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@Password", user.Password);
            command.Parameters.AddWithValue("@Phone", (object)user.Phone ?? DBNull.Value);
            command.Parameters.AddWithValue("@Role", user.Role);
            command.Parameters.AddWithValue("@Address", (object)user.Address ?? DBNull.Value);

            await connection.OpenAsync();

            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetUserByEmail", connection);

            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@Email", email);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserId = reader.GetInt32("user_id"),
                    FullName = reader.GetString("full_name"),
                    Email = reader.GetString("email"),
                    Password = reader.GetString("password"),
                    Role = reader.GetString("role")
                };
            }

            return null;
        }

        public async Task<bool> UpdateProfileAsync(int userId, string fullName, string? phone, string? address)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_UpdateUserProfile", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@FullName", fullName);
            command.Parameters.AddWithValue("@Phone", (object?)phone ?? DBNull.Value);
            command.Parameters.AddWithValue("@Address", (object?)address ?? DBNull.Value);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result) > 0;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("SELECT * FROM Users WHERE user_id = @UserId", connection);
            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    UserId = (int)reader["user_id"],
                    Password = reader["password"].ToString()!
                };
            }
            return null;
        }
        public async Task<bool> UpdatePasswordAsync(int userId, string newHashedPassword)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_UpdatePassword", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@NewPassword", newHashedPassword);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }
        public async Task<bool> DeleteAccountAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_DeleteUser", connection);
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result) > 0;
        }

        public async Task<UserProfileDto?> GetUserProfileAsync(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            using var command = new SqlCommand("sp_GetUserProfile", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@UserId", userId);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new UserProfileDto
                {
                    UserId = (int)reader["user_id"],
                    FullName = reader["full_name"].ToString()!,
                    Email = reader["email"].ToString()!,
                    Phone = reader["phone"]?.ToString(),
                    Address = reader["address"]?.ToString(),
                    Role = reader["role"].ToString()!,
                    CreatedAt = (DateTime)reader["created_at"]
                };
            }
            return null;
        }


    }
}
