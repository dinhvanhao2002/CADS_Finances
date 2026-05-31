using CADSFINANCE.Services.Application.ReportInfrastructure.DTOs;
using CADSFINANCE.Services.Application.StmPshhReportService.DTOs;

namespace CADSFINANCE.Services.Application.ReportInfrastructure;

public sealed class ReportExportWorkItem
{
    public required string JobId { get; set; }

    public required string ReportCode { get; set; }

    public required string CompanyCode { get; set; }

    public ReportExportFormat Format { get; set; }

    public StmPshhReportQueryDto Query { get; set; } = new();
}
