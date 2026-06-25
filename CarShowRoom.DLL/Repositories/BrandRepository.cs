using CarShowRoom.DAL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
public class BrandRepository
{
    private readonly string _connectionString;

    public BrandRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }
    public async Task<List<Brand>> GetAllBrandsAsync()
    {
        var brands = new List<Brand>();
        using var conn = new SqlConnection(_connectionString);
        string sql = "SELECT brand_id, name FROM Brands ORDER BY name ASC";

        using var cmd = new SqlCommand(sql, conn);
        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            brands.Add(new Brand
            {
                BrandId = (int)reader["brand_id"],
                Name = reader["name"].ToString() ?? ""
            });
        }
        return brands;
    }

    public async Task<bool> DeleteBrandAsync(int brandId)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_DeleteBrand", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("@brand_id", brandId);

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

    public async Task<Brand?> AddBrandAsync(string brandName)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_AddBrand", conn);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@name", brandName);

        await conn.OpenAsync();
        try
        {
            var result = await cmd.ExecuteScalarAsync();
            if (result != null)
            {
                return new Brand
                {
                    BrandId = Convert.ToInt32(result),
                    Name = brandName
                };
            }
            return null;
        }
        catch (SqlException ex)
        {
            throw new Exception(ex.Message);
        }
    }
}