# EDMS / OPDMS

Emergency & Outpatient Department Management System built with ASP.NET Core 8, SignalR, and a menu-driven C# console client.

## Project Overview

This solution provides a full workflow for emergency/outpatient operations:

- patient registration and search
- encounter creation and queue handling
- queue model computation (MM1, MG1, GG1)
- allergy conflict hard-block checks
- appointment scheduling and QR validation
- resource allocation/status tracking
- alerts and analytics reports
- real-time updates through SignalR

## Solution Structure

- `EDMS.API` - ASP.NET Core Web API + SignalR hub
- `EDMS.Core` - domain models, interfaces, queue engines
- `EDMS.Infrastructure` - EF Core DbContext, repositories, auth utilities, seeding
- `EDMS.Console` - menu-driven executable client
- `EDMS.Tests` - xUnit tests for queue logic
- `db/001_initial_schema.sql` - SQL schema script (for PostgreSQL/Supabase usage)

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- Npgsql (PostgreSQL provider)
- SignalR
- JWT authentication
- Spectre.Console
- xUnit

## Features Implemented

### Authentication & Roles

- login endpoint with JWT
- role policies:
  - `StaffPolicy`
  - `ClinicalPolicy`
  - `DoctorPolicy`
  - `AdminPolicy`

### Patient Management

- register patient
- search patient by MRN/name
- get patient by id
- get patient by MRN

### Encounter & Queue Operations

- create encounter
- get active encounters
- get encounter by id
- get encounters by patient
- update encounter state
- call next patient by triage priority + FIFO
- strict state machine enforcement:
  - `Waiting -> InService | Cancelled`
  - `InService -> Completed | Cancelled`
  - `Completed -> Discharged | Admitted`

### Queue Engines

- M/M/1 exact formulas
- M/G/1 Pollaczek-Khinchine
- G/G/1 Kingman approximation
- queue snapshot persistence
- queue alerts for SLA and capacity

### Clinical & Allergy

- order verification endpoint
- allergy conflict detection against allergy registry
- hard block on conflict
- `AllergyAlert` SignalR event

### Resource Management

- view resource availability
- update resource status
- `ResourceChange` SignalR event

### Appointments

- schedule appointment
- view appointments by date
- update appointment status
- validate QR code
- auto-create encounter from valid QR check-in

### Alerts

- list open/resolved alerts
- resolve alert

### Reports

- daily summary
- queue performance (time-series)
- resource utilization
- triage breakdown
- model comparison
- SLA compliance

### Real-Time SignalR Events

- `QueueUpdate`
- `SLABreach`
- `CapacityWarning`
- `AllergyAlert`
- `PatientCalled`
- `ResourceChange`
- `EncounterStateChange`
- `NewPatientArrival`

## API Base URL

Default local URL used in this project:

- `http://localhost:5071`

## Default Seed Credentials

- `admin / Admin@123`
- `doctor1 / Doctor@123`
- `nurse1 / Nurse@123`

## Run from Source (Development)

From repository root:

1) Build solution:

```powershell
dotnet build EDMS.sln
```

2) Run API:

```powershell
dotnet run --project EDMS.API
```

3) Run console client in another terminal:

```powershell
dotnet run --project EDMS.Console
```

4) Run tests:

```powershell
dotnet test EDMS.Tests/EDMS.Tests.csproj
```

## Generate Executables (.exe) for Submission

This project has two runtime applications:

- API server executable (`EDMS.API.exe`)
- Console client executable (`EDMS.Console.exe`)

Both are required for full operation.

Use these commands from repository root:

```powershell
dotnet publish EDMS.API/EDMS.API.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o publish/api
dotnet publish EDMS.Console/EDMS.Console.csproj -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true -o publish/console
```

After publishing, run:

1) Start API executable:

```powershell
.\publish\api\EDMS.API.exe
```

2) Start console executable (new terminal):

```powershell
.\publish\console\EDMS.Console.exe
```

## Notes

- Current default runtime uses in-memory DB if connection string is empty.
- For Supabase/PostgreSQL deployment, set `ConnectionStrings:SupabaseConnection` in `EDMS.API/appsettings.json`.
- Console config is in `EDMS.Console/appsettings.json`.
