namespace EDMS.Core.Queue;

public class QueueRequest
{
    public Guid? EncounterId { get; set; }
    public double Lambda { get; set; }
    public double Mu { get; set; }
    public double? SigmaS2 { get; set; }
    public double? SigmaA2 { get; set; }
    public string ModelType { get; set; } = "MM1";

    public void Validate()
    {
        if (Lambda <= 0 || double.IsNaN(Lambda) || double.IsInfinity(Lambda))
            throw new ArgumentOutOfRangeException(nameof(Lambda), "Lambda must be > 0.");

        if (Mu <= 0 || double.IsNaN(Mu) || double.IsInfinity(Mu))
            throw new ArgumentOutOfRangeException(nameof(Mu), "Mu must be > 0.");

        var rho = Lambda / Mu;
        if (rho >= 1.0)
            throw new InvalidOperationException($"System unstable: rho={rho:F4} >= 1.0");
    }
}
