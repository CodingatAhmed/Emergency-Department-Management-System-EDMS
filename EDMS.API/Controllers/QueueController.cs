using EDMS.API.Models;
using EDMS.Core.Domain;
using EDMS.Core.Interfaces;
using EDMS.Core.Queue;
using EDMS.API.Services;
using EDMS.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "StaffPolicy")]
public class QueueController : ControllerBase
{
    private readonly ModelSelectorService _selectorService;
    private readonly IQueueRepository _queueRepository;
    private readonly NotificationService _notificationService;
    private readonly AppDbContext _db;

    public QueueController(ModelSelectorService selectorService, IQueueRepository queueRepository, NotificationService notificationService, AppDbContext db)
    {
        _selectorService = selectorService;
        _queueRepository = queueRepository;
        _notificationService = notificationService;
        _db = db;
    }

    [HttpPost("compute")]
    public async Task<ActionResult<ApiResponse<QueueMetrics>>> Compute([FromBody] QueueRequest request)
    {
        try
        {
            var metrics = _selectorService.Compute(request);
            var model = await _queueRepository.GetModelByTypeAsync(metrics.ModelUsed);
            if (model is null)
                throw new InvalidOperationException($"Queue model '{metrics.ModelUsed}' is not configured.");

            var snapshot = await _queueRepository.SaveSnapshotAsync(new QueueSnapshot
            {
                ModelId = model.ModelId,
                Lambda = metrics.Lambda,
                Mu = metrics.Mu,
                SigmaS2 = request.SigmaS2,
                SigmaA2 = request.SigmaA2,
                Rho = metrics.Rho,
                Lq = metrics.Lq,
                Wq = metrics.Wq,
                L = metrics.L,
                W = metrics.W,
                Cv = metrics.Cv,
                CaSquared = metrics.CaSquared,
                CsSquared = metrics.CsSquared,
                ComputedAtUtc = DateTime.UtcNow
            });

            await _notificationService.NotifyQueueUpdateAsync(metrics);
            if (metrics.SlaBreach)
            {
                await _notificationService.NotifySlaBreachAsync(request.EncounterId, metrics);
                _db.QueueAlerts.Add(new QueueAlert
                {
                    SnapshotId = snapshot.SnapshotId,
                    EncounterId = request.EncounterId,
                    AlertType = "SLABreach",
                    ThresholdMinutes = 30,
                    ActualWqHours = metrics.Wq,
                    Severity = metrics.Wq > 1 ? "Critical" : "High",
                    Message = $"Queue wait exceeded SLA: {metrics.Wq * 60:F1} min"
                });
            }
            if (metrics.CapacityWarning)
            {
                await _notificationService.NotifyCapacityWarningAsync(metrics);
                _db.QueueAlerts.Add(new QueueAlert
                {
                    SnapshotId = snapshot.SnapshotId,
                    EncounterId = request.EncounterId,
                    AlertType = "CapacityWarning",
                    ThresholdMinutes = 0,
                    ActualWqHours = metrics.Wq,
                    Severity = metrics.Rho > 0.9 ? "Critical" : "Warning",
                    Message = $"Capacity warning: rho={metrics.Rho:F2}"
                });
            }

            await _db.SaveChangesAsync();

            return Ok(new ApiResponse<QueueMetrics>
            {
                Success = true,
                Data = metrics,
                Message = "Queue metrics computed successfully."
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Data = null,
                Message = ex.Message
            });
        }
    }

    [HttpGet("models")]
    [AllowAnonymous]
    public ActionResult<ApiResponse<IEnumerable<object>>> Models()
    {
        var models = new[]
        {
            new { ModelType = "MM1", Description = "Poisson arrival, exponential service, exact formula." },
            new { ModelType = "MG1", Description = "Poisson arrival, general service, Pollaczek-Khinchine." },
            new { ModelType = "GG1", Description = "General arrival/service, Kingman approximation." }
        };

        return Ok(new ApiResponse<IEnumerable<object>>
        {
            Success = true,
            Data = models,
            Message = "Available queue models."
        });
    }

    [HttpGet("metrics/latest")]
    public async Task<ActionResult<ApiResponse<QueueSnapshot>>> Latest()
    {
        var latest = await _queueRepository.GetLatestAsync();
        if (latest is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "No queue snapshots found." });

        return Ok(new ApiResponse<QueueSnapshot> { Success = true, Data = latest });
    }

    [HttpGet("metrics/history")]
    public async Task<ActionResult<ApiResponse<IEnumerable<QueueSnapshot>>>> History([FromQuery] int hours = 24)
    {
        var rows = await _queueRepository.GetHistoryAsync(hours);
        return Ok(new ApiResponse<IEnumerable<QueueSnapshot>> { Success = true, Data = rows });
    }

    [HttpGet("live-stats")]
    public async Task<ActionResult<ApiResponse<object>>> LiveStats([FromServices] IEncounterRepository encounters)
    {
        var active = await encounters.GetActiveAsync();
        var waiting = active.Where(x => x.CurrentState == EncounterState.Waiting).ToList();
        var latest = await _queueRepository.GetLatestAsync();

        var stats = new
        {
            TotalWaiting = waiting.Count,
            ByTriage = waiting
                .GroupBy(x => x.TriageCategory?.ToString() ?? "Unassigned")
                .ToDictionary(g => g.Key, g => g.Count()),
            CurrentRho = latest?.Rho,
            AvgEstimatedWaitMin = waiting.Count == 0 ? 0 : waiting.Average(x => x.EstimatedWaitMin ?? 0),
            LatestComputedAtUtc = latest?.ComputedAtUtc
        };

        return Ok(new ApiResponse<object> { Success = true, Data = stats });
    }
}
