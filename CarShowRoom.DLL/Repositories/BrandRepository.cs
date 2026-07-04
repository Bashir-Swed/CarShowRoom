using CarShowRoom.DAL.Models;
using CarShowRoom.DAL.DTOs;
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
        string sql = "SELECT brand_id, name ,brand_logo_url,created_at FROM Brands ORDER BY name ASC";

        using var cmd = new SqlCommand(sql, conn);
        await conn.OpenAsync();
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            brands.Add(new Brand
            {
                BrandId = (int)reader["brand_id"],
                Name = reader["name"].ToString() ?? "",
                BrandLogoUrl = reader["brand_logo_url"] != DBNull.Value ? reader["brand_logo_url"].ToString() : null,
                CreatedAt = reader["created_at"] != DBNull.Value ? (DateTime)reader["created_at"] : DateTime.MinValue
            });
        }
        return brands;
    }

    public async Task<Brand> GetBrandByIDAsync(int id)
    {
        using var conn = new SqlConnection(_connectionString);
        string sql = "SELECT name ,brand_logo_url,created_at FROM Brands where brand_id=@brand_id";
        using var cmd = new SqlCommand(sql, conn); await conn.OpenAsync();
        cmd.Parameters.AddWithValue("@brand_id", id);
        using var reader = await cmd.ExecuteReaderAsync();
        if ( await reader.ReadAsync())
        {
            return new Brand
            {
                BrandId = id,
                Name = reader["name"].ToString() ?? "",
                BrandLogoUrl = reader["brand_logo_url"] != DBNull.Value ? reader["brand_logo_url"].ToString() : null,
                CreatedAt = reader["created_at"] != DBNull.Value ? (DateTime)reader["created_at"] : DateTime.MinValue

            };
        }
        return null;

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

    public async Task<Brand?> AddBrandAsync(BrandAddDto brand)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_AddBrand", conn);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@name", brand.Name);
        cmd.Parameters.AddWithValue("@brand_logo_url", (object?)brand.BrandLogoUrl ?? DBNull.Value);

        await conn.OpenAsync();
        try
        {
            var result = await cmd.ExecuteScalarAsync();
            if (result != null)
            {
                return new Brand
                {
                    BrandId = Convert.ToInt32(result),
                    Name = brand.Name,
                };
            }
            return null;
        }
        catch (SqlException ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public async Task<bool> UpdateBrandAsync(int brandId, string brandName, string? imageUrl)
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("sp_UpdateBrand", conn);
        cmd.CommandType = CommandType.StoredProcedure;

        cmd.Parameters.AddWithValue("@brand_id", brandId);
        cmd.Parameters.AddWithValue("@brand_name", brandName);

        cmd.Parameters.AddWithValue("@image_url", (object?)imageUrl ?? DBNull.Value);

        await conn.OpenAsync();
        int rowsAffected = await cmd.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }

    public async Task<string?> GetBrandImageUrlAsync(int brandId)
    {
        using var conn = new SqlConnection(_connectionString);

        using var cmd = new SqlCommand("SELECT brand_logo_url FROM Brands WHERE brand_id = @brand_id", conn);
        cmd.Parameters.AddWithValue("@brand_id", brandId);

        await conn.OpenAsync();

        var result = await cmd.ExecuteScalarAsync();

        return result != DBNull.Value ? result?.ToString() : null;
    }
}