using EDMS.Console.ApiClient;
using EDMS.Console.Models;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

// Essential switch for PostgreSQL timestamp compatibility
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var config = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var apiBaseUrl = config["Api:BaseUrl"] ?? "https://localhost:5001";
var hubUrl = config["SignalR:HubUrl"] ?? "https://localhost:5001/hubs/queue";

var api = new ApiHttpClient(apiBaseUrl);
var signalR = new SignalRListener();

AnsiConsole.MarkupLine("[bold green]EDMS Console Client[/]");
AnsiConsole.MarkupLine($"API: [grey]{apiBaseUrl}[/]");
AnsiConsole.MarkupLine($"Hub: [grey]{hubUrl}[/]");

var loginOk = await LoginAsync();
if (!loginOk)
{
    AnsiConsole.MarkupLine("[red]Login failed. Exiting.[/]");
    return;
}

try
{
    if (!string.IsNullOrWhiteSpace(api.CurrentToken))
    {
        await signalR.StartAsync(hubUrl, api.CurrentToken);
        AnsiConsole.MarkupLine("[green]SignalR connected.[/]");
    }
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[yellow]SignalR not connected: {ex.Message}[/]");
}

await MainLoopAsync();
await signalR.StopAsync();

async Task<bool> LoginAsync()
{
    var username = AnsiConsole.Ask<string>("Username:");
    var password = AnsiConsole.Prompt(
        new TextPrompt<string>("Password:")
            .PromptStyle("red")
            .Secret());

    var response = await api.PostAsync<LoginRequest, ApiEnvelope<LoginResponse>>(
        "/api/auth/login",
        new LoginRequest { Username = username, Password = password });

    if (response?.Data is null || !response.Success)
        return false;

    api.SetToken(response.Data.Token);
    AnsiConsole.MarkupLine($"[green]Logged in as {username} ({response.Data.Role})[/]");
    return true;
}

async Task MainLoopAsync()
{
    while (true)
    {
        var option = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Main Menu[/]")
                .AddChoices(
                    "Patient: Register",
                    "Patient: Search",
                    "Encounter: Create",
                    "Encounter: Active Queue",
                    "Encounter: Call Next",
                    "Clinical: Verify Order",
                    "Appointment: Schedule",
                    "Appointment: View By Date",
                    "Appointment: Validate QR",
                    "Alerts: View Open",
                    "Alerts: Resolve",
                    "Reports: Daily Summary",
                    "Reports: Queue Performance",
                    "Reports: Triage Breakdown",
                    "Reports: Model Comparison",
                    "Reports: SLA Compliance",
                    "Queue: Compute Metrics",
                    "Resource: View Availability",
                    "Resource: Update Status",
                    "Exit"));

        if (option == "Exit")
            break;

        switch (option)
        {
            case "Patient: Register":
                await RegisterPatientAsync();
                break;
            case "Patient: Search":
                await SearchPatientAsync();
                break;
            case "Encounter: Create":
                await CreateEncounterAsync();
                break;
            case "Encounter: Active Queue":
                await ShowActiveQueueAsync();
                break;
            case "Encounter: Call Next":
                await CallNextAsync();
                break;
            case "Clinical: Verify Order":
                await VerifyOrderAsync();
                break;
            case "Appointment: Schedule":
                await ScheduleAppointmentAsync();
                break;
            case "Appointment: View By Date":
                await ViewAppointmentsByDateAsync();
                break;
            case "Appointment: Validate QR":
                await ValidateQrAsync();
                break;
            case "Alerts: View Open":
                await ViewOpenAlertsAsync();
                break;
            case "Alerts: Resolve":
                await ResolveAlertAsync();
                break;
            case "Reports: Daily Summary":
                await DailySummaryAsync();
                break;
            case "Reports: Queue Performance":
                await QueuePerformanceAsync();
                break;
            case "Reports: Triage Breakdown":
                await TriageBreakdownAsync();
                break;
            case "Reports: Model Comparison":
                await ModelComparisonAsync();
                break;
            case "Reports: SLA Compliance":
                await SlaComplianceAsync();
                break;
            case "Queue: Compute Metrics":
                await ComputeQueueAsync();
                break;
            case "Resource: View Availability":
                await ShowResourcesAsync();
                break;
            case "Resource: Update Status":
                await UpdateResourceAsync();
                break;
        }
    }
}

