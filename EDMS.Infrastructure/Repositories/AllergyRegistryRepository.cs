using EDMS.Core.Domain;
using EDMS.Core.Interfaces;
using EDMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EDMS.Infrastructure.Repositories;

public class AllergyRegistryRepository : IAllergyRegistryRepository
{
    private readonly AppDbContext _db;

    public AllergyRegistryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<AllergyRegistry>> GetAllAsync()
    {
        return await _db.AllergyRegistries.OrderBy(x => x.AllergenName).ToListAsync();
    }
}
