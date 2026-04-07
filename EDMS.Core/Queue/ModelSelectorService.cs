namespace EDMS.Core.Queue;

public class ModelSelectorService
{
    private readonly Dictionary<string, IQueueEngine> _engines;

    public ModelSelectorService(IEnumerable<IQueueEngine> engines)
    {
        _engines = engines.ToDictionary(e => e.ModelType.ToUpperInvariant(), e => e);
    }

    public QueueMetrics Compute(QueueRequest request)
    {
        var key = (request.ModelType ?? "MM1").ToUpperInvariant();
        if (!_engines.TryGetValue(key, out var engine))
            throw new ArgumentException($"Unknown model type: {request.ModelType}");

        return engine.Compute(request);
    }
}
