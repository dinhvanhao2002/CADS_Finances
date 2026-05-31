namespace CADSFINANCE.Services.Application.ReportInfrastructure.DTOs;

public sealed class ReportLayoutDocumentDto
{
    public required ReportLayoutRefDto Ref { get; set; }

    public required byte[] Content { get; set; }

    public required string Source { get; set; }
}
