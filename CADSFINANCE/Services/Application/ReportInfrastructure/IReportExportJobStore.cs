using CADSFINANCE.Services.Application.ReportInfrastructure.DTOs;

namespace CADSFINANCE.Services.Application.ReportInfrastructure;

public interface IReportExportJobStore
{
    ReportExportJobDto Create(string reportCode, string companyCode, ReportExportFormat format);

    ReportExportJobDto? Get(string jobId);

    void MarkRunning(string jobId);

    void MarkCompleted(string jobId, string resultBucket, string resultKey);

    void MarkFailed(string jobId, string error);
}
