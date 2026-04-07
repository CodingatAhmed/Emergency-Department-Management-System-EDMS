using EDMS.Core.Domain;
using EDMS.Core.Interfaces;
using EDMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EDMS.Infrastructure.Repositories;

public class EncounterRepository : IEncounterRepository
{
    private readonly AppDbContext _db;

    public EncounterRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Encounter> CreateAsync(Encounter encounter)
    {
        _db.Encounters.Add(encounter);
        await _db.SaveChangesAsync();
        return encounter;
    }

    public Task<int> CountWaitingAsync() =>
        _db.Encounters.CountAsync(x => x.CurrentState == EncounterState.Waiting);

    public async Task<IEnumerable<Encounter>> GetActiveAsync() =>
        await _db.Encounters
            .Where(x => x.CurrentState == EncounterState.Waiting || x.CurrentState == EncounterState.InService)
            .OrderBy(x => x.ArrivalTime)
            .ToListAsync();

    public Task<Encounter?> GetByIdAsync(Guid encounterId) =>
        _db.Encounters.FirstOrDefaultAsync(x => x.EncounterId == encounterId);

    public async Task<IEnumerable<Encounter>> GetByPatientAsync(Guid patientId) =>
        await _db.Encounters
            .Where(x => x.PatientId == patientId)
            .OrderByDescending(x => x.ArrivalTime)
            .ToListAsync();

    public async Task<Encounter> UpdateDispositionAsync(Guid id, FinalDisposition disposition)
    {
        var encounter = await _db.Encounters.FirstAsync(x => x.EncounterId == id);
        encounter.FinalDisposition = disposition;
        encounter.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return encounter;
    }

    public async Task<Encounter> UpdateStateAsync(Guid id, EncounterState newState)
    {
        var encounter = await _db.Encounters.FirstAsync(x => x.EncounterId == id);
        var current = encounter.CurrentState;

        var allowed = current switch
        {
            EncounterState.Waiting => newState is EncounterState.InService or EncounterState.Cancelled,
            EncounterState.InService => newState is EncounterState.Completed or EncounterState.Cancelled,
            EncounterState.Completed => newState is EncounterState.Discharged or EncounterState.Admitted,
            EncounterState.Discharged => false,
            EncounterState.Admitted => false,
            EncounterState.Cancelled => false,
            _ => false
        };

        if (!allowed)
            throw new InvalidOperationException($"Invalid state transition: {current} -> {newState}");

        encounter.CurrentState = newState;
        if (newState == EncounterState.InService)
            encounter.ServiceStartTime = DateTime.UtcNow;
        if (newState is EncounterState.Discharged or EncounterState.Admitted)
            encounter.DischargeTime = DateTime.UtcNow;

        encounter.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return encounter;
    }
}
