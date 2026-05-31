using CADSFINANCE.Services.Application.ReportInfrastructure.DTOs;
using CADSFINANCE.Services.Application.StmPshhReportService.DTOs;

namespace CADSFINANCE.Services.Application.ReportInfrastructure;

public interface IReportDocumentRenderer
{
    Task<(string Bucket, string Key)> RenderAsync(
        ReportLayoutDocumentDto layout,
        ReportDataTableResponseDto dataSource,
        ReportExportFormat format,
        CancellationToken cancellationToken = default);
}
