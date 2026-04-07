using EDMS.Core.Domain;
using EDMS.Core.Interfaces;
using EDMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EDMS.Infrastructure.Repositories;

public class ResourceRepository : IResourceRepository
{
    private readonly AppDbContext _db;

    public ResourceRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Resource> CreateAsync(Resource resource)
    {
        _db.Resources.Add(resource);
        await _db.SaveChangesAsync();
        return resource;
    }

    public async Task<IEnumerable<Resource>> GetAllAsync() =>
        await _db.Resources.OrderBy(x => x.Name).ToListAsync();

    public Task<Resource?> GetByIdAsync(Guid id) =>
        _db.Resources.FirstOrDefaultAsync(x => x.ResourceId == id);

    public async Task<Resource> UpdateAsync(Resource resource)
    {
        _db.Resources.Update(resource);
        await _db.SaveChangesAsync();
        return resource;
    }
}
