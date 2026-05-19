using CADSFINANCE.Services.Application.DonViTinhService;
using CADSFINANCE.Services.Application.DonViTinhService.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace CADSFINANCE.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DonViTinhController : ControllerBase
{
    private readonly IDonViTinhService _service;

    public DonViTinhController(IDonViTinhService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DonViTinhDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _service.GetAllAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{maDvt}")]
    public async Task<ActionResult<DonViTinhDto>> GetById(string maDvt, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(maDvt, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DonViTinhDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);
        if (!result.Success)
        {
            return BadRequest(result.Error);
        }

        return CreatedAtAction(nameof(GetById), new { maDvt = dto.MaDvt }, dto);
    }

    [HttpPut("{maDvt}")]
    public async Task<IActionResult> Update(string maDvt, [FromBody] DonViTinhDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(maDvt, dto, cancellationToken);
        if (result.Success)
        {
            return NoContent();
        }

        return result.Error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
            ? NotFound(result.Error)
            : BadRequest(result.Error);
    }

    [HttpDelete("{maDvt}")]
    public async Task<IActionResult> Delete(string maDvt, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(maDvt, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
