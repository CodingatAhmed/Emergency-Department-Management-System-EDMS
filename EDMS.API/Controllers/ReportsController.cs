using EDMS.API.Models;
using EDMS.Core.Domain;
using EDMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminPolicy")]
public class ReportsController : ControllerBase
{
    private readonly IEncounterRepository _encounters;
    private readonly IQueueRepository _queue;
    private readonly IResourceRepository _resources;

    public ReportsController(IEncounterRepository encounters, IQueueRepository queue, IResourceRepository resources)
    {
        _encounters = encounters;
        _queue = queue;
        _resources = resources;
    }

    [HttpGet("daily-summary")]
    public async Task<ActionResult<ApiResponse<object>>> DailySummary([FromQuery] DateTime? date = null)
    {
        var day = (date ?? DateTime.UtcNow).Date;
        var all = (await _encounters.GetActiveAsync()).ToList();
        var latest = await _queue.GetLatestAsync();

        var data = new
        {
            Date = day,
            ActiveEncounters = all.Count,
            WaitingCount = all.Count(x => x.CurrentState == EncounterState.Waiting),
            InServiceCount = all.Count(x => x.CurrentState == EncounterState.InService),
            AvgEstimatedWaitMin = all.Count == 0 ? 0 : all.Average(x => x.EstimatedWaitMin ?? 0),
            CurrentRho = latest?.Rho,
            LatestModel = latest?.ModelId
        };

        return Ok(new ApiResponse<object> { Success = true, Data = data });
    }

    [HttpGet("queue-performance")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> QueuePerformance([FromQuery] int hours = 24)
    {
        var rows = await _queue.GetHistoryAsync(hours);
        var data = rows.Select(x => new
        {
            x.ComputedAtUtc,
            x.Rho,
            x.Lq,
            x.Wq,
            WqMinutes = x.Wq * 60
        });
        return Ok(new ApiResponse<IEnumerable<object>> { Success = true, Data = data });
    }

    [HttpGet("resource-utilization")]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> ResourceUtilization()
    {
        var resources = await _resources.GetAllAsync();
        var data = resources.Select(r => new
        {
            r.ResourceId,
            r.Name,
            r.ResourceType,
            r.Status,
            OccupiedSince = r.OccupiedSince
        });
        return Ok(new ApiResponse<IEnumerable<object>> { Success = true, Data = data });
    }

    [HttpGet("triage-breakdown")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<IEnumerable<object>>>> TriageBreakdown()
    {
        var active = await _encounters.GetActiveAsync();
        var groups = active
            .GroupBy(x => x.TriageCategory?.ToString() ?? "Unassigned")
            .Select(g => new
            {
                Triage = g.Key,
                Count = g.Count(),
                AvgEstimatedWaitMin = g.Any() ? g.Average(x => x.EstimatedWaitMin ?? 0) : 0
            })
            .OrderByDescending(x => x.Count);

        return Ok(new ApiResponse<IEnumerable<object>> { Success = true, Data = groups });
    }

    [HttpGet("model-comparison")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> ModelComparison([FromQuery] int hours = 24)
    {
        var rows = (await _queue.GetHistoryAsync(hours)).ToList();
        var grouped = rows
            .GroupBy(x => x.ModelId)
            .Select(g => new
            {
                ModelId = g.Key,
                Samples = g.Count(),
                AvgRho = g.Average(x => x.Rho),
                AvgWqMin = g.Average(x => x.Wq) * 60,
                PeakWqMin = g.Max(x => x.Wq) * 60
            });

        return Ok(new ApiResponse<object> { Success = true, Data = grouped });
    }

    [HttpGet("sla-compliance")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<object>>> SlaCompliance([FromQuery] int hours = 24)
    {
        var rows = (await _queue.GetHistoryAsync(hours)).ToList();
        var total = rows.Count;
        var breaches = rows.Count(x => x.Wq > 0.5);
        var compliance = total == 0 ? 100 : ((double)(total - breaches) / total) * 100.0;

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = new
            {
                Hours = hours,
                TotalSnapshots = total,
                SlaBreaches = breaches,
                CompliancePercent = compliance
            }
        });
    }
}
