namespace CADSFINANCE.Services.Application.ReportInfrastructure.DTOs;

public sealed class ReportExportJobDto
{
    public required string JobId { get; set; }

    public required string ReportCode { get; set; }

    public required string CompanyCode { get; set; }

    public ReportExportFormat Format { get; set; }

    public ReportExportJobStatus Status { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? StartedAtUtc { get; set; }

    public DateTime? FinishedAtUtc { get; set; }

    public string? ResultBucket { get; set; }

    public string? ResultKey { get; set; }

    public string? Error { get; set; }
}
