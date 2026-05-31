namespace CADSFINANCE.Services.Application.ReportInfrastructure.DTOs;

public sealed class ReportLayoutRefDto
{
    public required string Bucket { get; set; }

    public required string Key { get; set; }

    public required string ReportCode { get; set; }

    public required string CompanyCode { get; set; }

    public string? Version { get; set; }

    public string? ContentType { get; set; }

    public string? DownloadUrl { get; set; }
}
