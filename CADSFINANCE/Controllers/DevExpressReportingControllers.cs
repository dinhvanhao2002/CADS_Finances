using DevExpress.AspNetCore.Reporting.QueryBuilder;
using DevExpress.AspNetCore.Reporting.QueryBuilder.Native.Services;
using DevExpress.AspNetCore.Reporting.ReportDesigner;
using DevExpress.AspNetCore.Reporting.ReportDesigner.Native.Services;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer;
using DevExpress.AspNetCore.Reporting.WebDocumentViewer.Native.Services;
using DevExpress.XtraReports.Web.ClientControls;
using DevExpress.XtraReports.Web.ReportDesigner;
using DevExpress.XtraReports.Web.ReportDesigner.Services;
using Microsoft.AspNetCore.Mvc;

namespace CADSFINANCE.Controllers;

[Route("DXXRDV")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class CustomWebDocumentViewerController : WebDocumentViewerController
{
    public CustomWebDocumentViewerController(IWebDocumentViewerMvcControllerService controllerService)
        : base(controllerService)
    {
    }
}

[Route("DXXRD")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class CustomReportDesignerController : ReportDesignerController
{
    public CustomReportDesignerController(IReportDesignerMvcControllerService controllerService)
        : base(controllerService)
    {
    }

    [HttpPost("[action]")]
    public IActionResult GetDesignerModel(
        [FromForm] string reportUrl,
        [FromServices] IReportDesignerModelBuilder designerModelBuilder,
        [FromForm] ReportDesignerSettingsBase designerModelSettings)
    {
        var designerModel = designerModelBuilder
            .Report(reportUrl)
            .BuildModel();

        designerModel.Assign(designerModelSettings);

        var clientSideModelSettings = new ClientSideModelSettings
        {
            IncludeLocalization = false,
            IncludeCldrData = false,
            IncludeCldrSupplemental = false
        };

        return DesignerModel(designerModel, clientSideModelSettings);
    }
}

[Route("DXXQB")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class CustomQueryBuilderController : QueryBuilderController
{
    public CustomQueryBuilderController(IQueryBuilderMvcControllerService controllerService)
        : base(controllerService)
    {
    }
}
