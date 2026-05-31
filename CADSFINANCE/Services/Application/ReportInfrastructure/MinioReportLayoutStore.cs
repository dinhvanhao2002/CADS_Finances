using CADSFINANCE.Services.Application.ReportInfrastructure.DTOs;

namespace CADSFINANCE.Services.Application.ReportInfrastructure;

public sealed class MinioReportLayoutStore : IReportLayoutStore
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public MinioReportLayoutStore(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public Task<ReportLayoutRefDto> GetLayoutRefAsync(
        string companyCode,
        string reportCode,
        CancellationToken cancellationToken = default)
    {
        var normalizedCompanyCode = NormalizeSegment(companyCode);
        var normalizedReportCode = NormalizeSegment(reportCode);
        var bucket = _configuration["Reports:Bucket"] ?? "reports";
        var version = _configuration["Reports:DefaultVersion"] ?? "v1";

        return Task.FromResult(new ReportLayoutRefDto
        {
            Bucket = bucket,
            Key = $"companies/{normalizedCompanyCode}/{normalizedReportCode}/{version}/layout.repx",
            ReportCode = normalizedReportCode,
            CompanyCode = normalizedCompanyCode,
            Version = version,
            ContentType = "application/xml",
            DownloadUrl = $"/api/reports/{normalizedReportCode}/layout?companyCode={Uri.EscapeDataString(normalizedCompanyCode)}"
        });
    }

    public async Task<ReportLayoutDocumentDto> GetLayoutAsync(
        string companyCode,
        string reportCode,
        CancellationToken cancellationToken = default)
    {
        var layoutRef = await GetLayoutRefAsync(companyCode, reportCode, cancellationToken);
        var webRootPath = ResolveWebRootPath();
        var localPath = Path.Combine(webRootPath, layoutRef.Key.Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(localPath))
        {
            return new ReportLayoutDocumentDto
            {
                Ref = layoutRef,
                Content = await File.ReadAllBytesAsync(localPath, cancellationToken),
                Source = localPath
            };
        }

        var fallbackPath = Path.Combine(webRootPath, "Report1 (1).repx");
        if (File.Exists(fallbackPath))
        {
            return new ReportLayoutDocumentDto
            {
                Ref = layoutRef,
                Content = await File.ReadAllBytesAsync(fallbackPath, cancellationToken),
                Source = fallbackPath
            };
        }

        throw new FileNotFoundException($"Report layout was not found for {layoutRef.Key}.");
    }

    private static string NormalizeSegment(string value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "default"
            : value.Trim().ToLowerInvariant().Replace(" ", "-");
    }

    private string ResolveWebRootPath()
    {
        if (!string.IsNullOrWhiteSpace(_environment.WebRootPath) && Directory.Exists(_environment.WebRootPath))
        {
            return _environment.WebRootPath;
        }

        var projectWebRoot = Path.Combine(_environment.ContentRootPath, "wwwroot");
        if (Directory.Exists(projectWebRoot))
        {
            return projectWebRoot;
        }

        var solutionWebRoot = Path.Combine(_environment.ContentRootPath, "CADSFINANCE", "wwwroot");
        if (Directory.Exists(solutionWebRoot))
        {
            return solutionWebRoot;
        }

        return projectWebRoot;
    }
}
