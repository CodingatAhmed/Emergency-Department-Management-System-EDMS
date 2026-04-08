<div align="center">

# 🏥 EDMS / OPDMS
### Emergency & Outpatient Department Management System

A full-stack hospital operations platform with real-time queue management,  
clinical safety checks, appointment scheduling, and live SignalR dashboards.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-336791?style=flat-square&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-00ADEF?style=flat-square)](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction)
[![EF Core](https://img.shields.io/badge/EF%20Core-8-68217A?style=flat-square)](https://learn.microsoft.com/en-us/ef/core/)
[![xUnit](https://img.shields.io/badge/Tests-xUnit-brightgreen?style=flat-square)](https://xunit.net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)](LICENSE)

</div>

---

## 📖 Table of Contents

- [Overview](#-overview)
- [Features](#-features)
- [Tech Stack](#-tech-stack)
- [Architecture](#-architecture)
- [Folder Structure](#-folder-structure)
- [Prerequisites](#-prerequisites)
- [Getting Started](#-getting-started)
  - [1. Clone the Repository](#1-clone-the-repository)
  - [2. Database Setup](#2-database-setup)
  - [3. Configure the API](#3-configure-the-api)
  - [4. Run the API Server](#4-run-the-api-server)
  - [5. Run the Console Client](#5-run-the-console-client)
  - [6. Run the Tests](#6-run-the-tests)
- [Configuration Reference](#-configuration-reference)
- [API Endpoints](#-api-endpoints)
- [SignalR Events](#-signalr-events)
- [Queue Engine Models](#-queue-engine-models)
- [Role & Auth System](#-role--auth-system)
- [Publishing Executables](#-publishing-executables)
- [Default Seed Credentials](#-default-seed-credentials)
- [Troubleshooting](#-troubleshooting)
- [Contributing](#-contributing)
- [License](#-license)

---

## 🔍 Overview

EDMS/OPDMS is a comprehensive hospital emergency and outpatient department management system. It manages the full patient lifecycle — from registration and triage, through encounter queuing and clinical order safety checks, all the way to discharge or admission — while broadcasting live updates to all connected clients via SignalR.

The solution is structured into clean, separated layers: a Web API backend, a shared domain/core library, an infrastructure layer for data persistence, a menu-driven console client for staff operations, and a test suite for queue logic verification.

---

## ✨ Features

### 🔐 Authentication & Access Control
- JWT-based login with role-scoped policy enforcement
- Four role policies: `StaffPolicy`, `ClinicalPolicy`, `DoctorPolicy`, `AdminPolicy`

### 🧑‍⚕️ Patient Management
- Register new patients with MRN assignment
- Search by MRN (Medical Record Number) or name
- Retrieve full patient record by ID or MRN

### 🚑 Encounter & Queue Operations
- Create and manage patient encounters
- Call next patient using **triage priority + FIFO** ordering
- Strict state machine enforcement:

```
Waiting  ──►  InService  ──►  Completed  ──►  Discharged
   │               │                              │
   └──► Cancelled  └──► Cancelled            Admitted
```

### 📊 Queue Engine Models
- **M/M/1** — exact Markovian queue formulas
- **M/G/1** — Pollaczek-Khinchine mean value approximation
- **G/G/1** — Kingman's approximation for general distributions
- Persistent queue snapshots and SLA/capacity alerting

### 💊 Clinical & Allergy Safety
- Order verification endpoint
- Allergy conflict detection against the allergy registry
- **Hard block** on conflicting medication orders
- Real-time `AllergyAlert` SignalR push to all connected staff

### 🛠️ Resource Management
- View current resource availability
- Update resource status with live `ResourceChange` events

### 📅 Appointment Scheduling
- Schedule, view, and update appointments
- QR code validation for patient check-in
- Auto-create encounter from a valid QR scan

### 🔔 Alerts & Reports
- Manage open/resolved system alerts
- Reports: daily summary, queue performance, resource utilization, triage breakdown, model comparison, SLA compliance

### ⚡ Real-time SignalR Events
`QueueUpdate` · `SLABreach` · `CapacityWarning` · `AllergyAlert` · `PatientCalled` · `ResourceChange` · `EncounterStateChange` · `NewPatientArrival`

---

## 🛠 Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 8 |
| Web Framework | ASP.NET Core 8 Web API |
| Real-time | ASP.NET Core SignalR |
| ORM | Entity Framework Core 8 |
| Database Driver | Npgsql (PostgreSQL provider) |
| Database | PostgreSQL / Supabase (or In-Memory) |
| Authentication | JWT Bearer Tokens |
| Console UI | Spectre.Console |
| Testing | xUnit |

---

## 🏛 Architecture

```
┌──────────────────────────────────────────────────────────┐
│                      EDMS.Console                        │
│         Menu-driven C# client (Spectre.Console)          │
│         Communicates via HTTP REST + SignalR WebSocket    │
└─────────────────────────┬────────────────────────────────┘
                          │  HTTP + WebSocket
┌─────────────────────────▼────────────────────────────────┐
│                       EDMS.API                           │
│   ASP.NET Core Web API  +  SignalR Hub (/hubs/queue)     │
│   Controllers · JWT Middleware · Role Policies           │
└──────────────┬───────────────────────────┬───────────────┘
               │                           │
┌──────────────▼────────────┐  ┌───────────▼──────────────────┐
│        EDMS.Core          │  │      EDMS.Infrastructure      │
│  Domain models            │  │  EF Core DbContext            │
│  Repository interfaces    │  │  Concrete repositories        │
│  Queue engines            │  │  JWT auth utilities           │
│  (MM1, MG1, GG1)          │  │  Database seeding             │
└───────────────────────────┘  └────────────┬─────────────────┘
                                             │
                                ┌────────────▼──────────┐
                                │ PostgreSQL / Supabase  │
                                │  (or In-Memory DB)     │
                                └───────────────────────┘
```

---

## 📁 Folder Structure

```
Emergency-Department-Management-System-EDMS/
│
├── EDMS.sln                             # Visual Studio solution — open this to load all projects
│
├── EDMS.API/                            # ── ASP.NET Core Web API ───────────────────────────
│   │
│   ├── Controllers/                     # HTTP endpoint controllers (one per feature domain)
│   │   ├── AuthController.cs            #   POST /api/auth/login → issues JWT token
│   │   ├── PatientController.cs         #   Register, search, get by MRN/ID
│   │   ├── EncounterController.cs       #   Create encounter, list active, advance state
│   │   ├── QueueController.cs           #   Call next patient, queue snapshots, model results
│   │   ├── AppointmentController.cs     #   Schedule, update status, validate QR code
│   │   ├── ResourceController.cs        #   List availability, update resource status
│   │   ├── AlertController.cs           #   List open/resolved alerts, resolve an alert
│   │   ├── ReportController.cs          #   All analytics report endpoints
│   │   └── ClinicalController.cs        #   Order verification + allergy hard-block check
│   │
│   ├── Hubs/
│   │   └── QueueHub.cs                  #   SignalR hub mounted at /hubs/queue
│   │                                    #   Broadcasts all real-time events to connected clients
│   │
│   ├── appsettings.json                 #   Production config: DB connection string, JWT, port
│   ├── appsettings.Development.json     #   Dev overrides: verbose EF logging, Swagger, etc.
│   └── Program.cs                       #   App bootstrap: DI registration, middleware pipeline,
│                                        #   EF Core setup, SignalR mount, Kestrel port binding
│
├── EDMS.Core/                           # ── Domain Layer (zero external dependencies) ───────
│   │
│   ├── Models/                          # Pure C# entity/domain models
│   │   ├── Patient.cs                   #   Patient: MRN, name, DOB, demographics
│   │   ├── Encounter.cs                 #   Encounter: triage category, state, timestamps
│   │   ├── QueueModel.cs                #   Queue snapshot: Lq, Wq, utilisation, model type
│   │   ├── Appointment.cs               #   Appointment + QR code payload
│   │   ├── StaffUser.cs                 #   Staff/doctor/nurse user record + role
│   │   ├── AllergyRegistry.cs           #   Patient allergy records (drug, severity)
│   │   ├── Resource.cs                  #   Hospital resource: bed, ventilator, etc.
│   │   └── Alert.cs                     #   System alert: type, message, resolved flag
│   │
│   ├── Interfaces/                      # Repository + service contracts
│   │   ├── IPatientRepository.cs        #   CRUD + search operations for patients
│   │   ├── IEncounterRepository.cs      #   Encounter queries + state transitions
│   │   ├── IQueueRepository.cs          #   Queue snapshot reads and writes
│   │   └── ...                          #   (one interface per aggregate)
│   │
│   └── QueueEngines/                    # Queueing theory computation engines
│       ├── MM1Engine.cs                 #   M/M/1: Poisson arrivals, exponential service
│       │                                #   Exact formulas: ρ, Lq, Wq, L, W
│       ├── MG1Engine.cs                 #   M/G/1: Poisson arrivals, general service
│       │                                #   Pollaczek-Khinchine mean value equations
│       └── GG1Engine.cs                 #   G/G/1: General arrivals + general service
│                                        #   Kingman's approximation formula
│
├── EDMS.Infrastructure/                 # ── Data & Auth Layer ──────────────────────────────
│   │
│   ├── Data/
│   │   └── AppDbContext.cs              #   EF Core DbContext: all DbSets, model configuration,
│   │                                    #   indexes, relationship mappings
│   │
│   ├── Repositories/                    # Concrete EF Core implementations of Core interfaces
│   │   ├── PatientRepository.cs         #   Patient queries: MRN lookup, name search, paging
│   │   ├── EncounterRepository.cs       #   State machine transitions, active encounter queries
│   │   ├── QueueRepository.cs           #   Snapshot persistence, time-series retrieval
│   │   └── ...
│   │
│   ├── Auth/
│   │   └── JwtHelper.cs                 #   Token generation, claims packaging, role-to-policy map
│   │
│   └── Seeding/
│       └── DbSeeder.cs                  #   Checks tables on startup; inserts default users
│                                        #   (admin, doctor1, nurse1) if StaffUsers is empty
│
├── EDMS.Console/                        # ── Interactive Console Client ─────────────────────
│   │
│   ├── ApiClient/
│   │   └── ApiHttpClient.cs             #   HTTP client wrapper used by all menus
│   │                                    #   PostAsync<TReq,T>, GetAsync<T>, PutAsync<TReq,T>
│   │                                    #   Attaches JWT Bearer token to every request
│   │
│   ├── Menus/                           # One Spectre.Console menu class per feature area
│   │   ├── PatientMenu.cs               #   Register new patient, search by MRN/name
│   │   ├── EncounterMenu.cs             #   Create encounter, view active, advance state
│   │   ├── QueueMenu.cs                 #   Call next patient, display queue metrics
│   │   ├── AppointmentMenu.cs           #   Schedule appointment, scan/validate QR
│   │   ├── ResourceMenu.cs              #   View availability, update resource status
│   │   ├── AlertMenu.cs                 #   View and resolve system alerts
│   │   └── ReportMenu.cs                #   Choose and display analytics reports
│   │
│   ├── appsettings.json                 #   ApiBaseUrl and HubUrl — must match API port
│   └── Program.cs                       #   Entry point: reads config, performs login,
│                                        #   establishes SignalR connection, shows main menu
│
├── EDMS.Tests/                          # ── Test Suite (xUnit) ─────────────────────────────
│   │
│   ├── QueueEngineTests/
│   │   ├── MM1EngineTests.cs            #   Tests M/M/1 exact formulas with known inputs
│   │   ├── MG1EngineTests.cs            #   Tests M/G/1 Pollaczek-Khinchine results
│   │   └── GG1EngineTests.cs            #   Tests G/G/1 Kingman approximation
│   │
│   └── EDMS.Tests.csproj
│
├── db/
│   └── 001_initial_schema.sql           # Raw PostgreSQL schema script — use this if you prefer
│                                        # manual DB setup instead of EF Core migrations
│
└── publish/                             # Compiled output (generated by dotnet publish)
    ├── api/                             #   EDMS.API.exe + appsettings.json
    └── console/                         #   EDMS.Console.exe + appsettings.json
```

### Dependency rules at a glance

```
EDMS.Console  ─────────────────────►  EDMS.API          (network only — HTTP + SignalR)
EDMS.API      ─────────────────────►  EDMS.Core
EDMS.API      ─────────────────────►  EDMS.Infrastructure
EDMS.Infrastructure  ──────────────►  EDMS.Core
EDMS.Tests    ─────────────────────►  EDMS.Core
```

`EDMS.Core` has **zero project references** — it is pure domain logic and can be tested in isolation. `EDMS.Console` never imports the other projects; it only ever communicates over the network with `EDMS.API`.

---

## ✅ Prerequisites

| Requirement | Minimum Version | Notes |
|---|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download/dotnet/8.0) | 8.0 | Required to build and run from source |
| [PostgreSQL](https://www.postgresql.org/download/) | 14 | Or use Supabase; leave connection string empty for in-memory |
| Terminal / PowerShell | any | VS Code integrated terminal works fine |
| [Visual Studio 2022](https://visualstudio.microsoft.com/) *(optional)* | 17.8+ | Or VS Code with the C# Dev Kit extension |

---

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/CodingatAhmed/Emergency-Department-Management-System-EDMS.git
cd Emergency-Department-Management-System-EDMS
```

### 2. Database Setup

**Option A — PostgreSQL (full persistence, recommended)**

```sql
-- Run in psql or pgAdmin
CREATE DATABASE edms_db;
```

Either let EF Core auto-create the schema on first run (happens automatically), or run the provided script manually:

```bash
psql -U postgres -d edms_db -f db/001_initial_schema.sql
```

**Option B — Supabase (cloud PostgreSQL)**

1. Create a free project at [supabase.com](https://supabase.com)
2. Go to **Project Settings → Database → Connection String**
3. Copy the URI and paste it into `EDMS.API/appsettings.json` (see step 3)

**Option C — In-Memory (quickest start, no install required)**

Leave `ConnectionStrings:SupabaseConnection` as an empty string `""`. The API will use EF Core's in-memory provider automatically. Data does not persist between restarts.

---

### 3. Configure the API

Edit `EDMS.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "SupabaseConnection": "Host=localhost;Port=5432;Database=edms_db;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Key": "your-secret-signing-key-must-be-at-least-32-characters",
    "Issuer": "EDMS",
    "Audience": "EDMS",
    "ExpiryMinutes": 480
  },
  "Kestrel": {
    "Endpoints": {
      "Http": { "Url": "http://localhost:5071" }
    }
  }
}
```

> **Why the Kestrel block matters:** Without it, `dotnet run` picks a random port and `dotnet publish` defaults to port 5000, causing a mismatch with the console client. The Kestrel config locks the API to 5071 consistently in both modes.

Edit `EDMS.Console/appsettings.json` to match:

```json
{
  "ApiBaseUrl": "http://localhost:5071",
  "HubUrl":     "http://localhost:5071/hubs/queue"
}
```

---

### 4. Run the API Server

```bash
# From the repository root
dotnet build EDMS.sln
dotnet run --project EDMS.API
```

Healthy startup output:

```
info: ...EF Core... SELECT EXISTS (SELECT 1 FROM "StaffUsers")
info: ...EF Core... SELECT EXISTS (SELECT 1 FROM "QueueModels")
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5071
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

The seeder runs on startup — if the `StaffUsers` table is empty, default admin/doctor/nurse accounts are inserted automatically.

> Swagger UI is available at **`http://localhost:5071/swagger`** when running in Development mode. You can use it to test any endpoint directly in the browser.

---

### 5. Run the Console Client

Open a **second terminal** (keep the API running in the first):

```bash
dotnet run --project EDMS.Console
```

Login prompt:

```
╔══════════════════════════════╗
║     EDMS Console Client      ║
║  API : http://localhost:5071 ║
║  Hub : /hubs/queue           ║
╚══════════════════════════════╝
Username: admin
Password: ****
✔ Login successful — welcome, admin
```

Main menu (rendered with Spectre.Console):

```
╔══════════════════════════════╗
║    EDMS / OPDMS Main Menu    ║
╠══════════════════════════════╣
║  [1] Patient Management      ║
║  [2] Encounter & Queue       ║
║  [3] Appointments            ║
║  [4] Resource Management     ║
║  [5] Alerts                  ║
║  [6] Reports                 ║
║  [0] Logout                  ║
╚══════════════════════════════╝
```

All menu actions call `EDMS.API` over HTTP. Real-time notifications (allergy alerts, SLA breaches, patient calls) are pushed to the console automatically via the SignalR connection established at login.

---

### 6. Run the Tests

```bash
dotnet test EDMS.Tests/EDMS.Tests.csproj
```

Expected output:

```
Passed!  - Failed: 0, Passed: N, Skipped: 0
```

The suite validates all three queue engines against known mathematical inputs and expected outputs (utilisation ρ, mean queue length Lq, mean wait time Wq).

---

## ⚙️ Configuration Reference

### `EDMS.API/appsettings.json`

| Key | Description | Example |
|---|---|---|
| `ConnectionStrings:SupabaseConnection` | Full Npgsql connection string. Empty string = in-memory DB. | `Host=localhost;Port=5432;...` |
| `Jwt:Key` | HMAC-SHA256 signing key — 32+ chars required in production | `"super-secret-key-min-32-chars!"` |
| `Jwt:Issuer` | JWT `iss` claim | `"EDMS"` |
| `Jwt:Audience` | JWT `aud` claim | `"EDMS"` |
| `Jwt:ExpiryMinutes` | Token lifetime | `480` |
| `Kestrel:Endpoints:Http:Url` | Port the API binds to | `"http://localhost:5071"` |

### `EDMS.Console/appsettings.json`

| Key | Description | Must equal |
|---|---|---|
| `ApiBaseUrl` | Base URL for all REST calls | Kestrel URL above |
| `HubUrl` | SignalR hub connection URL | `{ApiBaseUrl}/hubs/queue` |

---

## 📡 API Endpoints

### Auth
| Method | Route | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/login` | Public | Returns JWT token |

### Patients
| Method | Route | Policy | Description |
|---|---|---|---|
| `POST` | `/api/patient` | Staff | Register new patient |
| `GET` | `/api/patient/{id}` | Staff | Get by ID |
| `GET` | `/api/patient/mrn/{mrn}` | Staff | Get by MRN |
| `GET` | `/api/patient/search?q=` | Staff | Search by name or MRN |

### Encounters
| Method | Route | Policy | Description |
|---|---|---|---|
| `POST` | `/api/encounter` | Staff | Create new encounter |
| `GET` | `/api/encounter/active` | Staff | List all active encounters |
| `GET` | `/api/encounter/{id}` | Staff | Get single encounter |
| `GET` | `/api/encounter/patient/{patientId}` | Staff | All encounters for a patient |
| `PUT` | `/api/encounter/{id}/state` | Clinical | Advance the state machine |

### Queue
| Method | Route | Policy | Description |
|---|---|---|---|
| `POST` | `/api/queue/call-next` | Clinical | Call next by triage priority + FIFO |
| `GET` | `/api/queue/snapshot` | Staff | Current queue metrics |
| `GET` | `/api/queue/models` | Admin | MM1 / MG1 / GG1 computed results |

### Appointments
| Method | Route | Policy | Description |
|---|---|---|---|
| `POST` | `/api/appointment` | Staff | Schedule appointment |
| `GET` | `/api/appointment/date/{date}` | Staff | Appointments for a date |
| `PUT` | `/api/appointment/{id}/status` | Staff | Update appointment status |
| `POST` | `/api/appointment/validate-qr` | Staff | Validate QR → auto-create encounter |

### Clinical
| Method | Route | Policy | Description |
|---|---|---|---|
| `POST` | `/api/clinical/verify-order` | Doctor | Check order against allergy registry |

### Resources
| Method | Route | Policy | Description |
|---|---|---|---|
| `GET` | `/api/resource` | Staff | List all resources and availability |
| `PUT` | `/api/resource/{id}/status` | Admin | Update a resource's status |

### Alerts
| Method | Route | Policy | Description |
|---|---|---|---|
| `GET` | `/api/alert` | Staff | List open and resolved alerts |
| `PUT` | `/api/alert/{id}/resolve` | Admin | Mark an alert as resolved |

### Reports
| Method | Route | Policy | Description |
|---|---|---|---|
| `GET` | `/api/report/daily-summary` | Admin | Daily activity summary |
| `GET` | `/api/report/queue-performance` | Admin | Time-series queue data |
| `GET` | `/api/report/resource-utilization` | Admin | Resource usage breakdown |
| `GET` | `/api/report/triage-breakdown` | Admin | Encounter count by triage category |
| `GET` | `/api/report/model-comparison` | Admin | MM1 vs MG1 vs GG1 side-by-side |
| `GET` | `/api/report/sla-compliance` | Admin | SLA breach rate and details |

---

## ⚡ SignalR Events

Hub endpoint: `ws://localhost:5071/hubs/queue`

| Event | Fired when | Payload contains |
|---|---|---|
| `QueueUpdate` | Any encounter state change or new arrival | Queue snapshot |
| `SLABreach` | Patient wait time exceeds SLA threshold | Alert record |
| `CapacityWarning` | Queue length exceeds configured capacity | Alert record |
| `AllergyAlert` | Order conflicts with patient allergy registry | Patient + drug details |
| `PatientCalled` | `/api/queue/call-next` is invoked | Encounter + patient info |
| `ResourceChange` | Resource status is updated | Updated resource record |
| `EncounterStateChange` | State machine transitions | Previous + new state |
| `NewPatientArrival` | New encounter is registered | Patient + triage data |

---

## 📐 Queue Engine Models

Located in `EDMS.Core/QueueEngines/` — tested in `EDMS.Tests/`.

| Model | File | Arrivals | Service | Key formula used |
|---|---|---|---|---|
| **M/M/1** | `MM1Engine.cs` | Poisson | Exponential | Exact: Lq = ρ²/(1−ρ) |
| **M/G/1** | `MG1Engine.cs` | Poisson | General | Pollaczek-Khinchine mean value |
| **G/G/1** | `GG1Engine.cs` | General | General | Kingman's approximation |

All three engines return: utilisation ρ, mean customers in queue Lq, mean wait in queue Wq, mean customers in system L, mean time in system W. Results are stored as `QueueModel` snapshots in the database for trend analysis and reports.

---

## 🔐 Role & Auth System

JWT tokens are issued on login with a `role` claim. Policies are enforced by ASP.NET Core middleware.

| Policy | Roles permitted | Typical user |
|---|---|---|
| `StaffPolicy` | Staff, Clinical, Doctor, Admin | Any logged-in employee |
| `ClinicalPolicy` | Clinical, Doctor, Admin | Nurses, doctors |
| `DoctorPolicy` | Doctor, Admin | Attending physicians |
| `AdminPolicy` | Admin only | IT / department head |

---

## 📦 Publishing Executables

To produce standalone `.exe` files (no .NET SDK needed on the target machine):

```powershell
# From the repository root

# Publish API
dotnet publish EDMS.API/EDMS.API.csproj `
  -c Release -r win-x64 --self-contained true `
  /p:PublishSingleFile=true -o publish/api

# Publish Console client
dotnet publish EDMS.Console/EDMS.Console.csproj `
  -c Release -r win-x64 --self-contained true `
  /p:PublishSingleFile=true -o publish/console
```

Run on any Windows x64 machine:

```powershell
# Terminal 1
.\publish\api\EDMS.API.exe

# Terminal 2
.\publish\console\EDMS.Console.exe
```

> The `appsettings.json` files inside `publish/api/` and `publish/console/` are what the executables read at runtime. Edit those files (not the source ones) when deploying to a different machine or database.

---

## 🔑 Default Seed Credentials

Inserted automatically on first run if `StaffUsers` is empty.

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin@123` | Admin |
| `doctor1` | `Doctor@123` | Doctor |
| `nurse1` | `Nurse@123` | Clinical |

> ⚠️ **Change all default passwords before any production or public deployment.**

---

## 🔧 Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| `Connection actively refused (10061)` on port 5071 | API is not running | Start `EDMS.API` first, then the console |
| API listens on port 5000 instead of 5071 | Kestrel config missing | Add `Kestrel:Endpoints:Http:Url` to `EDMS.API/appsettings.json` |
| `relation "StaffUsers" does not exist` | DB not created or migrations not run | Run `dotnet ef database update --project EDMS.Infrastructure --startup-project EDMS.API` |
| `401 Unauthorized` on login | Wrong password or JWT key mismatch | Verify seed credentials; check `Jwt:Key` is the same in both environments |
| SignalR not receiving events | Wrong `HubUrl` or API down | Confirm API is running; check `HubUrl` in console `appsettings.json` |
| Data lost after restart | In-memory DB mode is active | Set a real PostgreSQL connection string in `appsettings.json` |
| Published `.exe` crashes on startup | `appsettings.json` not next to the `.exe` | Ensure `appsettings.json` was copied into the `publish/` output folder |

---

## 🤝 Contributing

1. **Fork** the repository
2. Create a feature branch
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. Make your changes and ensure tests pass
   ```bash
   dotnet test EDMS.Tests/EDMS.Tests.csproj
   ```
4. Commit with a clear message
   ```bash
   git commit -m "feat: add discharge summary PDF export"
   ```
5. Push and open a **Pull Request** against `main`

Please keep controllers thin — business logic belongs in services or domain engines, not in controller actions. New queue-related logic should be in `EDMS.Core/QueueEngines/` and covered by a test in `EDMS.Tests/`.

---

## 📄 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for full details.

---

<div align="center">
Built for efficient, real-time emergency department operations.<br/>
<sub>EDMS / OPDMS · ASP.NET Core 8 · PostgreSQL · SignalR · Spectre.Console · xUnit</sub>
</div>
