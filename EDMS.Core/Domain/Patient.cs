namespace EDMS.Core.Domain;

public class Patient
{
    public Guid PatientId { get; set; } = Guid.NewGuid();
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string MRN { get; set; } = string.Empty;
    public string? ContactNumber { get; set; }
    public string? Email { get; set; }
    public string Gender { get; set; } = "Other";
    public List<string> AllergiesList { get; set; } = [];
    public List<string> ActiveProblems { get; set; } = [];
    public bool IsActive { get; set; } = true;
    public bool IsTemporary { get; set; }
    public bool IsJohnDoe { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
