namespace CADSFINANCE.Models;

public sealed class SwaggerLoginViewModel
{
    public string Gmail { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string Taxcode { get; set; } = string.Empty;

    public string Uuid { get; set; } = string.Empty;

    public string ReturnUrl { get; set; } = "/swagger";

    public string? Error { get; set; }
}
