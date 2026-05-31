# Huong dan tao mot man CRUD moi trong CADSFINANCE

Tai lieu nay mo ta cach them mot bang moi va tao day du API CRUD trong du an ASP.NET Core.

Vi du minh hoa: tao man CRUD cho bang `dbo.LST_TaiKhoanNganHang`.

## 1. Xac dinh bang va cot nghiep vu

Truoc khi code, can xac dinh:

- Ten bang trong SQL Server.
- Khoa chinh.
- Danh sach cot.
- Cot nao bat buoc, cot nao nullable.
- Do dai toi da cua chuoi.

Vi du:

```sql
CREATE TABLE dbo.LST_TaiKhoanNganHang
(
    MA_TK NVARCHAR(20) NOT NULL PRIMARY KEY,
    TEN_TK NVARCHAR(250) NULL,
    SO_TAI_KHOAN NVARCHAR(50) NULL,
    TEN_NGAN_HANG NVARCHAR(250) NULL,
    IS_ACTIVE BIT NULL
);
```

## 2. Tao entity trong `Models`

Tao file:

```text
CADSFINANCE/Models/TaiKhoanNganHang.cs
```

Noi dung mau:

```csharp
namespace CADSFINANCE.Models;

public sealed class TaiKhoanNganHang
{
    public string MaTk { get; set; } = string.Empty;

    public string? TenTk { get; set; }

    public string? SoTaiKhoan { get; set; }

    public string? TenNganHang { get; set; }

    public bool? IsActive { get; set; }
}
```

Quy uoc:

- Ten class dung PascalCase.
- Ten property dung PascalCase.
- Khong dat ten property theo kieu database nhu `MA_TK`, `TEN_TK`.
- Mapping ten cot database se khai bao trong `DbContext`.

## 3. Dang ky entity vao `CadsFinanceDbContext`

Mo file:

```text
CADSFINANCE/Infrastructure/Persistence/CadsFinanceDbContext.cs
```

Them `DbSet`:

```csharp
public DbSet<TaiKhoanNganHang> TaiKhoanNganHangs => Set<TaiKhoanNganHang>();
```

Them mapping trong `OnModelCreating`:

```csharp
modelBuilder.Entity<TaiKhoanNganHang>(entity =>
{
    entity.ToTable("LST_TaiKhoanNganHang", "dbo");
    entity.HasKey(item => item.MaTk);

    entity.Property(item => item.MaTk)
        .HasColumnName("MA_TK")
        .HasMaxLength(20);

    entity.Property(item => item.TenTk)
        .HasColumnName("TEN_TK")
        .HasMaxLength(250);

    entity.Property(item => item.SoTaiKhoan)
        .HasColumnName("SO_TAI_KHOAN")
        .HasMaxLength(50);

    entity.Property(item => item.TenNganHang)
        .HasColumnName("TEN_NGAN_HANG")
        .HasMaxLength(250);

    entity.Property(item => item.IsActive)
        .HasColumnName("IS_ACTIVE");
});
```

## 4. Tao DTO cho request/response

Tao folder:

```text
CADSFINANCE/Services/Application/TaiKhoanNganHangService/DTOs
```

Tao file:

```text
CADSFINANCE/Services/Application/TaiKhoanNganHangService/DTOs/TaiKhoanNganHangDto.cs
```

Noi dung mau:

```csharp
namespace CADSFINANCE.Services.Application.TaiKhoanNganHangService.DTOs;

public sealed class TaiKhoanNganHangDto
{
    public string MaTk { get; set; } = string.Empty;

    public string? TenTk { get; set; }

    public string? SoTaiKhoan { get; set; }

    public string? TenNganHang { get; set; }

    public bool? IsActive { get; set; }
}
```

## 5. Tao interface service

Tao file:

```text
CADSFINANCE/Services/Application/TaiKhoanNganHangService/ITaiKhoanNganHangService.cs
```

Noi dung mau:

```csharp
using CADSFINANCE.Services.Application.TaiKhoanNganHangService.DTOs;

namespace CADSFINANCE.Services.Application.TaiKhoanNganHangService;

public interface ITaiKhoanNganHangService
{
    Task<IReadOnlyList<TaiKhoanNganHangDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<TaiKhoanNganHangDto?> GetByIdAsync(string maTk, CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> CreateAsync(TaiKhoanNganHangDto dto, CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> UpdateAsync(string maTk, TaiKhoanNganHangDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string maTk, CancellationToken cancellationToken = default);
}
```

