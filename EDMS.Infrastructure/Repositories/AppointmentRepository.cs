using EDMS.Core.Domain;
using EDMS.Core.Interfaces;
using EDMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EDMS.Infrastructure.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly AppDbContext _db;

    public AppointmentRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Appointment> CreateAsync(Appointment appointment)
    {
        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();
        return appointment;
    }

    public async Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);
        return await _db.Appointments
            .Where(x => x.ScheduledTime >= dayStart && x.ScheduledTime < dayEnd)
            .OrderBy(x => x.ScheduledTime)
            .ToListAsync();
    }

    public Task<Appointment?> GetByIdAsync(Guid id) =>
        _db.Appointments.FirstOrDefaultAsync(x => x.AppointmentId == id);

    public Task<Appointment?> GetByQrCodeAsync(string qrCode) =>
        _db.Appointments.FirstOrDefaultAsync(x => x.QRCode == qrCode);

    public async Task<Appointment> UpdateAsync(Appointment appointment)
    {
        _db.Appointments.Update(appointment);
        await _db.SaveChangesAsync();
        return appointment;
    }
}
