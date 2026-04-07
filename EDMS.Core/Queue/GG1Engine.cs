namespace EDMS.Core.Queue;

public class GG1Engine : IQueueEngine
{
    public string ModelType => "GG1";

    public QueueMetrics Compute(QueueRequest request)
    {
        request.Validate();
        if (request.SigmaS2 is null || request.SigmaS2 <= 0 || request.SigmaA2 is null || request.SigmaA2 <= 0)
            throw new ArgumentException("G/G/1 requires SigmaS2 and SigmaA2 > 0.");

        var rho = request.Lambda / request.Mu;
        var caSquared = request.SigmaA2.Value * request.Lambda * request.Lambda;
        var csSquared = request.SigmaS2.Value * request.Mu * request.Mu;
        var wq = (rho / (1 - rho)) * ((caSquared + csSquared) / 2) * (1 / request.Mu);
        var lq = request.Lambda * wq;
        var w = wq + (1 / request.Mu);
        var l = request.Lambda * w;
        var cv = Math.Sqrt(csSquared);

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
            CaSquared = caSquared,
            CsSquared = csSquared,
            SlaBreach = wq > 0.5,
            CapacityWarning = rho > 0.85,
            StabilityViolation = false,
            Interpretation = $"G/G/1 Kingman approximation: higher variability increases queueing delay (Wq={wq * 60:F1} min)."
        };
    }
}