async Task ComputeQueueAsync()
{
    var model = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Select model")
            .AddChoices("MM1", "MG1", "GG1"));

    var lambda = AnsiConsole.Ask<double>("Lambda (arrivals/hr):");
    var mu = AnsiConsole.Ask<double>("Mu (service/hr):");

    double? sigmaS2 = null;
    double? sigmaA2 = null;

    if (model is "MG1" or "GG1")
        sigmaS2 = AnsiConsole.Ask<double>("SigmaS2:");
    if (model == "GG1")
        sigmaA2 = AnsiConsole.Ask<double>("SigmaA2:");

    var req = new QueueRequest
    {
        ModelType = model,
        Lambda = lambda,
        Mu = mu,
        SigmaS2 = sigmaS2,
        SigmaA2 = sigmaA2
    };

    var result = await api.PostAsync<QueueRequest, ApiEnvelope<QueueMetrics>>("/api/queue/compute", req);
    if (result?.Data is null)
        return;

    var m = result.Data;
    var table = new Table().Border(TableBorder.Rounded);
    table.AddColumn("Metric");
    table.AddColumn("Value");
    table.AddRow("Model", m.ModelUsed);
    table.AddRow("rho", m.Rho.ToString("F4"));
    table.AddRow("Lq", m.Lq.ToString("F4"));
    table.AddRow("Wq (min)", (m.Wq * 60).ToString("F2"));
    table.AddRow("L", m.L.ToString("F4"));
    table.AddRow("W (min)", (m.W * 60).ToString("F2"));
    AnsiConsole.Write(table);
    AnsiConsole.MarkupLine($"[grey]{m.Interpretation}[/]");
}

async Task ShowResourcesAsync()
{
    var response = await api.GetAsync<ApiEnvelope<List<ResourceDto>>>("/api/resource/availability");
    if (response?.Data is null)
        return;

    var table = new Table().Border(TableBorder.Simple);
    table.AddColumn("Id");
    table.AddColumn("Name");
    table.AddColumn("Type");
    table.AddColumn("Status");
    table.AddColumn("Department");

    foreach (var r in response.Data)
        table.AddRow(r.ResourceId.ToString(), r.Name, r.ResourceType, r.Status, r.Department ?? "-");

    AnsiConsole.Write(table);
}

async Task UpdateResourceAsync()
{
    var response = await api.GetAsync<ApiEnvelope<List<ResourceDto>>>("/api/resource/availability");
    if (response?.Data is null || response.Data.Count == 0)
        return;

    var selected = AnsiConsole.Prompt(
        new SelectionPrompt<ResourceDto>()
            .Title("Select resource")
            .UseConverter(r => $"{r.Name} ({r.Status})")
            .AddChoices(response.Data));

    var newStatus = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("New status")
            .AddChoices("Available", "Occupied", "Maintenance", "Reserved"));

    var result = await api.PutAsync<string, ApiEnvelope<ResourceDto>>($"/api/resource/{selected.ResourceId}/status", newStatus);
    if (result?.Data is not null)
        AnsiConsole.MarkupLine($"[green]Updated {result.Data.Name} to {result.Data.Status}[/]");
}

