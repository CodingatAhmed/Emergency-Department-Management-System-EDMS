using System.Text.Json;
using EDMS.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EDMS.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Encounter> Encounters => Set<Encounter>();
    public DbSet<QueueModel> QueueModels => Set<QueueModel>();
    public DbSet<QueueSnapshot> QueueSnapshots => Set<QueueSnapshot>();
    public DbSet<QueueAlert> QueueAlerts => Set<QueueAlert>();
    public DbSet<StaffUser> StaffUsers => Set<StaffUser>();
    public DbSet<AllergyRegistry> AllergyRegistries => Set<AllergyRegistry>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. JSON Converters for Lists
        var listToJson = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

        var listComparer = new ValueComparer<List<string>>(
            (a, b) => (a ?? new List<string>()).SequenceEqual(b ?? new List<string>()),
            c => c.Aggregate(0, (h, v) => HashCode.Combine(h, v.GetHashCode())),
            c => c.ToList());

        // 2. GLOBAL DATETIME CONVERTER (Fixes the PostgreSQL UTC Error)
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(dateTimeConverter);
                }
            }
        }

        // 3. Entity Configurations
        modelBuilder.Entity<Patient>(e =>
        {
            e.HasKey(x => x.PatientId);
            e.HasIndex(x => x.MRN).IsUnique();
            e.Property(x => x.AllergiesList).HasConversion(listToJson).Metadata.SetValueComparer(listComparer);
            e.Property(x => x.ActiveProblems).HasConversion(listToJson).Metadata.SetValueComparer(listComparer);
        });

        modelBuilder.Entity<Encounter>(e =>
        {
            e.HasKey(x => x.EncounterId);
            e.HasIndex(x => x.PatientId);
            e.HasIndex(x => x.ArrivalTime);
        });

        modelBuilder.Entity<QueueModel>(e =>
        {
            e.HasKey(x => x.ModelId);
            e.HasIndex(x => x.ModelType).IsUnique();
        });

        modelBuilder.Entity<QueueSnapshot>(e =>
        {
            e.HasKey(x => x.SnapshotId);
            e.HasIndex(x => new { x.ModelId, x.ComputedAtUtc });
        });

        modelBuilder.Entity<QueueAlert>(e =>
        {
            e.HasKey(x => x.AlertId);
            e.HasIndex(x => new { x.AlertType, x.TriggeredAt });
        });

        modelBuilder.Entity<StaffUser>(e =>
        {
            e.HasKey(x => x.UserId);
            e.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<AllergyRegistry>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.AllergenName);
        });

        modelBuilder.Entity<Resource>(e =>
        {
            e.HasKey(x => x.ResourceId);
            e.HasIndex(x => x.Status);
            e.HasIndex(x => x.ResourceType);
        });

        modelBuilder.Entity<Appointment>(e =>
        {
            e.HasKey(x => x.AppointmentId);
            e.HasIndex(x => x.PatientId);
            e.HasIndex(x => x.ScheduledTime);
            e.HasIndex(x => x.QRCode).IsUnique();
        });
    }
}