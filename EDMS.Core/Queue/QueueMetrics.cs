namespace EDMS.Core.Queue;

public class QueueMetrics
{
    public string ModelUsed { get; set; } = string.Empty;
    public double Lambda { get; set; }
    public double Mu { get; set; }
    public double Rho { get; set; }
    public double Lq { get; set; }
    public double Wq { get; set; }
    public double L { get; set; }
    public double W { get; set; }
    public double? Cv { get; set; }
    public double? CaSquared { get; set; }
    public double? CsSquared { get; set; }
    public bool SlaBreach { get; set; }
    public bool CapacityWarning { get; set; }
    public bool StabilityViolation { get; set; }
    public string Interpretation { get; set; } = string.Empty;
    public DateTime ComputedAtUtc { get; set; } = DateTime.UtcNow;
}
