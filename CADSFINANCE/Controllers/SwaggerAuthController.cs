using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using CADSFINANCE.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CADSFINANCE.Controllers;

[AllowAnonymous]
public sealed class SwaggerAuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public SwaggerAuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpGet("swagger-login")]
    public IActionResult Login([FromQuery] SwaggerLoginViewModel model)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Redirect(GetSafeReturnUrl(model.ReturnUrl));
        }

        model.ReturnUrl = GetSafeReturnUrl(model.ReturnUrl);
        model.Password = string.Empty;

        return View(model);
    }

    [HttpPost("swagger-login")]
    public async Task<IActionResult> LoginPost([FromForm] SwaggerLoginViewModel model, CancellationToken cancellationToken)
    {
        model.ReturnUrl = GetSafeReturnUrl(model.ReturnUrl);
        model.Uuid = string.IsNullOrWhiteSpace(model.Uuid) ? model.Gmail : model.Uuid;

        if (string.IsNullOrWhiteSpace(model.Gmail) ||
            string.IsNullOrWhiteSpace(model.Password) ||
            string.IsNullOrWhiteSpace(model.Taxcode))
        {
            model.Password = string.Empty;
            model.Error = "Vui long nhap day du gmail, password va taxcode.";
            return View("Login", model);
        }

        var loginEndpoint = _configuration["SwaggerAuth:LoginEndpoint"]
            ?? "https://api-system-k8s.1business.vn/api/v0/user/login-desktop/";
        var platform = _configuration["SwaggerAuth:Platform"] ?? "client-desktop";

        using var request = new HttpRequestMessage(HttpMethod.Post, loginEndpoint);
        request.Headers.TryAddWithoutValidation("platform", platform);
        request.Content = new StringContent(
            JsonSerializer.Serialize(new
            {
                gmail = model.Gmail,
                password = model.Password,
                taxcode = model.Taxcode,
                uuid = model.Uuid
            }),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClientFactory
            .CreateClient("SwaggerAuth")
            .SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            model.Password = string.Empty;
            model.Error = $"Dang nhap that bai ({(int)response.StatusCode}).";
            return View("Login", model);
        }

        var token = TryExtractToken(responseBody);
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, model.Gmail),
            new("taxcode", model.Taxcode)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        return Content(BuildLoginSuccessHtml(token, model.ReturnUrl), "text/html; charset=utf-8");
    }

    [HttpGet("swagger-logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Content("""
            <!doctype html>
            <html lang="vi">
            <head><meta charset="utf-8"><script>localStorage.removeItem('CADSFINANCE_SWAGGER_TOKEN'); location.href='/swagger-login';</script></head>
            <body></body>
            </html>
            """, "text/html; charset=utf-8");
    }

    [HttpGet("swagger-auth-token")]
    public IActionResult GetAuthToken()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Unauthorized();
        }

        var token = User.FindFirstValue("access_token");
        return Ok(new { token });
    }

    private static string BuildLoginSuccessHtml(string? token, string returnUrl)
    {
        var encodedToken = JavaScriptEncoder.Default.Encode(token ?? string.Empty);
        var encodedReturnUrl = JavaScriptEncoder.Default.Encode(GetSafeReturnUrl(returnUrl));

        return $$"""
            <!doctype html>
            <html lang="vi">
            <head>
                <meta charset="utf-8">
                <title>Dang nhap Swagger</title>
                <script>
                    const token = "{{encodedToken}}";
                    if (token) {
                        localStorage.setItem("CADSFINANCE_SWAGGER_TOKEN", token);
                    }
                    location.replace("{{encodedReturnUrl}}");
                </script>
            </head>
            <body></body>
            </html>
            """;
    }

    private static string GetSafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl) || !returnUrl.StartsWith('/'))
        {
            return "/swagger";
        }

        return returnUrl.StartsWith("//", StringComparison.Ordinal) ? "/swagger" : returnUrl;
    }

    private static string? TryExtractToken(string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            return FindToken(document.RootElement);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? FindToken(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (property.Value.ValueKind == JsonValueKind.String &&
                    IsTokenProperty(property.Name))
                {
                    return property.Value.GetString();
                }

                var nestedToken = FindToken(property.Value);
                if (!string.IsNullOrWhiteSpace(nestedToken))
                {
                    return nestedToken;
                }
            }
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                var nestedToken = FindToken(item);
                if (!string.IsNullOrWhiteSpace(nestedToken))
                {
                    return nestedToken;
                }
            }
        }

        return null;
    }

    private static bool IsTokenProperty(string name)
    {
        return name.Equals("access_token", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("accessToken", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("token", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("jwt", StringComparison.OrdinalIgnoreCase);
    }
}
