using CADSFINANCE.Services.Application.ReportInfrastructure;
using CADSFINANCE.Services.Application.ReportInfrastructure.DTOs;
using CADSFINANCE.Services.Application.StmPshhReportService;
using CADSFINANCE.Services.Application.StmPshhReportService.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CADSFINANCE.Controllers.Reports;

[ApiController]
[Route("api/reports/stm-pshh")]
[Route("api/reports/stm_pshh")]
public sealed class StmPshhReportController : ControllerBase
{
    private const string ReportCode = "stm-pshh";
    private readonly IStmPshhReportService _service;
    private readonly IReportLayoutStore _layoutStore;
    private readonly IReportExportJobStore _exportJobStore;
    private readonly IReportExportQueue _exportQueue;

    public StmPshhReportController(
        IStmPshhReportService service,
        IReportLayoutStore layoutStore,
        IReportExportJobStore exportJobStore,
        IReportExportQueue exportQueue)
    {
        _service = service;
        _layoutStore = layoutStore;
        _exportJobStore = exportJobStore;
        _exportQueue = exportQueue;
    }

    [HttpGet]
    public async Task<ActionResult<StmPshhReportResponseDto>> GetList(
        [FromQuery] StmPshhReportQueryDto query,
        CancellationToken cancellationToken)
    {
        var report = await _service.GetListAsync(query, cancellationToken);
        return Ok(report);
    }

    [HttpGet("datatable")]
    public async Task<ActionResult<ReportDataTableResponseDto>> GetDataTable(
        [FromQuery] StmPshhReportQueryDto query,
        CancellationToken cancellationToken)
    {
        var report = await _service.GetDataTableAsync(query, cancellationToken);
        return Ok(report);
    }

    [HttpGet("preview")]
    public async Task<ActionResult<StmPshhReportPreviewDto>> Preview(
        [FromQuery] string companyCode,
        [FromQuery] int pageNumber,
        [FromQuery] int pageSize,
        [FromQuery] StmPshhReportQueryDto query,
        CancellationToken cancellationToken)
    {
        companyCode = string.IsNullOrWhiteSpace(companyCode) ? "default" : companyCode;
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize <= 0 ? query.Take : pageSize, 1, 500);

        query.Skip = (pageNumber - 1) * pageSize;
        query.Take = pageSize;

        var layoutRef = await _layoutStore.GetLayoutRefAsync(companyCode, ReportCode, cancellationToken);
        var dataSource = await _service.GetDataTableAsync(query, cancellationToken);

        return Ok(new StmPshhReportPreviewDto
        {
            ReportCode = ReportCode,
            CompanyCode = layoutRef.CompanyCode,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalRows = dataSource.Total,
            TotalPages = (int)Math.Ceiling(dataSource.Total / (double)pageSize),
            LayoutRef = layoutRef,
            DataSource = dataSource
        });
    }



    [HttpPost("exports")]
    public async Task<ActionResult<ReportExportJobDto>> StartExport(
        [FromQuery] string companyCode,
        [FromQuery] StmPshhReportQueryDto query,
        [FromBody] ReportExportRequestDto request,
        CancellationToken cancellationToken)
    {
        companyCode = string.IsNullOrWhiteSpace(companyCode) ? "default" : companyCode;
        query.Skip = 0;
        query.Take = Math.Clamp(query.Take <= 0 ? 500 : query.Take, 1, 500);

        var job = _exportJobStore.Create(ReportCode, companyCode, request.Format);
        await _exportQueue.EnqueueAsync(new ReportExportWorkItem
        {
            JobId = job.JobId,
            ReportCode = ReportCode,
            CompanyCode = companyCode,
            Format = request.Format,
            Query = query
        }, cancellationToken);

        return AcceptedAtAction(nameof(GetExportJob), new { jobId = job.JobId }, job);
    }

    [HttpGet("exports/{jobId}")]
    public ActionResult<ReportExportJobDto> GetExportJob(string jobId)
    {
        var job = _exportJobStore.Get(jobId);
        return job is null ? NotFound() : Ok(job);
    }
}
