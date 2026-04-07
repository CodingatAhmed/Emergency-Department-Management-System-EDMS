using EDMS.Core.Domain;

namespace EDMS.API.Models;

public class RegisterPatientRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? MRN { get; set; }
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }
    public string Gender { get; set; } = "Other";
    public List<string> Allergies { get; set; } = [];
    public List<string> ActiveProblems { get; set; } = [];
}

public class CreateEncounterRequest
{
    public Guid PatientId { get; set; }
    public string EncounterType { get; set; } = "WalkIn";
    public TriageCategory? TriageCategory { get; set; }
}

public class UpdateEncounterStateRequest
{
    public EncounterState NewState { get; set; }
}

public class CreateAppointmentRequest
{
    public Guid PatientId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public string Department { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
}

public class ValidateQrRequest
{
    public string QrCode { get; set; } = string.Empty;
}
