# STM_PSHH report flow

## Preview

1. CadsNextJs calls `GET /api/reports/stm-pshh/preview`.
2. ASP.NET Core resolves the layout reference by company:
   `reports/companies/{companyCode}/stm-pshh/v1/layout.repx`.
3. ASP.NET Core also returns `layoutRef.downloadUrl` for local/dev preview:
   `/api/reports/stm-pshh/layout?companyCode={companyCode}`.
4. ASP.NET Core queries `dbo.STM_PSHH` and returns a paged DataTable payload.
5. CadsNextJs downloads `.repx` from S3/MinIO using `layoutRef.key`, or uses `layoutRef.downloadUrl` in development.
6. CadsNextJs binds `dataSource.rows` to DevExpress viewer using:
   `dataSetName = ThongSoKT`, `tableName = STM_PSHH`.

## Export

1. CadsNextJs calls `POST /api/reports/stm-pshh/exports`.
2. API creates a job and puts it into the internal export queue.
3. Background worker loads the layout ref, gets report data, and calls `IReportDocumentRenderer`.
4. Current renderer writes a JSON scaffold under `wwwroot/companies/.../exports/...`.
5. Replace `LocalReportDocumentRenderer` with a DevExpress renderer to produce real PDF/XLSX and upload it to MinIO.

## Internal gRPC boundary

Use `CADSFINANCE/Protos/report_data_service.proto` when splitting services:

- `ReportService`: layout, DevExpress render, preview/export orchestration.
- `ReportDataService`: SQL/data access and DataTable response.

The HTTP contract can stay unchanged while the service-to-service call moves to gRPC.
