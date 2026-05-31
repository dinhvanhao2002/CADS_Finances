using CADSFINANCE.Infrastructure.Persistence;
using CADSFINANCE.Models;
using CADSFINANCE.Services.Application.DonViTinhService.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CADSFINANCE.Services.Application.DonViTinhService;

public sealed class DonViTinhService : IDonViTinhService
{
    private readonly CadsFinanceDbContext _dbContext;

    public DonViTinhService(CadsFinanceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DonViTinhDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.DonViTinhs
            .AsNoTracking()
            .OrderBy(item => item.MaDvt)
            .Select(item => ToDto(item))
            .ToListAsync(cancellationToken);
    }

    public async Task<DonViTinhDto?> GetByIdAsync(string maDvt, CancellationToken cancellationToken = default)
    {
        return await _dbContext.DonViTinhs
            .AsNoTracking()
            .Where(item => item.MaDvt == maDvt)
            .Select(item => ToDto(item))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<(bool Success, string? Error)> CreateAsync(DonViTinhDto dto, CancellationToken cancellationToken = default)
    {
        var validationError = Validate(dto);
        if (validationError is not null)
        {
            return (false, validationError);
        }

        var maDvt = dto.MaDvt.Trim();
        if (await _dbContext.DonViTinhs.AnyAsync(item => item.MaDvt == maDvt, cancellationToken))
        {
            return (false, $"MaDvt '{dto.MaDvt}' already exists.");
        }

        _dbContext.DonViTinhs.Add(new DonViTinh
        {
            MaDvt = maDvt,
            TenDvt = dto.TenDvt,
            UserId = dto.UserId,
            QuyCach = dto.QuyCach,
            IsActive = dto.IsActive
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(string maDvt, DonViTinhDto dto, CancellationToken cancellationToken = default)
    {
        dto.MaDvt = maDvt;

        var validationError = Validate(dto);
        if (validationError is not null)
        {
            return (false, validationError);
        }

        var entity = await _dbContext.DonViTinhs
            .FirstOrDefaultAsync(item => item.MaDvt == maDvt, cancellationToken);

        if (entity is null)
        {
            return (false, $"MaDvt '{maDvt}' was not found.");
        }

        entity.TenDvt = dto.TenDvt;
        entity.UserId = dto.UserId;
        entity.QuyCach = dto.QuyCach;
        entity.IsActive = dto.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<bool> DeleteAsync(string maDvt, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.DonViTinhs
            .FirstOrDefaultAsync(item => item.MaDvt == maDvt, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        _dbContext.DonViTinhs.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private static string? Validate(DonViTinhDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.MaDvt))
        {
            return "MaDvt is required.";
        }

        if (dto.MaDvt.Length > 10)
        {
            return "MaDvt must be 10 characters or less.";
        }

        if (dto.TenDvt?.Length > 50)
        {
            return "TenDvt must be 50 characters or less.";
        }

        return null;
    }

    private static DonViTinhDto ToDto(DonViTinh entity)
    {
        return new DonViTinhDto
        {
            MaDvt = entity.MaDvt,
            TenDvt = entity.TenDvt,
            UserId = entity.UserId,
            QuyCach = entity.QuyCach,
            IsActive = entity.IsActive
        };
    }
}
