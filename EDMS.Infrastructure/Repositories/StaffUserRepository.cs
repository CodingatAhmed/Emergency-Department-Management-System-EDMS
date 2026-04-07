using EDMS.Core.Domain;
using EDMS.Core.Interfaces;
using EDMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EDMS.Infrastructure.Repositories;

public class StaffUserRepository : IStaffUserRepository
{
    private readonly AppDbContext _db;

    public StaffUserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<StaffUser> CreateAsync(StaffUser user)
    {
        _db.StaffUsers.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public Task<StaffUser?> GetByIdAsync(Guid userId) =>
        _db.StaffUsers.FirstOrDefaultAsync(x => x.UserId == userId && x.IsActive);

    public Task<StaffUser?> GetByUsernameAsync(string username) =>
        _db.StaffUsers.FirstOrDefaultAsync(x => x.Username == username && x.IsActive);

    public async Task UpdateLastLoginAsync(Guid userId)
    {
        var user = await _db.StaffUsers.FirstAsync(x => x.UserId == userId);
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
