namespace CADSFINANCE.Services.Application.StmPshhReportService.DTOs;

public sealed class StmPshhReportResponseDto
{
    public int Total { get; set; }

    public int Skip { get; set; }

    public int Take { get; set; }

    public IReadOnlyList<StmPshhReportItemDto> Items { get; set; } = [];
}
