namespace EDMS.Core.Domain;

public class QueueModel
{
    public Guid ModelId { get; set; } = Guid.NewGuid();
    public string ModelType { get; set; } = "MM1";
    public string ArrivalDistribution { get; set; } = "Poisson";
    public string ServiceDistribution { get; set; } = "Exponential";
    public double? DefaultSigmaS2 { get; set; }
    public double? DefaultSigmaA2 { get; set; }
    public bool IsActive { get; set; } = true;
}
