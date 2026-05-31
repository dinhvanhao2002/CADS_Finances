using CADSFINANCE.Services.Application.StmPshhReportService;

namespace CADSFINANCE.Services.Application.ReportInfrastructure;

public sealed class ReportExportWorker : BackgroundService
{
    private readonly IReportExportQueue _queue;
    private readonly IReportExportJobStore _jobStore;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReportExportWorker> _logger;

    public ReportExportWorker(
        IReportExportQueue queue,
        IReportExportJobStore jobStore,
        IServiceScopeFactory scopeFactory,
        ILogger<ReportExportWorker> logger)
    {
        _queue = queue;
        _jobStore = jobStore;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await _queue.DequeueAsync(stoppingToken);

            try
            {
                _jobStore.MarkRunning(workItem.JobId);

                using var scope = _scopeFactory.CreateScope();
                var reportService = scope.ServiceProvider.GetRequiredService<IStmPshhReportService>();
                var layoutStore = scope.ServiceProvider.GetRequiredService<IReportLayoutStore>();
                var renderer = scope.ServiceProvider.GetRequiredService<IReportDocumentRenderer>();

                var layout = await layoutStore.GetLayoutAsync(workItem.CompanyCode, workItem.ReportCode, stoppingToken);
                var dataSource = await reportService.GetDataTableAsync(workItem.Query, stoppingToken);
                var result = await renderer.RenderAsync(layout, dataSource, workItem.Format, stoppingToken);

                _jobStore.MarkCompleted(workItem.JobId, result.Bucket, result.Key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export report job {JobId}", workItem.JobId);
                _jobStore.MarkFailed(workItem.JobId, ex.Message);
            }
        }
    }
}
