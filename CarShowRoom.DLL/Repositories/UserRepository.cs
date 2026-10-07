using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace CarShowRoom.DAL.Repositories
{
    public class UserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }
        public async Task<bool> DeleteUserAsync(int userId)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("sp_DeleteUserByAdmin", conn);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@user_id_to_delete", userId);

            await conn.OpenAsync();
            try
            {
                var result = await cmd.ExecuteScalarAsync();
                return result != null && Convert.ToInt32(result) == 1;
            }
            catch
            {
                return false;
            }
        }
    }
}
