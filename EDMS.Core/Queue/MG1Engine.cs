namespace EDMS.Core.Queue;

public class MG1Engine : IQueueEngine
{
    public string ModelType => "MG1";

    public QueueMetrics Compute(QueueRequest request)
    {
        request.Validate();
        if (request.SigmaS2 is null || request.SigmaS2 <= 0)
            throw new ArgumentException("M/G/1 requires service variance (SigmaS2) > 0.");

        var rho = request.Lambda / request.Mu;
        var cv = Math.Sqrt(request.SigmaS2.Value) * request.Mu;
        var csSquared = cv * cv;
        var lq = (rho * rho * (1 + csSquared)) / (2 * (1 - rho));
        var wq = lq / request.Lambda;
        var l = lq + rho;
        var w = wq + (1 / request.Mu);

        return new QueueMetrics
        {
            ModelUsed = ModelType,
            Lambda = request.Lambda,
            Mu = request.Mu,
            Rho = rho,
            Lq = lq,
            Wq = wq,
            L = l,
            W = w,
            Cv = cv,
            CsSquared = csSquared,
            SlaBreach = wq > 0.5,
            CapacityWarning = rho > 0.85,
            StabilityViolation = false,
            Interpretation = $"M/G/1 P-K result: service variability (Cs^2={csSquared:F3}) yields Wq={wq * 60:F1} min."
        };
    }
}
