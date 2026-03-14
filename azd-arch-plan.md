# Azure Developer CLI (azd) Application Discovery and Analysis Report

## 1. File System Inventory (High-Level)
- **Root Directory**: PuppyWeightWatcher.slnx, azd-agent-*.log
- **Projects**:
  - PuppyWeightWatcher.ApiService/ (ASP.NET Core Web API, .NET 10)
  - PuppyWeightWatcher.Web/ (ASP.NET Core Razor Components Web App, .NET 10)
  - PuppyWeightWatcher.AppHost/ (Aspire Orchestrator, .NET 10)
  - PuppyWeightWatcher.ServiceDefaults/ (Shared Infra/Defaults/Observability, .NET 10)
  - PuppyWeightWatcher.Shared/ (Shared Models, .NET 10)
- **Config & Data**: appsettings.json (per project), bin/obj (per project), Models/Components/Services (internal logic)

## 2. Component Classification
| Component                        | Type                   | Technology/Framework     | Location                              | Purpose                                                  |
|-----------------------------------|------------------------|-------------------------|---------------------------------------|----------------------------------------------------------|
| PuppyWeightWatcher.ApiService     | API Service            | ASP.NET Core Web API    | PuppyWeightWatcher.ApiService/        | Backend service exposing API endpoints                   |
| PuppyWeightWatcher.Web            | Web Application        | Razor Components, .NET  | PuppyWeightWatcher.Web/               | Interactive Web UI, communicates with ApiService         |
| PuppyWeightWatcher.AppHost        | Orchestrator           | Aspire AppHost, .NET    | PuppyWeightWatcher.AppHost/           | Dev-time orchestration of all app components             |
| PuppyWeightWatcher.ServiceDefaults| Shared/Infra Library   | .NET, OpenTelemetry     | PuppyWeightWatcher.ServiceDefaults/    | Observability, Resilience, Service Discovery, Shared conf |
| PuppyWeightWatcher.Shared         | Shared Model Library   | .NET                    | PuppyWeightWatcher.Shared/             | Common data models shared by all components              |

## 3. Dependency & Communication Map
- **Web App (PuppyWeightWatcher.Web)** ⟶ (HTTP) ⟶ **ApiService (PuppyWeightWatcher.ApiService)**
- **ApiService** uses libraries: Shared (models), ServiceDefaults (logging, config)
- **AppHost** aggregates and orchestrates Web + ApiService for local development
- Internal communication via project references (Shared, ServiceDefaults)
- Uses OutputCache, HTTP client, static assets in Web

## 4. External Dependencies & Environment Variables
- **Database**: Evidence of SQL Server (EntityFrameworkCore.SqlServer, Aspire.Hosting.SqlServer), but no explicit connection string in versioned appsettings.json. Configuration is likely via environment variable or dev secret (AppHost has UserSecretsId).
- **Cloud/Other**: OpenAPI, service discovery/libs for future cloud infra integration are present.
- **Environment Variables Required** (potential examples, confirm in config pipeline):
    - Connection strings for SQL Server (eg. `ConnectionStrings__Default`)
    - ASPNETCORE_ENVIRONMENT
    - Logging/Telemetry endpoints if set via env

## 5. Pipeline/CI Integration
- No explicit CI/CD pipeline config found at root (e.g., no .github/workflows, azure-pipelines.yml) in scanned directories. This may need to be added for Azure deployment.

## 6. Ready for Architecture Planning
- All major components, frameworks, and dependencies are discovered and classified.
- No discovered environmental secrets committed; ready for Azure migration architecture planning phase.

---
