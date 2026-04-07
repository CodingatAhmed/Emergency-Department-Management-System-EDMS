namespace EDMS.Core.Queue;

public class MM1Engine : IQueueEngine
{
    public string ModelType => "MM1";

    public QueueMetrics Compute(QueueRequest request)
    {
        request.Validate();
        var rho = request.Lambda / request.Mu;
        var lq = (rho * rho) / (1 - rho);
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
            SlaBreach = wq > 0.5,
            CapacityWarning = rho > 0.85,
            StabilityViolation = false,
            Interpretation = $"M/M/1 with lambda={request.Lambda:F2}, mu={request.Mu:F2}: average queue wait is {wq * 60:F1} min."
        };
    }
}
