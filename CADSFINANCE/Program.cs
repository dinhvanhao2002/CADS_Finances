using CADSFINANCE.Extensions;
using CADSFINANCE.Services.Application.ReportInfrastructure;
using DevExpress.AspNetCore;
using DevExpress.AspNetCore.Reporting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.OpenApi.Models;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy("CadsNextjs", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "http://127.0.0.1:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllersWithViews();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplicationServices();
builder.Services.AddReportInfrastructure();
builder.Services.AddHttpClient("SwaggerAuth");
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "CADSFINANCE.SwaggerAuth";
        options.LoginPath = "/swagger-login";
        options.LogoutPath = "/swagger-logout";
        options.AccessDeniedPath = "/swagger-login";
        options.SlidingExpiration = true;
    });
builder.Services.AddDevExpressControls();
builder.Services.AddScoped<DevExpress.XtraReports.Web.Extensions.ReportStorageWebExtension, DevExpressReportStorage>();
builder.Services.ConfigureReportingServices(configurator =>
{
    if (builder.Environment.IsDevelopment())
    {
        configurator.UseDevelopmentMode();
    }

    configurator.UseAsyncEngine();
    configurator.ConfigureWebDocumentViewer(viewerConfigurator =>
    {
        viewerConfigurator.UseCachedReportSourceBuilder();
    });
});
builder.Services.AddHealthChecks();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Token returned by the desktop login API.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }
        ] = Array.Empty<string>()
    });
});

var app = builder.Build();

app.UseCors("CadsNextjs");
app.UseDevExpressControls();
app.UseStaticFiles();
app.UseAuthentication();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() ||
    builder.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.Use(async (context, next) =>
    {
        if (IsSwaggerRequest(context.Request) &&
            context.User.Identity?.IsAuthenticated != true)
        {
            var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
            context.Response.Redirect($"/swagger-login?returnUrl={WebUtility.UrlEncode(returnUrl)}");
            return;
        }

        await next();
    });

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.InjectJavascript("/swagger-auth.js");
    });
}

app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.MapControllers();
app.MapHealthChecks("/healthz");

app.Run();

static bool IsSwaggerRequest(HttpRequest request)
{
    return request.Path.StartsWithSegments("/swagger") &&
        !request.Path.StartsWithSegments("/swagger-login") &&
        !request.Path.StartsWithSegments("/swagger-logout") &&
        !request.Path.StartsWithSegments("/swagger-auth-token") &&
        !request.Path.StartsWithSegments("/swagger-auth.js");
}
