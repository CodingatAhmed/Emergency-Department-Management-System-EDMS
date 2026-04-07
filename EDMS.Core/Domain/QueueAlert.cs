namespace EDMS.Core.Domain;

public class QueueAlert
{
    public Guid AlertId { get; set; } = Guid.NewGuid();
    public Guid SnapshotId { get; set; }
    public Guid? EncounterId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public double ThresholdMinutes { get; set; }
    public double ActualWqHours { get; set; }
    public string Severity { get; set; } = "Info";
    public string? Message { get; set; }
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
    public bool IsResolved { get; set; }
}
