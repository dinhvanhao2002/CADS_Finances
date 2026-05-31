namespace CADSFINANCE.Models;

public sealed class StmPshh
{
    public string IdPs { get; set; } = string.Empty;

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
