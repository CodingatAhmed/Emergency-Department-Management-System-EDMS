using EDMS.Core.Domain;
using EDMS.Core.Interfaces;
using EDMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EDMS.Infrastructure.Repositories;

public class PatientRepository : IPatientRepository
{
    private readonly AppDbContext _db;

    public PatientRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Patient> CreateAsync(Patient patient)
    {
        _db.Patients.Add(patient);
        await _db.SaveChangesAsync();
        return patient;
    }

    public Task<Patient?> GetByIdAsync(Guid patientId) =>
        _db.Patients.FirstOrDefaultAsync(x => x.PatientId == patientId && x.IsActive);

    public Task<Patient?> GetByMrnAsync(string mrn) =>
        _db.Patients.FirstOrDefaultAsync(x => x.MRN == mrn && x.IsActive);

    public async Task<IEnumerable<Patient>> SearchAsync(string term)
    {
        term = term.Trim().ToLowerInvariant();
        return await _db.Patients
            .Where(x => x.IsActive && (x.MRN.ToLower().Contains(term) ||
                                       x.FirstName.ToLower().Contains(term) ||
                                       x.LastName.ToLower().Contains(term)))
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync();
    }

    public async Task<Patient> UpdateAsync(Patient patient)
    {
        _db.Patients.Update(patient);
        await _db.SaveChangesAsync();
        return patient;
    }
}
