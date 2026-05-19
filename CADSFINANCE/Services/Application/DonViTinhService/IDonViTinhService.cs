using CADSFINANCE.Services.Application.DonViTinhService.DTOs;

namespace CADSFINANCE.Services.Application.DonViTinhService;

public interface IDonViTinhService
{
    Task<IReadOnlyList<DonViTinhDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<DonViTinhDto?> GetByIdAsync(string maDvt, CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> CreateAsync(DonViTinhDto dto, CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error)> UpdateAsync(string maDvt, DonViTinhDto dto, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string maDvt, CancellationToken cancellationToken = default);
}
