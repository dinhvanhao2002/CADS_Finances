namespace CADSFINANCE.Services.Application.StmPshhReportService.DTOs;

public sealed class StmPshhReportItemDto
{
    public string? IdPs { get; set; }

    public string? SoHd { get; set; }

    public string? SoPhieu { get; set; }

    public string? MaDt { get; set; }

    public DateTime? NgayCt { get; set; }

    public string? MaLoaiCt { get; set; }

    public string? DienGiai { get; set; }

    public decimal? Tien { get; set; }

    public decimal? TienNt { get; set; }

    public decimal? TienVat { get; set; }
}
