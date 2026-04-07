using EDMS.API.Models;
using EDMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EDMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "StaffPolicy")]
public class AlertController : ControllerBase
{
    private readonly AppDbContext _db;

    public AlertController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<object>>> Get([FromQuery] bool resolved = false)
    {
        var items = await _db.QueueAlerts
            .Where(x => x.IsResolved == resolved)
            .OrderByDescending(x => x.TriggeredAt)
            .Take(200)
            .ToListAsync();

        return Ok(new ApiResponse<object> { Success = true, Data = items });
    }

    [HttpPut("{id:guid}/resolve")]
    public async Task<ActionResult<ApiResponse<object>>> Resolve(Guid id)
    {
        var alert = await _db.QueueAlerts.FirstOrDefaultAsync(x => x.AlertId == id);
        if (alert is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Alert not found." });

        alert.IsResolved = true;
        alert.ResolvedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new ApiResponse<object> { Success = true, Data = alert, Message = "Alert resolved." });
    }
}
