using Microsoft.AspNetCore.SignalR.Client;

namespace EDMS.Console.ApiClient;

public class SignalRListener
{
    private HubConnection? _hubConnection;

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public async Task StartAsync(string hubUrl, string token)
    {
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token)!;
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<object>("QueueUpdate", e => Print("[LIVE] QueueUpdate", e));
        _hubConnection.On<object>("SLABreach", e => Print("[WARN] SLABreach", e));
        _hubConnection.On<object>("AllergyAlert", e => Print("[ALERT] AllergyAlert", e));
        _hubConnection.On<object>("ResourceChange", e => Print("[RESOURCE] ResourceChange", e));
        _hubConnection.On<object>("PatientCalled", e => Print("[CALLED] PatientCalled", e));
        _hubConnection.On<object>("CapacityWarning", e => Print("[WARN] CapacityWarning", e));

        await _hubConnection.StartAsync();
    }

    public async Task StopAsync()
    {
        if (_hubConnection is not null)
            await _hubConnection.DisposeAsync();
    }

    private static void Print(string title, object payload)
    {
        System.Console.WriteLine($"{DateTime.Now:HH:mm:ss} {title}: {payload}");
    }
}
