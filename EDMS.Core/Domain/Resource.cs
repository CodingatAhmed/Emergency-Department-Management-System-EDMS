namespace EDMS.Core.Domain;

public class Resource
{
    public Guid ResourceId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string ResourceType { get; set; } = "Room";
    public string Status { get; set; } = "Available";
    public string? Department { get; set; }
    public DateTime? OccupiedSince { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
