using EDMS.API.Hubs;
using EDMS.Core.Domain;
using EDMS.Core.Queue;
using Microsoft.AspNetCore.SignalR;

namespace EDMS.API.Services;

public class NotificationService
{
    private readonly IHubContext<QueueHub> _hub;

    public NotificationService(IHubContext<QueueHub> hub)
    {
        _hub = hub;
    }

    public Task NotifyQueueUpdateAsync(QueueMetrics metrics) =>
        _hub.Clients.All.SendAsync("QueueUpdate", metrics);

    public Task NotifySlaBreachAsync(Guid? encounterId, QueueMetrics metrics) =>
        _hub.Clients.All.SendAsync("SLABreach", new
        {
            EncounterId = encounterId,
            WqHours = metrics.Wq,
            ModelType = metrics.ModelUsed
        });

    public Task NotifyCapacityWarningAsync(QueueMetrics metrics) =>
        _hub.Clients.All.SendAsync("CapacityWarning", new
        {
            Rho = metrics.Rho,
            ModelType = metrics.ModelUsed
        });

    public Task NotifyPatientCalledAsync(Encounter encounter) =>
        _hub.Clients.All.SendAsync("PatientCalled", new
        {
            EncounterId = encounter.EncounterId,
            RoomId = encounter.AssignedRoom,
            CalledAt = DateTime.UtcNow
        });

    public Task NotifyEncounterStateChangeAsync(Encounter encounter, EncounterState oldState) =>
        _hub.Clients.All.SendAsync("EncounterStateChange", new
        {
            EncounterId = encounter.EncounterId,
            OldState = oldState.ToString(),
            NewState = encounter.CurrentState.ToString(),
            Timestamp = DateTime.UtcNow
        });

    public Task NotifyNewPatientArrivalAsync(Encounter encounter) =>
        _hub.Clients.All.SendAsync("NewPatientArrival", new
        {
            EncounterId = encounter.EncounterId,
            TriageCategory = encounter.TriageCategory?.ToString(),
            EstimatedWaitMin = encounter.EstimatedWaitMin,
            ArrivalTime = encounter.ArrivalTime
        });

    public Task NotifyResourceChangeAsync(Resource resource, string oldStatus) =>
        _hub.Clients.All.SendAsync("ResourceChange", new
        {
            ResourceId = resource.ResourceId,
            ResourceName = resource.Name,
            OldStatus = oldStatus,
            NewStatus = resource.Status,
            UpdatedAt = resource.UpdatedAtUtc
        });
}
