# 🎯 Migrate to SQL Server with EF Core (Aspire Integration)

## Understanding
Replace the in-memory `List<T>` storage in PuppyService with a SQL Server database using Entity Framework Core. Aspire will manage the SQL Server container locally and the connection string wiring automatically.

## Assumptions
- SQL Server container managed by Aspire AppHost for local dev (no manual DB setup needed)
- EF Core with code-first approach; migration applied on startup via `EnsureCreated` for simplicity
- ShotRecord needs a `PuppyId` foreign key added (currently only embedded in Puppy's list)
- WeightStatistics remains a computed DTO — not stored in DB
- The `IPuppyService` interface stays unchanged so the API layer is unaffected
- Photos (Base64Data) stored in the DB; acceptable for a personal app

## Approach
Add `Aspire.Hosting.SqlServer` to AppHost and wire a SQL Server resource into the API service. Add `Aspire.Microsoft.EntityFrameworkCore.SqlServer` to ApiService so it can consume the connection. Create a `PuppyDbContext` with DbSets for all four entities. Update shared models with EF Core navigation/FK properties. Rewrite `PuppyService` to inject the DbContext and use async EF Core queries. Apply DB creation on startup.

## Key Files
- PuppyWeightWatcher.AppHost/PuppyWeightWatcher.AppHost.csproj - add SQL Server hosting package
- PuppyWeightWatcher.AppHost/AppHost.cs - add SQL Server resource + wire to API
- PuppyWeightWatcher.ApiService/PuppyWeightWatcher.ApiService.csproj - add EF Core SQL Server Aspire package
- PuppyWeightWatcher.ApiService/Data/PuppyDbContext.cs - new DbContext
- PuppyWeightWatcher.ApiService/Program.cs - register DbContext, apply on startup
- PuppyWeightWatcher.Shared/Models/ShotRecord.cs - add PuppyId FK
- PuppyWeightWatcher.ApiService/Services/PuppyService.cs - rewrite to use EF Core

## Risks & Open Questions
- Large photo blobs in SQL may become slow at scale; fine for a personal litter tracker
- Using EnsureCreated instead of migrations for simplicity; can add formal migrations later

**Progress**: 100% [██████████]

**Last Updated**: 2026-03-13 22:59:49

## 📝 Plan Steps
- ✅ **Add Aspire SQL Server hosting NuGet package to AppHost project**
- ✅ **Update AppHost.cs to add SQL Server resource and wire it to the API service**
- ✅ **Add Aspire EF Core SQL Server NuGet package to ApiService project**
- ✅ **Update ShotRecord model to add PuppyId foreign key**
- ✅ **Create PuppyDbContext with DbSets and relationship configuration**
- ✅ **Update ApiService Program.cs to register DbContext and apply DB creation on startup**
- ✅ **Rewrite PuppyService to use EF Core DbContext**
- ✅ **Build and verify**

