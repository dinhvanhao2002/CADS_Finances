using System.Data;
using System.Text.Json;
using CADSFINANCE.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CADSFINANCE.Controllers;

[ApiController]
[Route("api/stm-pshh")]
public sealed class StmPshhController : ControllerBase
{
    private const string TableName = "dbo.STM_PSHH";
    private readonly CadsFinanceDbContext _dbContext;

    public StmPshhController(CadsFinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Dictionary<string, object?>>>> GetList(
        [FromQuery] string? q,
        [FromQuery] int take = 100)
    {
        take = Math.Clamp(take, 1, 500);

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT TOP (@Take) *
            FROM {TableName}
            WHERE @Q IS NULL
               OR ID_PS LIKE @LikeQ
               OR SO_HD LIKE @LikeQ
               OR SO_PHIEU LIKE @LikeQ
               OR MA_DT LIKE @LikeQ
               OR DIEN_GIAI LIKE @LikeQ
            ORDER BY NGAY_CT DESC, ID_PS DESC
            """;
        command.Parameters.Add("@Take", SqlDbType.Int).Value = take;
        command.Parameters.Add("@Q", SqlDbType.NVarChar, 250).Value = string.IsNullOrWhiteSpace(q) ? DBNull.Value : q;
        command.Parameters.Add("@LikeQ", SqlDbType.NVarChar, 260).Value = string.IsNullOrWhiteSpace(q) ? DBNull.Value : $"%{q}%";

        var rows = await ReadRowsAsync(command);
        return Ok(rows);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Dictionary<string, object?>>> GetById(string id)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT * FROM {TableName} WHERE ID_PS = @ID_PS";
        command.Parameters.Add("@ID_PS", SqlDbType.NVarChar, 250).Value = id;

        var rows = await ReadRowsAsync(command);
        return rows.Count == 0 ? NotFound() : Ok(rows[0]);
    }

    [HttpPost]
    public async Task<ActionResult> Create([FromBody] Dictionary<string, JsonElement> payload)
    {
        var columns = await GetColumnMapAsync();
        var values = BuildValues(payload, columns, includeKey: true);

        if (!values.ContainsKey("ID_PS") || values["ID_PS"] is null)
        {
            values["ID_PS"] = $"web_{DateTime.Now:yyyyMMddHHmmssffff}";
        }

        var columnSql = string.Join(", ", values.Keys.Select(QuoteColumn));
        var parameterSql = string.Join(", ", values.Keys.Select(c => $"@{SafeParameterName(c)}"));

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"INSERT INTO {TableName} ({columnSql}) VALUES ({parameterSql})";
        AddParameters(command, values);

        await command.ExecuteNonQueryAsync();
        return CreatedAtAction(nameof(GetById), new { id = values["ID_PS"] }, new { id = values["ID_PS"] });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Dictionary<string, JsonElement> payload)
    {
        var columns = await GetColumnMapAsync();
        var values = BuildValues(payload, columns, includeKey: false);

        if (values.Count == 0)
        {
            return BadRequest("No valid STM_PSHH columns were provided.");
        }

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            UPDATE {TableName}
            SET {string.Join(", ", values.Keys.Select(c => $"{QuoteColumn(c)} = @{SafeParameterName(c)}"))}
            WHERE ID_PS = @ID_PS
            """;
        AddParameters(command, values);
        command.Parameters.Add("@ID_PS", SqlDbType.NVarChar, 250).Value = id;

        var affected = await command.ExecuteNonQueryAsync();
        return affected == 0 ? NotFound() : NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"DELETE FROM {TableName} WHERE ID_PS = @ID_PS";
        command.Parameters.Add("@ID_PS", SqlDbType.NVarChar, 250).Value = id;

        var affected = await command.ExecuteNonQueryAsync();
        return affected == 0 ? NotFound() : NoContent();
    }

    private async Task<IReadOnlyDictionary<string, string>> GetColumnMapAsync()
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COLUMN_NAME, DATA_TYPE
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'STM_PSHH'
            """;

        var columns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columns[reader.GetString(0)] = reader.GetString(1);
        }

        return columns;
    }

    private static Dictionary<string, object?> BuildValues(
        Dictionary<string, JsonElement> payload,
        IReadOnlyDictionary<string, string> columns,
        bool includeKey)
    {
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        foreach (var (key, element) in payload)
        {
            if (!columns.TryGetValue(key, out var dataType) || (!includeKey && key.Equals("ID_PS", StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            values[key] = ConvertJsonValue(element, dataType);
        }

        return values;
    }

    private static object? ConvertJsonValue(JsonElement element, string dataType)
    {
        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        if (element.ValueKind == JsonValueKind.String && string.IsNullOrWhiteSpace(element.GetString()))
        {
            return null;
        }

        return dataType switch
        {
            "int" => element.ValueKind == JsonValueKind.Number ? element.GetInt32() : int.Parse(element.GetString()!),
            "float" => element.ValueKind == JsonValueKind.Number ? element.GetDouble() : double.Parse(element.GetString()!),
            "bit" => element.ValueKind == JsonValueKind.True || (element.ValueKind == JsonValueKind.String && bool.Parse(element.GetString()!)),
            "datetime" => element.ValueKind == JsonValueKind.String ? DateTime.Parse(element.GetString()!) : element.GetDateTime(),
            _ => element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString()
        };
    }

    private static async Task<List<Dictionary<string, object?>>> ReadRowsAsync(SqlCommand command)
    {
        var rows = new List<Dictionary<string, object?>>();

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            rows.Add(row);
        }

        return rows;
    }

    private static void AddParameters(SqlCommand command, Dictionary<string, object?> values)
    {
        foreach (var (key, value) in values)
        {
            command.Parameters.AddWithValue($"@{SafeParameterName(key)}", value ?? DBNull.Value);
        }
    }

    private static string QuoteColumn(string columnName) => $"[{columnName.Replace("]", "]]")}]";

    private static string SafeParameterName(string columnName) =>
        columnName.Replace("/", "_").Replace(" ", "_").Replace("-", "_");

    private SqlConnection CreateConnection()
    {
        return new SqlConnection(_dbContext.Database.GetDbConnection().ConnectionString);
    }
}
