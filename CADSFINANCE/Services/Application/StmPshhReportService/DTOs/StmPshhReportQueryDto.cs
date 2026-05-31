namespace CADSFINANCE.Services.Application.StmPshhReportService.DTOs;

public sealed class StmPshhReportQueryDto
{
    public string? Q { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public string? MaDt { get; set; }

    public string? MaLoaiCt { get; set; }

    public int Skip { get; set; }

    public int Take { get; set; } = 100;
}
