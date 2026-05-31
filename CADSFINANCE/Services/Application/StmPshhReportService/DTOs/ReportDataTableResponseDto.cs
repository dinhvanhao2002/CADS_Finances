namespace CADSFINANCE.Services.Application.StmPshhReportService.DTOs;

public sealed class ReportDataTableResponseDto
{
    public required string DataSetName { get; set; }

    public required string TableName { get; set; }

    public string? LayoutKey { get; set; }

    public int Total { get; set; }

    public int Skip { get; set; }

    public int Take { get; set; }

    public IReadOnlyList<ReportDataTableColumnDto> Columns { get; set; } = [];

    public IReadOnlyList<IReadOnlyDictionary<string, object?>> Rows { get; set; } = [];
}