async Task RegisterPatientAsync()
{
    var firstName = AnsiConsole.Ask<string>("First name:");
    var lastName = AnsiConsole.Ask<string>("Last name:");
    var dobInput = AnsiConsole.Ask<DateTime>("DOB (yyyy-MM-dd):");

    var req = new RegisterPatientRequest
    {
        FirstName = firstName,
        LastName = lastName,
        // Convert to UTC to ensure compatibility with PostgreSQL
        DateOfBirth = DateTime.SpecifyKind(dobInput, DateTimeKind.Utc),
        Gender = AnsiConsole.Ask<string>("Gender (M/F/Other):"),
        ContactNumber = AnsiConsole.Ask<string>("Contact number (optional):"),
        Email = AnsiConsole.Ask<string>("Email (optional):")
    };

    var allergies = AnsiConsole.Ask<string>("Allergies (comma separated, optional):");
    if (!string.IsNullOrWhiteSpace(allergies))
        req.Allergies = allergies.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

    var result = await api.PostAsync<RegisterPatientRequest, ApiEnvelope<PatientDto>>("/api/patient/register", req);
    if (result?.Data is not null)
        AnsiConsole.MarkupLine($"[green]Patient registered: {result.Data.MRN} ({result.Data.FirstName} {result.Data.LastName})[/]");
}

async Task SearchPatientAsync()
{
    var q = AnsiConsole.Ask<string>("Search term (MRN/name):");
    var result = await api.GetAsync<ApiEnvelope<List<PatientDto>>>($"/api/patient/search?q={Uri.EscapeDataString(q)}");
    if (result?.Data is null || result.Data.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]No patients found.[/]");
        return;
    }

    var table = new Table();
    table.AddColumn("PatientId");
    table.AddColumn("MRN");
    table.AddColumn("Name");
    table.AddColumn("DOB");
    foreach (var p in result.Data)
        table.AddRow(p.PatientId.ToString(), p.MRN, $"{p.FirstName} {p.LastName}", p.DateOfBirth.ToString("yyyy-MM-dd"));
    AnsiConsole.Write(table);
}

async Task CreateEncounterAsync()
{
    var patientId = AnsiConsole.Ask<Guid>("Patient Id:");
    var type = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Encounter Type").AddChoices("WalkIn", "Scheduled"));
    var triage = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Triage").AddChoices("UrgentCare", "MinorProcedure", "Diagnostics", "TherapySession", "Routine"));

    var req = new CreateEncounterRequest
    {
        PatientId = patientId,
        EncounterType = type,
        TriageCategory = triage
    };

    var result = await api.PostAsync<CreateEncounterRequest, ApiEnvelope<EncounterDto>>("/api/encounter", req);
    if (result?.Data is not null)
        AnsiConsole.MarkupLine($"[green]Encounter created: {result.Data.EncounterId}[/]");
}

async Task ShowActiveQueueAsync()
{
    var result = await api.GetAsync<ApiEnvelope<List<EncounterDto>>>("/api/encounter/active");
    if (result?.Data is null || result.Data.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]No active encounters.[/]");
        return;
    }

    var table = new Table();
    table.AddColumn("Encounter");
    table.AddColumn("Patient");
    table.AddColumn("Type");
    table.AddColumn("Triage");
    table.AddColumn("State");
    table.AddColumn("Arrival");
    foreach (var e in result.Data.OrderBy(x => x.ArrivalTime))
        table.AddRow(e.EncounterId.ToString(), e.PatientId.ToString(), e.EncounterType, e.TriageCategory ?? "-", e.CurrentState, e.ArrivalTime.ToString("HH:mm:ss"));
    AnsiConsole.Write(table);
}

async Task CallNextAsync()
{
    var result = await api.PutAsync<object, ApiEnvelope<EncounterDto>>("/api/encounter/call-next", new { });
    if (result?.Data is not null)
        AnsiConsole.MarkupLine($"[green]Called encounter: {result.Data.EncounterId}[/]");
}

