using System.Data;
using CADSFINANCE.Infrastructure.Persistence;
using CADSFINANCE.Services.Application.StmPshhReportService.DTOs;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CADSFINANCE.Services.Application.StmPshhReportService;

public sealed class StmPshhReportService : IStmPshhReportService
{
    private const string DataSetName = "ThongSoKT";
    private const string TableName = "STM_PSHH";
    private const string LayoutKey = "reports/stm-pshh.repx";
    private readonly CadsFinanceDbContext _dbContext;

    public StmPshhReportService(CadsFinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StmPshhReportResponseDto> GetListAsync(
        StmPshhReportQueryDto query,
        CancellationToken cancellationToken = default)
    {
        query.Skip = Math.Max(query.Skip, 0);
        query.Take = Math.Clamp(query.Take, 1, 500);

        await using var connection = new SqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        var total = await CountAsync(connection, query, cancellationToken);
        var items = await GetItemsAsync(connection, query, cancellationToken);

        return new StmPshhReportResponseDto
        {
            Total = total,
            Skip = query.Skip,
            Take = query.Take,
            Items = items
        };
    }

    public async Task<ReportDataTableResponseDto> GetDataTableAsync(
        StmPshhReportQueryDto query,
        CancellationToken cancellationToken = default)
    {
        query.Skip = Math.Max(query.Skip, 0);
        query.Take = Math.Clamp(query.Take, 1, 500);

        await using var connection = new SqlConnection(GetConnectionString());
        await connection.OpenAsync(cancellationToken);

        var total = await CountAsync(connection, query, cancellationToken);
        var dataTable = await GetReportDataTableAsync(connection, query, cancellationToken);

        return ToDataTableResponse(dataTable, total, query);
    }

    private static async Task<int> CountAsync(
        SqlConnection connection,
        StmPshhReportQueryDto query,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT COUNT(1)
            FROM dbo.STM_PSHH
            {BuildWhereSql()}
            """;
        AddFilterParameters(command, query);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    private string GetConnectionString()
    {
        return _dbContext.Database.GetDbConnection().ConnectionString;
    }

    private static async Task<IReadOnlyList<StmPshhReportItemDto>> GetItemsAsync(
        SqlConnection connection,
        StmPshhReportQueryDto query,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT
                ID_PS,
                SO_HD,
                SO_PHIEU,
                MA_DT,
                NGAY_CT,
                MA_LOAI_CT,
                DIEN_GIAI,
                TIEN,
                TIEN_NT,
                TIEN_VAT
            FROM dbo.STM_PSHH
            {BuildWhereSql()}
            ORDER BY NGAY_CT DESC, ID_PS DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;
        AddFilterParameters(command, query);
        command.Parameters.Add("@Skip", SqlDbType.Int).Value = query.Skip;
        command.Parameters.Add("@Take", SqlDbType.Int).Value = query.Take;

        var items = new List<StmPshhReportItemDto>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new StmPshhReportItemDto
            {
                IdPs = GetNullableString(reader, "ID_PS"),
                SoHd = GetNullableString(reader, "SO_HD"),
                SoPhieu = GetNullableString(reader, "SO_PHIEU"),
                MaDt = GetNullableString(reader, "MA_DT"),
                NgayCt = GetNullableDateTime(reader, "NGAY_CT"),
                MaLoaiCt = GetNullableString(reader, "MA_LOAI_CT"),
                DienGiai = GetNullableString(reader, "DIEN_GIAI"),
                Tien = GetNullableDecimal(reader, "TIEN"),
                TienNt = GetNullableDecimal(reader, "TIEN_NT"),
                TienVat = GetNullableDecimal(reader, "TIEN_VAT")
            });
        }

        return items;
    }

    private static async Task<DataTable> GetReportDataTableAsync(
        SqlConnection connection,
        StmPshhReportQueryDto query,
        CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = $"""
            SELECT
                ID_PS,
                SO_HD,
                SO_PHIEU,
                MA_DT,
                NGAY_CT,
                MA_LOAI_CT,
                DIEN_GIAI,
                TIEN,
                TIEN_NT,
                TIEN_VAT
            FROM dbo.STM_PSHH
            {BuildWhereSql()}
            ORDER BY NGAY_CT DESC, ID_PS DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
            """;
        AddFilterParameters(command, query);
        command.Parameters.Add("@Skip", SqlDbType.Int).Value = query.Skip;
        command.Parameters.Add("@Take", SqlDbType.Int).Value = query.Take;

        var dataTable = CreateReportDataTable();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            dataTable.Rows.Add(
                GetValueOrDbNull(reader, "ID_PS"),
                GetValueOrDbNull(reader, "SO_HD"),
                GetValueOrDbNull(reader, "SO_PHIEU"),
                GetValueOrDbNull(reader, "MA_DT"),
                GetValueOrDbNull(reader, "NGAY_CT"),
                GetValueOrDbNull(reader, "MA_LOAI_CT"),
                GetValueOrDbNull(reader, "DIEN_GIAI"),
                GetValueOrDbNull(reader, "TIEN"),
                GetValueOrDbNull(reader, "TIEN_NT"),
                GetValueOrDbNull(reader, "TIEN_VAT"));
        }

        return dataTable;
    }

    private static DataTable CreateReportDataTable()
    {
        var dataTable = new DataTable(TableName)
        {
            Locale = System.Globalization.CultureInfo.InvariantCulture
        };

        dataTable.Columns.Add("ID_PS", typeof(string));
        dataTable.Columns.Add("SO_HD", typeof(string));
        dataTable.Columns.Add("SO_PHIEU", typeof(string));
        dataTable.Columns.Add("MA_DT", typeof(string));
        dataTable.Columns.Add("NGAY_CT", typeof(DateTime));
        dataTable.Columns.Add("MA_LOAI_CT", typeof(string));
        dataTable.Columns.Add("DIEN_GIAI", typeof(string));
        dataTable.Columns.Add("TIEN", typeof(decimal));
        dataTable.Columns.Add("TIEN_NT", typeof(decimal));
        dataTable.Columns.Add("TIEN_VAT", typeof(decimal));

        return dataTable;
    }

    private static ReportDataTableResponseDto ToDataTableResponse(
        DataTable dataTable,
        int total,
        StmPshhReportQueryDto query)
    {
        var columns = dataTable.Columns
            .Cast<DataColumn>()
            .Select(column => new ReportDataTableColumnDto
            {
                Name = column.ColumnName,
                DataType = column.DataType.Name
            })
            .ToList();

        var rows = dataTable.Rows
            .Cast<DataRow>()
            .Select(row => dataTable.Columns
                .Cast<DataColumn>()
                .ToDictionary<DataColumn, string, object?>(
                    column => column.ColumnName,
                    column => row.IsNull(column) ? null : row[column],
                    StringComparer.OrdinalIgnoreCase))
            .ToList();

        return new ReportDataTableResponseDto
        {
            DataSetName = DataSetName,
            TableName = dataTable.TableName,
            LayoutKey = LayoutKey,
            Total = total,
            Skip = query.Skip,
            Take = query.Take,
            Columns = columns,
            Rows = rows
        };
    }

    private static string BuildWhereSql()
    {
        return """
            WHERE (@Q IS NULL
                OR ID_PS LIKE @LikeQ
                OR SO_HD LIKE @LikeQ
                OR SO_PHIEU LIKE @LikeQ
                OR MA_DT LIKE @LikeQ
                OR DIEN_GIAI LIKE @LikeQ)
              AND (@FromDate IS NULL OR NGAY_CT >= @FromDate)
              AND (@ToDateExclusive IS NULL OR NGAY_CT < @ToDateExclusive)
              AND (@MaDt IS NULL OR MA_DT = @MaDt)
              AND (@MaLoaiCt IS NULL OR MA_LOAI_CT = @MaLoaiCt)
            """;
    }

    private static void AddFilterParameters(SqlCommand command, StmPshhReportQueryDto query)
    {
        var q = string.IsNullOrWhiteSpace(query.Q) ? null : query.Q.Trim();
        var maDt = string.IsNullOrWhiteSpace(query.MaDt) ? null : query.MaDt.Trim();
        var maLoaiCt = string.IsNullOrWhiteSpace(query.MaLoaiCt) ? null : query.MaLoaiCt.Trim();
        var toDateExclusive = query.ToDate?.Date.AddDays(1);

        command.Parameters.Add("@Q", SqlDbType.NVarChar, 250).Value = q is null ? DBNull.Value : q;
        command.Parameters.Add("@LikeQ", SqlDbType.NVarChar, 260).Value = q is null ? DBNull.Value : $"%{q}%";
        command.Parameters.Add("@FromDate", SqlDbType.DateTime).Value = query.FromDate?.Date is null ? DBNull.Value : query.FromDate.Value.Date;
        command.Parameters.Add("@ToDateExclusive", SqlDbType.DateTime).Value = toDateExclusive is null ? DBNull.Value : toDateExclusive.Value;
        command.Parameters.Add("@MaDt", SqlDbType.NVarChar, 250).Value = maDt is null ? DBNull.Value : maDt;
        command.Parameters.Add("@MaLoaiCt", SqlDbType.NVarChar, 250).Value = maLoaiCt is null ? DBNull.Value : maLoaiCt;
    }

    private static string? GetNullableString(SqlDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static DateTime? GetNullableDateTime(SqlDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }

    private static decimal? GetNullableDecimal(SqlDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? null : Convert.ToDecimal(reader.GetValue(ordinal));
    }

    private static object GetValueOrDbNull(SqlDataReader reader, string name)
    {
        var ordinal = reader.GetOrdinal(name);
        return reader.IsDBNull(ordinal) ? DBNull.Value : reader.GetValue(ordinal);
    }
}
