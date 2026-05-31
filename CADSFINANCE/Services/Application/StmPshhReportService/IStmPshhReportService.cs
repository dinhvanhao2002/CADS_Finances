using CADSFINANCE.Services.Application.StmPshhReportService.DTOs;

namespace CADSFINANCE.Services.Application.StmPshhReportService;

public interface IStmPshhReportService
{
    Task<StmPshhReportResponseDto> GetListAsync(
        StmPshhReportQueryDto query,
        CancellationToken cancellationToken = default);

    Task<ReportDataTableResponseDto> GetDataTableAsync(
        StmPshhReportQueryDto query,
        CancellationToken cancellationToken = default);
}
