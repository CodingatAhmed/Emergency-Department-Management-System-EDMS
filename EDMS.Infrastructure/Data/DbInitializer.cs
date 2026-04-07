using EDMS.Core.Domain;
using EDMS.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

namespace EDMS.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext db, PasswordService passwordService)
    {
        await db.Database.EnsureCreatedAsync();

        if (!await db.QueueModels.AnyAsync())
        {
            db.QueueModels.AddRange(
                new QueueModel { ModelType = "MM1", ArrivalDistribution = "Poisson", ServiceDistribution = "Exponential", IsActive = true },
                new QueueModel { ModelType = "MG1", ArrivalDistribution = "Poisson", ServiceDistribution = "General", DefaultSigmaS2 = 0.01, IsActive = true },
                new QueueModel { ModelType = "GG1", ArrivalDistribution = "General", ServiceDistribution = "General", DefaultSigmaS2 = 0.01, DefaultSigmaA2 = 0.01, IsActive = true }
            );
        }

        if (!await db.StaffUsers.AnyAsync())
        {
            db.StaffUsers.AddRange(
                new StaffUser { Username = "admin", FullName = "System Admin", Role = StaffRole.Admin, PasswordHash = passwordService.HashPassword("Admin@123"), IsActive = true },
                new StaffUser { Username = "doctor1", FullName = "Doctor One", Role = StaffRole.Doctor, PasswordHash = passwordService.HashPassword("Doctor@123"), IsActive = true },
                new StaffUser { Username = "nurse1", FullName = "Nurse One", Role = StaffRole.Nurse, PasswordHash = passwordService.HashPassword("Nurse@123"), IsActive = true }
            );
        }

        if (!await db.AllergyRegistries.AnyAsync())
        {
            db.AllergyRegistries.AddRange(
                new AllergyRegistry { AllergenName = "Penicillin", ConflictingDrugs = ["Amoxicillin", "Ampicillin"], Severity = "Severe", Category = "Antibiotic" },
                new AllergyRegistry { AllergenName = "Aspirin", ConflictingDrugs = ["Aspirin"], Severity = "Moderate", Category = "NSAID" },
                new AllergyRegistry { AllergenName = "Ibuprofen", ConflictingDrugs = ["Ibuprofen"], Severity = "Moderate", Category = "NSAID" },
                new AllergyRegistry { AllergenName = "Morphine", ConflictingDrugs = ["Morphine"], Severity = "Severe", Category = "Opioid" },
                new AllergyRegistry { AllergenName = "Sulfa", ConflictingDrugs = ["Sulfamethoxazole"], Severity = "Severe", Category = "Antibiotic" }
            );
        }

        if (!await db.Resources.AnyAsync())
        {
            db.Resources.AddRange(
                new Resource { Name = "Room-A", ResourceType = "Room", Status = "Available", Department = "Emergency" },
                new Resource { Name = "Room-B", ResourceType = "Room", Status = "Available", Department = "Emergency" },
                new Resource { Name = "Room-C", ResourceType = "Room", Status = "Available", Department = "Outpatient" },
                new Resource { Name = "ECG", ResourceType = "Equipment", Status = "Available", Department = "Diagnostics" },
                new Resource { Name = "X-Ray", ResourceType = "Equipment", Status = "Available", Department = "Diagnostics" }
            );
        }

        await db.SaveChangesAsync();
    }
}
