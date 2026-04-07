namespace EDMS.Core.Domain;

public class AllergyRegistry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string AllergenName { get; set; } = string.Empty;
    public string[] ConflictingDrugs { get; set; } = [];
    public string Severity { get; set; } = "Moderate";
    public string Category { get; set; } = "General";
}
