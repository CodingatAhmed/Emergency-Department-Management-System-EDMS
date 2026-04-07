namespace EDMS.Core.Domain;

public class Encounter
{
    public Guid EncounterId { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public Guid? QueueModelId { get; set; }
    public Guid? ResourceId { get; set; }
    public Guid? AppointmentId { get; set; }
    public string EncounterType { get; set; } = "WalkIn";
    public DateTime ArrivalTime { get; set; } = DateTime.UtcNow;
    public DateTime? ServiceStartTime { get; set; }
    public DateTime? DischargeTime { get; set; }
    public TriageCategory? TriageCategory { get; set; }
    public decimal? EstimatedWaitMin { get; set; }
    public decimal? ActualWaitMin { get; set; }
    public EncounterState CurrentState { get; set; } = EncounterState.Waiting;
    public FinalDisposition? FinalDisposition { get; set; }
    public string? AssignedRoom { get; set; }
    public string? AssignedDoctorId { get; set; }
    public float? BloodPressureSystolic { get; set; }
    public float? BloodPressureDiastolic { get; set; }
    public float? HeartRate { get; set; }
    public float? Temperature { get; set; }
    public float? SpO2 { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
