using System.Collections.Concurrent;
using CADSFINANCE.Services.Application.ReportInfrastructure.DTOs;

namespace CADSFINANCE.Services.Application.ReportInfrastructure;

public sealed class InMemoryReportExportJobStore : IReportExportJobStore
{
    private readonly ConcurrentDictionary<string, ReportExportJobDto> _jobs = new(StringComparer.OrdinalIgnoreCase);

    public ReportExportJobDto Create(string reportCode, string companyCode, ReportExportFormat format)
    {
        var job = new ReportExportJobDto
        {
            JobId = Guid.NewGuid().ToString("N"),
            ReportCode = reportCode,
            CompanyCode = companyCode,
            Format = format,
            Status = ReportExportJobStatus.Queued,
            CreatedAtUtc = DateTime.UtcNow
        };

        _jobs[job.JobId] = job;
        return job;
    }

    public ReportExportJobDto? Get(string jobId)
    {
        return _jobs.TryGetValue(jobId, out var job) ? job : null;
    }

    public void MarkRunning(string jobId)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            job.Status = ReportExportJobStatus.Running;
            job.StartedAtUtc = DateTime.UtcNow;
        }
    }

    public void MarkCompleted(string jobId, string resultBucket, string resultKey)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            job.Status = ReportExportJobStatus.Completed;
            job.ResultBucket = resultBucket;
            job.ResultKey = resultKey;
            job.FinishedAtUtc = DateTime.UtcNow;
        }
    }

    public void MarkFailed(string jobId, string error)
    {
        if (_jobs.TryGetValue(jobId, out var job))
        {
            job.Status = ReportExportJobStatus.Failed;
            job.Error = error;
            job.FinishedAtUtc = DateTime.UtcNow;
        }
    }
}
