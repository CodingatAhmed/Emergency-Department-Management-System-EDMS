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
public class EncounterController : ControllerBase
{
    private readonly IEncounterRepository _encounters;
    private readonly NotificationService _notificationService;

    public EncounterController(IEncounterRepository encounters, NotificationService notificationService)
    {
        _encounters = encounters;
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Encounter>>> Create([FromBody] CreateEncounterRequest request)
    {
        var encounter = new Encounter
        {
            PatientId = request.PatientId,
            EncounterType = request.EncounterType,
            TriageCategory = request.TriageCategory,
            CurrentState = EncounterState.Waiting,
            ArrivalTime = DateTime.UtcNow
        };

        var created = await _encounters.CreateAsync(encounter);
        await _notificationService.NotifyNewPatientArrivalAsync(created);
        return Ok(new ApiResponse<Encounter> { Success = true, Data = created, Message = "Encounter created." });
    }

    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Encounter>>>> Active()
    {
        var rows = await _encounters.GetActiveAsync();
        return Ok(new ApiResponse<IEnumerable<Encounter>> { Success = true, Data = rows });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<Encounter>>> GetById(Guid id)
    {
        var encounter = await _encounters.GetByIdAsync(id);
        if (encounter is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Encounter not found." });

        return Ok(new ApiResponse<Encounter> { Success = true, Data = encounter });
    }

    [HttpGet("patient/{patientId:guid}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Encounter>>>> ByPatient(Guid patientId)
    {
        var rows = await _encounters.GetByPatientAsync(patientId);
        return Ok(new ApiResponse<IEnumerable<Encounter>> { Success = true, Data = rows });
    }

    [HttpPut("{id:guid}/state")]
    public async Task<ActionResult<ApiResponse<Encounter>>> UpdateState(Guid id, [FromBody] UpdateEncounterStateRequest request)
    {
        var current = await _encounters.GetByIdAsync(id);
        if (current is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Encounter not found." });

        var oldState = current.CurrentState;
        var updated = await _encounters.UpdateStateAsync(id, request.NewState);
        await _notificationService.NotifyEncounterStateChangeAsync(updated, oldState);
        if (request.NewState == EncounterState.InService)
            await _notificationService.NotifyPatientCalledAsync(updated);
        return Ok(new ApiResponse<Encounter> { Success = true, Data = updated, Message = "Encounter state updated." });
    }

    [HttpPut("call-next")]
    public async Task<ActionResult<ApiResponse<Encounter>>> CallNext()
    {
        var active = await _encounters.GetActiveAsync();
        var waiting = active.Where(x => x.CurrentState == EncounterState.Waiting).ToList();
        if (waiting.Count == 0)
            return NotFound(new ApiResponse<object> { Success = false, Message = "No waiting encounters." });

        var priority = new Dictionary<TriageCategory, int>
        {
            [TriageCategory.UrgentCare] = 1,
            [TriageCategory.MinorProcedure] = 2,
            [TriageCategory.Diagnostics] = 3,
            [TriageCategory.TherapySession] = 4,
            [TriageCategory.Routine] = 5
        };

        var next = waiting
            .OrderBy(x => x.TriageCategory is null ? 99 : priority.GetValueOrDefault(x.TriageCategory.Value, 99))
            .ThenBy(x => x.ArrivalTime)
            .First();

        var updated = await _encounters.UpdateStateAsync(next.EncounterId, EncounterState.InService);
        await _notificationService.NotifyEncounterStateChangeAsync(updated, next.CurrentState);
        await _notificationService.NotifyPatientCalledAsync(updated);

        return Ok(new ApiResponse<Encounter> { Success = true, Data = updated, Message = "Next patient called." });
    }
}
