using CADSFINANCE.Services.Application.ReportInfrastructure.DTOs;

namespace CADSFINANCE.Services.Application.ReportInfrastructure;

public interface IReportLayoutStore
{
    Task<ReportLayoutRefDto> GetLayoutRefAsync(
        string companyCode,
        string reportCode,
        CancellationToken cancellationToken = default);

    Task<ReportLayoutDocumentDto> GetLayoutAsync(
        string companyCode,
        string reportCode,
        CancellationToken cancellationToken = default);
}
