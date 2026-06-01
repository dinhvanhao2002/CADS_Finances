using CADSFINANCE.Infrastructure.Persistence;
using CADSFINANCE.Services.Application.DonViTinhService;
using CADSFINANCE.Services.Application.ReportInfrastructure;
using CADSFINANCE.Services.Application.StmPshhReportService;
using CADSFINANCE.Services.Application.WebSockets;
using Microsoft.EntityFrameworkCore;

namespace CADSFINANCE.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection");

        services.AddDbContext<CadsFinanceDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IDonViTinhService, DonViTinhService>();
        services.AddScoped<IStmPshhReportService, StmPshhReportService>();
        services.AddSingleton<IWebSocketConnectionManager, WebSocketConnectionManager>();
        services.AddSingleton<IWebSocketMessagePublisher, WebSocketMessagePublisher>();

        return services;
    }

    public static IServiceCollection AddReportInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IReportLayoutStore, MinioReportLayoutStore>();
        services.AddScoped<IReportDocumentRenderer, LocalReportDocumentRenderer>();
        services.AddSingleton<IReportExportJobStore, InMemoryReportExportJobStore>();
        services.AddSingleton<IReportExportQueue, InMemoryReportExportQueue>();
        services.AddHostedService<ReportExportWorker>();

        return services;
    }
}
