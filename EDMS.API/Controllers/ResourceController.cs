using EDMS.API.Models;
using EDMS.API.Services;
using EDMS.Core.Domain;
using EDMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "StaffPolicy")]
public class ResourceController : ControllerBase
{
    private readonly IResourceRepository _resources;
    private readonly NotificationService _notificationService;

    public ResourceController(IResourceRepository resources, NotificationService notificationService)
    {
        _resources = resources;
        _notificationService = notificationService;
    }

    [HttpGet("availability")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Resource>>>> Availability()
    {
        var items = await _resources.GetAllAsync();
        return Ok(new ApiResponse<IEnumerable<Resource>> { Success = true, Data = items });
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<Resource>>> UpdateStatus(Guid id, [FromBody] string newStatus)
    {
        var resource = await _resources.GetByIdAsync(id);
        if (resource is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Resource not found." });

        var old = resource.Status;
        resource.Status = newStatus;
        resource.UpdatedAtUtc = DateTime.UtcNow;
        if (newStatus.Equals("Occupied", StringComparison.OrdinalIgnoreCase))
        {
            resource.OccupiedSince = DateTime.UtcNow;
            resource.AvailableFrom = null;
        }
        else if (newStatus.Equals("Available", StringComparison.OrdinalIgnoreCase))
        {
            resource.AvailableFrom = DateTime.UtcNow;
        }

        var saved = await _resources.UpdateAsync(resource);
        await _notificationService.NotifyResourceChangeAsync(saved, old);

        return Ok(new ApiResponse<Resource> { Success = true, Data = saved, Message = "Resource status updated." });
    }
}
