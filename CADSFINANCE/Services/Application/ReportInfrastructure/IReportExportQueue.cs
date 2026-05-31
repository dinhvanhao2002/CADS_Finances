namespace CADSFINANCE.Services.Application.ReportInfrastructure;

public interface IReportExportQueue
{
    ValueTask EnqueueAsync(ReportExportWorkItem workItem, CancellationToken cancellationToken = default);

    ValueTask<ReportExportWorkItem> DequeueAsync(CancellationToken cancellationToken);
}
