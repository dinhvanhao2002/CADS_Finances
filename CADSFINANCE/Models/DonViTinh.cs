namespace CADSFINANCE.Models;

public sealed class DonViTinh
{
    public string MaDvt { get; set; } = string.Empty;

    public string? TenDvt { get; set; }

    public int? UserId { get; set; }

    public double? QuyCach { get; set; }

    public bool? IsActive { get; set; }
}
