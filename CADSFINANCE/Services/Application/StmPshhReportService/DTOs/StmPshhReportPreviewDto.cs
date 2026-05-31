using CADSFINANCE.Services.Application.ReportInfrastructure.DTOs;

namespace CADSFINANCE.Services.Application.StmPshhReportService.DTOs;

public sealed class StmPshhReportPreviewDto
{
    public required string ReportCode { get; set; }

    public required string CompanyCode { get; set; }

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRows { get; set; }

    public int TotalPages { get; set; }

    public required ReportLayoutRefDto LayoutRef { get; set; }

    public required ReportDataTableResponseDto DataSource { get; set; }
}
