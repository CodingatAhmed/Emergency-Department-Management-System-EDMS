namespace EDMS.Core.Domain;

public class QueueSnapshot
{
    public Guid SnapshotId { get; set; } = Guid.NewGuid();
    public Guid? EncounterId { get; set; }
    public Guid ModelId { get; set; }
    public double Lambda { get; set; }
    public double Mu { get; set; }
    public double? SigmaS2 { get; set; }
    public double? SigmaA2 { get; set; }
    public double Rho { get; set; }
    public double Lq { get; set; }
    public double Wq { get; set; }
    public double L { get; set; }
    public double W { get; set; }
    public double? Cv { get; set; }
    public double? CaSquared { get; set; }
    public double? CsSquared { get; set; }
    public DateTime ComputedAtUtc { get; set; } = DateTime.UtcNow;
}
