namespace EDMS.Core.Domain;

public class Appointment
{
    public Guid AppointmentId { get; set; } = Guid.NewGuid();
    public Guid PatientId { get; set; }
    public string QRCode { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime ScheduledTime { get; set; }
    public string Department { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string Status { get; set; } = "Scheduled";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