## 6. Tao service xu ly CRUD

Tao file:

```text
CADSFINANCE/Services/Application/TaiKhoanNganHangService/TaiKhoanNganHangService.cs
```

Noi dung mau:

```csharp
using CADSFINANCE.Infrastructure.Persistence;
using CADSFINANCE.Models;
using CADSFINANCE.Services.Application.TaiKhoanNganHangService.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CADSFINANCE.Services.Application.TaiKhoanNganHangService;

public sealed class TaiKhoanNganHangService : ITaiKhoanNganHangService
{
    private readonly CadsFinanceDbContext _dbContext;

    public TaiKhoanNganHangService(CadsFinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TaiKhoanNganHangDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.TaiKhoanNganHangs
            .AsNoTracking()
            .OrderBy(item => item.MaTk)
            .Select(item => ToDto(item))
            .ToListAsync(cancellationToken);
    }

    public async Task<TaiKhoanNganHangDto?> GetByIdAsync(string maTk, CancellationToken cancellationToken = default)
    {
        return await _dbContext.TaiKhoanNganHangs
            .AsNoTracking()
            .Where(item => item.MaTk == maTk)
            .Select(item => ToDto(item))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(bool Success, string? Error)> CreateAsync(TaiKhoanNganHangDto dto, CancellationToken cancellationToken = default)
    {
        var validationError = Validate(dto);
        if (validationError is not null)
        {
            return (false, validationError);
        }

        var maTk = dto.MaTk.Trim();
        if (await _dbContext.TaiKhoanNganHangs.AnyAsync(item => item.MaTk == maTk, cancellationToken))
        {
            return (false, $"MaTk '{dto.MaTk}' already exists.");
        }

        _dbContext.TaiKhoanNganHangs.Add(new TaiKhoanNganHang
        {
            MaTk = maTk,
            TenTk = dto.TenTk,
            SoTaiKhoan = dto.SoTaiKhoan,
            TenNganHang = dto.TenNganHang,
            IsActive = dto.IsActive
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(string maTk, TaiKhoanNganHangDto dto, CancellationToken cancellationToken = default)
    {
        dto.MaTk = maTk;

        var validationError = Validate(dto);
        if (validationError is not null)
        {
            return (false, validationError);
        }

        var entity = await _dbContext.TaiKhoanNganHangs
            .FirstOrDefaultAsync(item => item.MaTk == maTk, cancellationToken);

        if (entity is null)
        {
            return (false, $"MaTk '{maTk}' was not found.");
        }

        entity.TenTk = dto.TenTk;
        entity.SoTaiKhoan = dto.SoTaiKhoan;
        entity.TenNganHang = dto.TenNganHang;
        entity.IsActive = dto.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<bool> DeleteAsync(string maTk, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.TaiKhoanNganHangs
            .FirstOrDefaultAsync(item => item.MaTk == maTk, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _dbContext.TaiKhoanNganHangs.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string? Validate(TaiKhoanNganHangDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.MaTk))
        {
            return "MaTk is required.";
        }

        if (dto.MaTk.Length > 20)
        {
            return "MaTk must be 20 characters or less.";
        }

        if (dto.TenTk?.Length > 250)
        {
            return "TenTk must be 250 characters or less.";
        }

        return null;
    }

    private static TaiKhoanNganHangDto ToDto(TaiKhoanNganHang entity)
    {
        return new TaiKhoanNganHangDto
        {
            MaTk = entity.MaTk,
            TenTk = entity.TenTk,
            SoTaiKhoan = entity.SoTaiKhoan,
            TenNganHang = entity.TenNganHang,
            IsActive = entity.IsActive
        };
    }
}
```

## 7. Dang ky service vao DI

Mo file:

```text
CADSFINANCE/Extensions/ServiceCollectionExtensions.cs
```

Them `using`:

```csharp
using CADSFINANCE.Services.Application.TaiKhoanNganHangService;
```

Trong method `AddApplicationServices`, them:

```csharp
services.AddScoped<ITaiKhoanNganHangService, TaiKhoanNganHangService>();
```

## 8. Tao API controller

Tao file:

```text
CADSFINANCE/Controllers/TaiKhoanNganHangController.cs
```

Noi dung mau:

