using System.Text.Json;
using CADSFINANCE.Services.Application.ReportInfrastructure.DTOs;
using CADSFINANCE.Services.Application.StmPshhReportService.DTOs;

namespace CADSFINANCE.Services.Application.ReportInfrastructure;

public sealed class LocalReportDocumentRenderer : IReportDocumentRenderer
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public LocalReportDocumentRenderer(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    public async Task<(string Bucket, string Key)> RenderAsync(
        ReportLayoutDocumentDto layout,
        ReportDataTableResponseDto dataSource,
        ReportExportFormat format,
        CancellationToken cancellationToken = default)
    {
        var bucket = _configuration["Reports:ExportBucket"] ?? "report-exports";
        var layoutRef = layout.Ref;
        var key = $"companies/{layoutRef.CompanyCode}/{layoutRef.ReportCode}/exports/{DateTime.UtcNow:yyyy/MM/dd}/{Guid.NewGuid():N}.{format.ToString().ToLowerInvariant()}.json";
        var filePath = Path.Combine(_environment.ContentRootPath, "wwwroot", key.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        var payload = new
        {
            Renderer = "Local scaffold. Replace IReportDocumentRenderer with DevExpress renderer for real PDF/XLSX.",
            Format = format.ToString(),
            Layout = layoutRef,
            LayoutSource = layout.Source,
            LayoutBytes = layout.Content.Length,
            DataSource = dataSource
        };

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, payload, cancellationToken: cancellationToken);

        return (bucket, key);
    }
}