async Task VerifyOrderAsync()
{
    var patientId = AnsiConsole.Ask<Guid>("Patient Id:");
    var drug = AnsiConsole.Ask<string>("Drug Name:");
    var req = new VerifyOrderRequest { PatientId = patientId, DrugName = drug };
    var result = await api.PostAsync<VerifyOrderRequest, ApiEnvelope<AllergyCheckResult>>("/api/order/verify", req);
    if (result is null)
        return;

    if (result.Success)
    {
        AnsiConsole.MarkupLine("[green]Order verified: no allergy conflict.[/]");
    }
    else
    {
        var data = result.Data;
        AnsiConsole.MarkupLine($"[red]Hard block[/]: {data?.ConflictDetail} (Severity: {data?.Severity})");
    }
}

async Task ScheduleAppointmentAsync()
{
    var patientId = AnsiConsole.Ask<Guid>("Patient Id:");
    var scheduledTimeInput = AnsiConsole.Ask<DateTime>("Scheduled datetime (yyyy-MM-dd HH:mm):");

    var req = new CreateAppointmentRequest
    {
        PatientId = patientId,
        // Convert to UTC
        ScheduledTime = DateTime.SpecifyKind(scheduledTimeInput, DateTimeKind.Utc),
        Department = AnsiConsole.Ask<string>("Department:"),
        DoctorName = AnsiConsole.Ask<string>("Doctor name:")
    };

    var result = await api.PostAsync<CreateAppointmentRequest, ApiEnvelope<AppointmentDto>>("/api/appointment", req);
    if (result?.Data is not null)
        AnsiConsole.MarkupLine($"[green]Appointment created[/] ID={result.Data.AppointmentId} QR={result.Data.QRCode}");
}

async Task ViewAppointmentsByDateAsync()
{
    var dateInput = AnsiConsole.Ask<DateTime>("Date (yyyy-MM-dd):").Date;
    var result = await api.GetAsync<ApiEnvelope<List<AppointmentDto>>>($"/api/appointment?date={dateInput:yyyy-MM-dd}");
    if (result?.Data is null || result.Data.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]No appointments.[/]");
        return;
    }

    var table = new Table();
    table.AddColumn("Id");
    table.AddColumn("Patient");
    table.AddColumn("Time");
    table.AddColumn("Dept");
    table.AddColumn("Doctor");
    table.AddColumn("Status");
    foreach (var a in result.Data)
        table.AddRow(a.AppointmentId.ToString(), a.PatientId.ToString(), a.ScheduledTime.ToString("HH:mm"), a.Department, a.DoctorName, a.Status);
    AnsiConsole.Write(table);
}

async Task ValidateQrAsync()
{
    var qr = AnsiConsole.Ask<string>("QR code/token:");
    var result = await api.PostAsync<ValidateQrRequest, ApiEnvelope<object>>("/api/appointment/validate-qr", new ValidateQrRequest { QrCode = qr });
    if (result?.Success == true)
        AnsiConsole.MarkupLine("[green]QR validated and encounter created.[/]");
}

async Task ViewOpenAlertsAsync()
{
    var result = await api.GetAsync<ApiEnvelope<List<QueueAlertDto>>>("/api/alert?resolved=false");
    if (result?.Data is null || result.Data.Count == 0)
    {
        AnsiConsole.MarkupLine("[yellow]No open alerts.[/]");
        return;
    }

    var table = new Table();
    table.AddColumn("AlertId");
    table.AddColumn("Type");
    table.AddColumn("Severity");
    table.AddColumn("Triggered");
    foreach (var a in result.Data)
        table.AddRow(a.AlertId.ToString(), a.AlertType, a.Severity, a.TriggeredAt.ToString("yyyy-MM-dd HH:mm:ss"));
    AnsiConsole.Write(table);
}

async Task ResolveAlertAsync()
{
    var id = AnsiConsole.Ask<Guid>("Alert Id:");
    var result = await api.PutAsync<object, ApiEnvelope<object>>($"/api/alert/{id}/resolve", new { });
    if (result?.Success == true)
        AnsiConsole.MarkupLine("[green]Alert resolved.[/]");
}

