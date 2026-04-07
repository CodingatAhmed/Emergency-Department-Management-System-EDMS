namespace EDMS.Console.Models;

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

public class QueueRequest
{
    public Guid? EncounterId { get; set; }
    public double Lambda { get; set; }
    public double Mu { get; set; }
    public double? SigmaS2 { get; set; }
    public double? SigmaA2 { get; set; }
    public string ModelType { get; set; } = "MM1";
}

public class QueueMetrics
{
    public string ModelUsed { get; set; } = string.Empty;
    public double Rho { get; set; }
    public double Lq { get; set; }
    public double Wq { get; set; }
    public double L { get; set; }
    public double W { get; set; }
    public string Interpretation { get; set; } = string.Empty;
}

public class ResourceDto
{
    public Guid ResourceId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Department { get; set; }
}

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

public class PatientDto
{
    public Guid PatientId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MRN { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public List<string> AllergiesList { get; set; } = [];
}

public class CreateEncounterRequest
{
    public Guid PatientId { get; set; }
    public string EncounterType { get; set; } = "WalkIn";
    public string? TriageCategory { get; set; }
}

public class EncounterDto
{
    public Guid EncounterId { get; set; }
    public Guid PatientId { get; set; }
    public string EncounterType { get; set; } = string.Empty;
    public string? TriageCategory { get; set; }
    public string CurrentState { get; set; } = string.Empty;
    public DateTime ArrivalTime { get; set; }
}

public class VerifyOrderRequest
{
    public Guid PatientId { get; set; }
    public string DrugName { get; set; } = string.Empty;
}

public class AllergyCheckResult
{
    public bool HasConflict { get; set; }
    public string? Severity { get; set; }
    public string? ConflictDetail { get; set; }
    public string? AllergenName { get; set; }
}

public class AppointmentDto
{
    public Guid AppointmentId { get; set; }
    public Guid PatientId { get; set; }
    public string QRCode { get; set; } = string.Empty;
    public DateTime ScheduledTime { get; set; }
    public string Department { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
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

public class QueueAlertDto
{
    public Guid AlertId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? Message { get; set; }
    public DateTime TriggeredAt { get; set; }
    public bool IsResolved { get; set; }
}

public class DailySummaryDto
{
    public DateTime Date { get; set; }
    public int ActiveEncounters { get; set; }
    public int WaitingCount { get; set; }
    public int InServiceCount { get; set; }
    public decimal AvgEstimatedWaitMin { get; set; }
    public double? CurrentRho { get; set; }
}

public class QueuePerformancePoint
{
    public DateTime ComputedAtUtc { get; set; }
    public double Rho { get; set; }
    public double Lq { get; set; }
    public double Wq { get; set; }
    public double WqMinutes { get; set; }
}

public class TriageBreakdownItem
{
    public string Triage { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal AvgEstimatedWaitMin { get; set; }
}

public class ModelComparisonItem
{
    public Guid ModelId { get; set; }
    public int Samples { get; set; }
    public double AvgRho { get; set; }
    public double AvgWqMin { get; set; }
    public double PeakWqMin { get; set; }
}

public class SlaComplianceDto
{
    public int Hours { get; set; }
    public int TotalSnapshots { get; set; }
    public int SlaBreaches { get; set; }
    public double CompliancePercent { get; set; }
}
