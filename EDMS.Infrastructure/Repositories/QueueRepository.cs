using EDMS.Core.Domain;
using EDMS.Core.Interfaces;
using EDMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EDMS.Infrastructure.Repositories;

public class QueueRepository : IQueueRepository
{
    private readonly AppDbContext _db;

    public QueueRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<QueueModel?> GetModelByTypeAsync(string modelType) =>
        _db.QueueModels.FirstOrDefaultAsync(x => x.ModelType == modelType && x.IsActive);

    public Task<QueueSnapshot?> GetLatestAsync() =>
        _db.QueueSnapshots.OrderByDescending(x => x.ComputedAtUtc).FirstOrDefaultAsync();

    public async Task<IEnumerable<QueueSnapshot>> GetHistoryAsync(int hours)
    {
        var since = DateTime.UtcNow.AddHours(-Math.Abs(hours));
        return await _db.QueueSnapshots
            .Where(x => x.ComputedAtUtc >= since)
            .OrderByDescending(x => x.ComputedAtUtc)
            .ToListAsync();
    }

    public async Task<QueueSnapshot> SaveSnapshotAsync(QueueSnapshot snapshot)
    {
        _db.QueueSnapshots.Add(snapshot);
        await _db.SaveChangesAsync();
        return snapshot;
    }
}
