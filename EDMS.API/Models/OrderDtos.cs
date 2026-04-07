namespace EDMS.API.Models;

public record VerifyOrderRequest(Guid PatientId, string DrugName);

public class AllergyCheckResult
{
    public bool HasConflict { get; set; }
    public string? Severity { get; set; }
    public string? ConflictDetail { get; set; }
    public string? AllergenName { get; set; }
}
