using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.Web.Extensions;

namespace CADSFINANCE.Services.Application.ReportInfrastructure;

public sealed class DevExpressReportStorage : ReportStorageWebExtension
{
    private readonly IWebHostEnvironment _environment;

    public DevExpressReportStorage(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public override bool CanSetData(string url)
    {
        return IsValidUrl(url);
    }

    public override byte[] GetData(string url)
    {
        return File.ReadAllBytes(ResolveReportPath(url));
    }

    public override Dictionary<string, string> GetUrls()
    {
        return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Report1"] = "Report1",
            ["StmPshhReport"] = "STM_PSHH"
        };
    }

    public override bool IsValidUrl(string url)
    {
        var reportName = NormalizeReportName(url);
        return reportName is "Report1" or "StmPshhReport" or "stm-pshh" or "stm_pshh";
    }

    public override void SetData(XtraReport report, string url)
    {
        if (!IsValidUrl(url))
        {
            throw new ArgumentException($"Invalid report url: {url}", nameof(url));
        }

        using var stream = File.Create(ResolveReportPath(url));
        report.SaveLayoutToXml(stream);
    }

    public override string SetNewData(XtraReport report, string defaultUrl)
    {
        var url = string.IsNullOrWhiteSpace(defaultUrl) ? "Report1" : NormalizeReportName(defaultUrl);
        SetData(report, url);
        return url;
    }

    private string ResolveReportPath(string url)
    {
        var reportName = NormalizeReportName(url);
        var webRoot = ResolveWebRootPath();

        if (reportName is "StmPshhReport" or "stm-pshh" or "stm_pshh")
        {
            var companyLayout = Path.Combine(webRoot, "companies", "cads", "stm-pshh", "v1", "layout.repx");
            if (File.Exists(companyLayout))
            {
                return companyLayout;
            }
        }

        var report1 = Path.Combine(webRoot, "Report1 (1).repx");
        if (File.Exists(report1))
        {
            return report1;
        }

        var simpleReport1 = Path.Combine(webRoot, "Report1.repx");
        if (File.Exists(simpleReport1))
        {
            return simpleReport1;
        }

        throw new FileNotFoundException($"Report layout file was not found for url '{url}'.");
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

    private static string NormalizeReportName(string url)
    {
        var reportName = string.IsNullOrWhiteSpace(url) ? "Report1" : url.Trim();
        var queryIndex = reportName.IndexOf('?', StringComparison.Ordinal);
        return queryIndex >= 0 ? reportName[..queryIndex] : reportName;
    }
}
