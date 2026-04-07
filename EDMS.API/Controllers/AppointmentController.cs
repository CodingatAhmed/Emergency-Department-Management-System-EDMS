using EDMS.API.Models;
using EDMS.Core.Domain;
using EDMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "StaffPolicy")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentRepository _appointments;
    private readonly IEncounterRepository _encounters;

    public AppointmentController(IAppointmentRepository appointments, IEncounterRepository encounters)
    {
        _appointments = appointments;
        _encounters = encounters;
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Appointment>>> Create([FromBody] CreateAppointmentRequest request)
    {
        var appointment = new Appointment
        {
            PatientId = request.PatientId,
            ScheduledTime = request.ScheduledTime,
            Department = request.Department,
            DoctorName = request.DoctorName,
            QRCode = Guid.NewGuid().ToString("N"),
            Status = "Scheduled"
        };

        var created = await _appointments.CreateAsync(appointment);
        return Ok(new ApiResponse<Appointment> { Success = true, Data = created, Message = "Appointment created." });
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Appointment>>>> GetByDate([FromQuery] DateTime? date = null)
    {
        var items = await _appointments.GetByDateAsync(date ?? DateTime.UtcNow.Date);
        return Ok(new ApiResponse<IEnumerable<Appointment>> { Success = true, Data = items });
    }

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<ApiResponse<Appointment>>> UpdateStatus(Guid id, [FromBody] string status)
    {
        var item = await _appointments.GetByIdAsync(id);
        if (item is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Appointment not found." });

        item.Status = status;
        var updated = await _appointments.UpdateAsync(item);
        return Ok(new ApiResponse<Appointment> { Success = true, Data = updated, Message = "Appointment status updated." });
    }

    [HttpPost("validate-qr")]
    public async Task<ActionResult<ApiResponse<object>>> ValidateQr([FromBody] ValidateQrRequest request)
    {
        var appt = await _appointments.GetByQrCodeAsync(request.QrCode);
        if (appt is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Invalid QR code." });

        var deltaMin = Math.Abs((DateTime.UtcNow - appt.ScheduledTime).TotalMinutes);
        if (!string.Equals(appt.Status, "Scheduled", StringComparison.OrdinalIgnoreCase) || deltaMin > 60)
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Appointment is not valid for check-in." });

        appt.Status = "CheckedIn";
        await _appointments.UpdateAsync(appt);

        var encounter = await _encounters.CreateAsync(new Encounter
        {
            PatientId = appt.PatientId,
            EncounterType = "Scheduled",
            CurrentState = EncounterState.Waiting,
            ArrivalTime = DateTime.UtcNow
        });

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Data = new { Appointment = appt, Encounter = encounter },
            Message = "Appointment validated and encounter created."
        });
    }
}
