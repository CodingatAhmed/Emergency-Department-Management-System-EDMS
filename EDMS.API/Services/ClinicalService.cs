using EDMS.API.Hubs;
using EDMS.API.Models;
using EDMS.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace EDMS.API.Services;

public class ClinicalService
{
    private readonly IPatientRepository _patients;
    private readonly IAllergyRegistryRepository _allergyRegistry;
    private readonly IHubContext<QueueHub> _hub;

    public ClinicalService(
        IPatientRepository patients,
        IAllergyRegistryRepository allergyRegistry,
        IHubContext<QueueHub> hub)
    {
        _patients = patients;
        _allergyRegistry = allergyRegistry;
        _hub = hub;
    }

    public async Task<AllergyCheckResult> CheckConflictAsync(Guid patientId, string drugName)
    {
        var patient = await _patients.GetByIdAsync(patientId) ?? throw new InvalidOperationException("Patient not found.");
        var registries = await _allergyRegistry.GetAllAsync();
        var drug = drugName.Trim().ToLowerInvariant();

        foreach (var allergen in patient.AllergiesList)
        {
            var matches = registries.Where(x => x.AllergenName.Equals(allergen, StringComparison.OrdinalIgnoreCase));
            foreach (var entry in matches)
            {
                if (entry.ConflictingDrugs.Any(d => d.Equals(drugName, StringComparison.OrdinalIgnoreCase) || d.ToLowerInvariant().Contains(drug)))
                {
                    var result = new AllergyCheckResult
                    {
                        HasConflict = true,
                        Severity = entry.Severity,
                        AllergenName = entry.AllergenName,
                        ConflictDetail = $"{drugName} conflicts with {entry.AllergenName} allergy."
                    };

                    await _hub.Clients.All.SendAsync("AllergyAlert", new
                    {
                        PatientId = patient.PatientId,
                        PatientName = $"{patient.FirstName} {patient.LastName}",
                        DrugName = drugName,
                        Severity = entry.Severity,
                        ConflictDetail = result.ConflictDetail
                    });

                    return result;
                }
            }
        }

        return new AllergyCheckResult { HasConflict = false };
    }
}