```csharp
using CADSFINANCE.Services.Application.TaiKhoanNganHangService;
using CADSFINANCE.Services.Application.TaiKhoanNganHangService.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CADSFINANCE.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TaiKhoanNganHangController : ControllerBase
{
    private readonly ITaiKhoanNganHangService _service;

    public TaiKhoanNganHangController(ITaiKhoanNganHangService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaiKhoanNganHangDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _service.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{maTk}")]
    public async Task<ActionResult<TaiKhoanNganHangDto>> GetById(string maTk, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(maTk, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TaiKhoanNganHangDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { maTk = dto.MaTk }, dto);
    }

    [HttpPut("{maTk}")]
    public async Task<IActionResult> Update(string maTk, [FromBody] TaiKhoanNganHangDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(maTk, dto, cancellationToken);
        if (result.Success)
        {
            return NoContent();
        }

        return result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
            ? NotFound(result.Error)
            : BadRequest(result.Error);
    }

    [HttpDelete("{maTk}")]
    public async Task<IActionResult> Delete(string maTk, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(maTk, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
```

## 9. Them migration

Neu bang moi se duoc EF Core tao ra, chay lenh:

```powershell
dotnet ef migrations add AddTaiKhoanNganHang --project CADSFINANCE --startup-project CADSFINANCE
```

Neu may chua co tool `dotnet ef`, cai bang lenh:

```powershell
dotnet tool install --global dotnet-ef
```

Neu da cai nhung version cu, update:

```powershell
dotnet tool update --global dotnet-ef
```

Sau khi tao migration, kiem tra file trong:

```text
CADSFINANCE/Migrations
```

Can doc migration truoc khi update database de dam bao:

- Dung ten bang.
- Dung schema `dbo`.
- Dung khoa chinh.
- Dung do dai cot.
- Khong drop/sua bang ngoai y muon.

## 10. Update database

Chay:

```powershell
dotnet ef database update --project CADSFINANCE --startup-project CADSFINANCE
```

Lenh nay se lay `ConnectionStrings:DefaultConnection` tu config hien tai.

Trong production, khong nen de connection string that trong `appsettings.json`. Hay override bang:

- Environment variable.
- Kubernetes secret.
- CI/CD secret.

Vi du environment variable:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=...;Database=...;User Id=...;Password=...;Encrypt=True;TrustServerCertificate=True"
```

## 11. Truong hop bang da ton tai san trong database

Neu bang da ton tai va khong muon EF tao bang moi:

1. Tao entity va mapping nhu cac buoc tren.
2. Khong can tao migration neu khong thay doi schema database.
3. Build va test API truc tiep.

Neu van muon EF Core ghi nhan schema hien tai lam baseline:

```powershell
dotnet ef migrations add BaselineTaiKhoanNganHang --project CADSFINANCE --startup-project CADSFINANCE --ignore-changes
```

Luu y: `--ignore-changes` phu thuoc version EF tooling. Neu lenh khong ho tro, can tao migration rong thu cong hoac dung quy trinh migration rieng cua team.

## 12. Build va test Swagger

Build:

```powershell
dotnet build CADSFINANCE.sln
```

Run:

```powershell
dotnet run --project CADSFINANCE
```

Mo Swagger:

```text
https://localhost:7129/swagger
```

Dang nhap Swagger, sau do test cac endpoint:

- `GET /api/TaiKhoanNganHang`
- `GET /api/TaiKhoanNganHang/{maTk}`
- `POST /api/TaiKhoanNganHang`
- `PUT /api/TaiKhoanNganHang/{maTk}`
- `DELETE /api/TaiKhoanNganHang/{maTk}`

## 13. Checklist khi tao CRUD moi

- Da tao entity trong `Models`.
- Da mapping entity trong `CadsFinanceDbContext`.
- Da tao DTO.
- Da tao interface service.
- Da tao service implementation.
- Da dang ky service trong `AddApplicationServices`.
- Da tao controller.
- Da tao migration neu can EF tao/sua bang.
- Da review migration truoc khi update database.
- Da update database.
- Da build thanh cong.
- Da test CRUD tren Swagger.

## 14. Nguyen tac cho du an finance

- Khong tra truc tiep entity database ra ngoai API. Nen tra DTO.
- Khong de controller truy cap database truc tiep. Controller goi service.
- Khong viet SQL string trong controller neu co the dung `DbContext`.
- Raw SQL chi nen dung cho report, truy van dong, hoac case can toi uu ro rang.
- Kiem tra validation truoc khi ghi database.
- Khong hardcode connection string that trong source code.
- Migration can duoc review ky vi co the anh huong du lieu tai chinh.
