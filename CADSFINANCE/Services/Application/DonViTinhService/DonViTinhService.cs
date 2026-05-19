using CADSFINANCE.Services.Application.DonViTinhService.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CADSFINANCE.Services.Application.DonViTinhService;

public sealed class DonViTinhService : IDonViTinhService
{
    private readonly string _connectionString;

    public DonViTinhService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");
    }

    public async Task<IReadOnlyList<DonViTinhDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT MA_DVT, TEN_DVT, USER_ID, QUY_CACH, isActive
            FROM dbo.LST_DonViTinh
            ORDER BY MA_DVT
            """;

        var items = new List<DonViTinhDto>();
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(Map(reader));
        }

        return items;
    }

    public async Task<DonViTinhDto?> GetByIdAsync(string maDvt, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT MA_DVT, TEN_DVT, USER_ID, QUY_CACH, isActive
            FROM dbo.LST_DonViTinh
            WHERE MA_DVT = @MA_DVT
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@MA_DVT", maDvt);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<(bool Success, string? Error)> CreateAsync(DonViTinhDto dto, CancellationToken cancellationToken = default)
    {
        var validationError = Validate(dto);
        if (validationError is not null)
        {
            return (false, validationError);
        }

        if (await ExistsAsync(dto.MaDvt.Trim(), cancellationToken))
        {
            return (false, $"MaDvt '{dto.MaDvt}' already exists.");
        }

        const string sql = """
            INSERT INTO dbo.LST_DonViTinh (MA_DVT, TEN_DVT, USER_ID, QUY_CACH, isActive)
            VALUES (@MA_DVT, @TEN_DVT, @USER_ID, @QUY_CACH, @isActive)
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, dto);
        await command.ExecuteNonQueryAsync(cancellationToken);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(string maDvt, DonViTinhDto dto, CancellationToken cancellationToken = default)
    {
        dto.MaDvt = maDvt;

        var validationError = Validate(dto);
        if (validationError is not null)
        {
            return (false, validationError);
        }

        const string sql = """
            UPDATE dbo.LST_DonViTinh
            SET TEN_DVT = @TEN_DVT,
                USER_ID = @USER_ID,
                QUY_CACH = @QUY_CACH,
                isActive = @isActive
            WHERE MA_DVT = @MA_DVT
            """;

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        AddParameters(command, dto);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0 ? (true, null) : (false, $"MaDvt '{maDvt}' was not found.");
    }

    public async Task<bool> DeleteAsync(string maDvt, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM dbo.LST_DonViTinh WHERE MA_DVT = @MA_DVT";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@MA_DVT", maDvt);

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        return affectedRows > 0;
    }

    private async Task<bool> ExistsAsync(string maDvt, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT COUNT(1) FROM dbo.LST_DonViTinh WHERE MA_DVT = @MA_DVT";

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@MA_DVT", maDvt);

        var count = (int)await command.ExecuteScalarAsync(cancellationToken);
        return count > 0;
    }

    private static string? Validate(DonViTinhDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.MaDvt))
        {
            return "MaDvt is required.";
        }

        if (dto.MaDvt.Length > 10)
        {
            return "MaDvt must be 10 characters or less.";
        }

        if (dto.TenDvt?.Length > 50)
        {
            return "TenDvt must be 50 characters or less.";
        }

        return null;
    }

    private static DonViTinhDto Map(SqlDataReader reader)
    {
        return new DonViTinhDto
        {
            MaDvt = reader.GetString(reader.GetOrdinal("MA_DVT")),
            TenDvt = reader.IsDBNull(reader.GetOrdinal("TEN_DVT")) ? null : reader.GetString(reader.GetOrdinal("TEN_DVT")),
            UserId = reader.IsDBNull(reader.GetOrdinal("USER_ID")) ? null : reader.GetInt32(reader.GetOrdinal("USER_ID")),
            QuyCach = reader.IsDBNull(reader.GetOrdinal("QUY_CACH")) ? null : reader.GetDouble(reader.GetOrdinal("QUY_CACH")),
            IsActive = reader.IsDBNull(reader.GetOrdinal("isActive")) ? null : reader.GetBoolean(reader.GetOrdinal("isActive"))
        };
    }

    private static void AddParameters(SqlCommand command, DonViTinhDto dto)
    {
        command.Parameters.AddWithValue("@MA_DVT", dto.MaDvt.Trim());
        command.Parameters.AddWithValue("@TEN_DVT", (object?)dto.TenDvt ?? DBNull.Value);
        command.Parameters.AddWithValue("@USER_ID", (object?)dto.UserId ?? DBNull.Value);
        command.Parameters.AddWithValue("@QUY_CACH", (object?)dto.QuyCach ?? DBNull.Value);
        command.Parameters.AddWithValue("@isActive", (object?)dto.IsActive ?? DBNull.Value);
    }
}
