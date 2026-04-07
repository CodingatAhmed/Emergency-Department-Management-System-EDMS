using EDMS.API.Models;
using EDMS.Core.Domain;
using EDMS.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDMS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "StaffPolicy")]
public class PatientController : ControllerBase
{
    private readonly IPatientRepository _patients;

    public PatientController(IPatientRepository patients)
    {
        _patients = patients;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<Patient>>> Register([FromBody] RegisterPatientRequest request)
    {
        if (request.DateOfBirth >= DateTime.UtcNow.Date)
            return BadRequest(new ApiResponse<object> { Success = false, Message = "Date of birth must be in the past." });

        var patient = new Patient
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            DateOfBirth = request.DateOfBirth.Date,
            MRN = string.IsNullOrWhiteSpace(request.MRN) ? $"MRN-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(10000, 99999)}" : request.MRN.Trim(),
            ContactNumber = request.ContactNumber,
            Email = request.Email,
            Gender = request.Gender,
            AllergiesList = request.Allergies,
            ActiveProblems = request.ActiveProblems
        };

        var created = await _patients.CreateAsync(patient);
        return Ok(new ApiResponse<Patient> { Success = true, Data = created, Message = "Patient registered." });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<Patient>>> GetById(Guid id)
    {
        var patient = await _patients.GetByIdAsync(id);
        if (patient is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Patient not found." });

        return Ok(new ApiResponse<Patient> { Success = true, Data = patient });
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<IEnumerable<Patient>>>> Search([FromQuery] string q)
    {
        var items = await _patients.SearchAsync(q);
        return Ok(new ApiResponse<IEnumerable<Patient>> { Success = true, Data = items });
    }

    [HttpGet("mrn/{mrn}")]
    public async Task<ActionResult<ApiResponse<Patient>>> GetByMrn(string mrn)
    {
        var patient = await _patients.GetByMrnAsync(mrn);
        if (patient is null)
            return NotFound(new ApiResponse<object> { Success = false, Message = "Patient not found." });

        return Ok(new ApiResponse<Patient> { Success = true, Data = patient });
    }
}
