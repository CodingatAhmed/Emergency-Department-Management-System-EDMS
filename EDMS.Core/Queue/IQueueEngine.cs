namespace EDMS.Core.Queue;

public interface IQueueEngine
{
    string ModelType { get; }
    QueueMetrics Compute(QueueRequest request);
}
