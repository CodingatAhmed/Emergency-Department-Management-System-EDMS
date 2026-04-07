using EDMS.Core.Domain;

namespace EDMS.Core.Interfaces;

public interface IPatientRepository
{
    Task<Patient?> GetByMrnAsync(string mrn);
    Task<Patient?> GetByIdAsync(Guid patientId);
    Task<Patient> CreateAsync(Patient patient);
    Task<Patient> UpdateAsync(Patient patient);
    Task<IEnumerable<Patient>> SearchAsync(string term);
}

public interface IEncounterRepository
{
    Task<Encounter> CreateAsync(Encounter encounter);
    Task<Encounter?> GetByIdAsync(Guid encounterId);
    Task<IEnumerable<Encounter>> GetActiveAsync();
    Task<Encounter> UpdateStateAsync(Guid id, EncounterState newState);
    Task<Encounter> UpdateDispositionAsync(Guid id, FinalDisposition disposition);
    Task<IEnumerable<Encounter>> GetByPatientAsync(Guid patientId);
    Task<int> CountWaitingAsync();
}

public interface IQueueRepository
{
    Task<QueueSnapshot> SaveSnapshotAsync(QueueSnapshot snapshot);
    Task<QueueSnapshot?> GetLatestAsync();
    Task<IEnumerable<QueueSnapshot>> GetHistoryAsync(int hours);
    Task<QueueModel?> GetModelByTypeAsync(string modelType);
}

public interface IStaffUserRepository
{
    Task<StaffUser?> GetByUsernameAsync(string username);
    Task<StaffUser?> GetByIdAsync(Guid userId);
    Task<StaffUser> CreateAsync(StaffUser user);
    Task UpdateLastLoginAsync(Guid userId);
}

public interface IAllergyRegistryRepository
{
    Task<IEnumerable<AllergyRegistry>> GetAllAsync();
}

public interface IResourceRepository
{
    Task<IEnumerable<Resource>> GetAllAsync();
    Task<Resource?> GetByIdAsync(Guid id);
    Task<Resource> CreateAsync(Resource resource);
    Task<Resource> UpdateAsync(Resource resource);
}

public interface IAppointmentRepository
{
    Task<Appointment> CreateAsync(Appointment appointment);
    Task<IEnumerable<Appointment>> GetByDateAsync(DateTime date);
    Task<Appointment?> GetByIdAsync(Guid id);
    Task<Appointment?> GetByQrCodeAsync(string qrCode);
    Task<Appointment> UpdateAsync(Appointment appointment);
}
