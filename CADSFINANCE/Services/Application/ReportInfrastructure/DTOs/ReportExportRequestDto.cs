namespace CADSFINANCE.Services.Application.ReportInfrastructure.DTOs;

public sealed class ReportExportRequestDto
{
    public ReportExportFormat Format { get; set; } = ReportExportFormat.Pdf;
}