async Task DailySummaryAsync()
{
    var dateInput = AnsiConsole.Ask<DateTime>("Date (yyyy-MM-dd):").Date;
    var result = await api.GetAsync<ApiEnvelope<DailySummaryDto>>($"/api/reports/daily-summary?date={dateInput:yyyy-MM-dd}");
    if (result?.Data is null) return;

    var d = result.Data;
    var table = new Table();
    table.AddColumn("Metric");
    table.AddColumn("Value");
    table.AddRow("Date", d.Date.ToString("yyyy-MM-dd"));
    table.AddRow("Active Encounters", d.ActiveEncounters.ToString());
    table.AddRow("Waiting", d.WaitingCount.ToString());
    table.AddRow("In Service", d.InServiceCount.ToString());
    table.AddRow("Avg Est Wait (min)", d.AvgEstimatedWaitMin.ToString("F2"));
    table.AddRow("Current rho", d.CurrentRho?.ToString("F2") ?? "-");
    AnsiConsole.Write(table);
}

async Task QueuePerformanceAsync()
{
    var hours = AnsiConsole.Ask<int>("Hours:");
    var result = await api.GetAsync<ApiEnvelope<List<QueuePerformancePoint>>>($"/api/reports/queue-performance?hours={hours}");
    if (result?.Data is null || result.Data.Count == 0) return;

    var table = new Table();
    table.AddColumn("Time");
    table.AddColumn("rho");
    table.AddColumn("Lq");
    table.AddColumn("Wq min");
    foreach (var p in result.Data.Take(20))
        table.AddRow(p.ComputedAtUtc.ToLocalTime().ToString("MM-dd HH:mm"), p.Rho.ToString("F2"), p.Lq.ToString("F2"), p.WqMinutes.ToString("F1"));
    AnsiConsole.Write(table);
}

async Task TriageBreakdownAsync()
{
    var result = await api.GetAsync<ApiEnvelope<List<TriageBreakdownItem>>>("/api/reports/triage-breakdown");
    if (result?.Data is null || result.Data.Count == 0) return;

    var table = new Table();
    table.AddColumn("Triage");
    table.AddColumn("Count");
    table.AddColumn("Avg Wait (min)");
    foreach (var row in result.Data)
        table.AddRow(row.Triage, row.Count.ToString(), row.AvgEstimatedWaitMin.ToString("F1"));
    AnsiConsole.Write(table);
}

async Task ModelComparisonAsync()
{
    var hours = AnsiConsole.Ask<int>("Hours:");
    var result = await api.GetAsync<ApiEnvelope<List<ModelComparisonItem>>>($"/api/reports/model-comparison?hours={hours}");
    if (result?.Data is null || result.Data.Count == 0) return;

    var table = new Table();
    table.AddColumn("ModelId");
    table.AddColumn("Samples");
    table.AddColumn("Avg rho");
    table.AddColumn("Avg Wq min");
    table.AddColumn("Peak Wq min");
    foreach (var row in result.Data)
        table.AddRow(row.ModelId.ToString(), row.Samples.ToString(), row.AvgRho.ToString("F2"), row.AvgWqMin.ToString("F1"), row.PeakWqMin.ToString("F1"));
    AnsiConsole.Write(table);
}

async Task SlaComplianceAsync()
{
    var hours = AnsiConsole.Ask<int>("Hours:");
    var result = await api.GetAsync<ApiEnvelope<SlaComplianceDto>>($"/api/reports/sla-compliance?hours={hours}");
    if (result?.Data is null) return;
    var d = result.Data;
    AnsiConsole.MarkupLine($"[green]SLA compliance[/]: {d.CompliancePercent:F2}% (breaches {d.SlaBreaches}/{d.TotalSnapshots}, hours={d.Hours})");
}