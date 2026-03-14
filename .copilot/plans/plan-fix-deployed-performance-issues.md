# 🎯 Fix Deployed Performance Issues

## Understanding
The app is slow on Azure due to an underpowered SQL tier, missing database indexes, unnecessary sample data, no connection resilience, and no response compression.

## Assumptions
- The SQL Basic tier (5 DTUs) is the primary bottleneck
- Database indexes on foreign key columns will significantly improve query performance
- EF Core retry logic will prevent transient Azure SQL failures from causing visible delays
- Standard S0 tier (10 DTUs) is still very affordable (~$15/mo) and 2x the capacity

## Approach
Fix at both the infrastructure and code levels. Upgrade SQL tier, remove sample data, add database indexes, enable EF Core retry logic, and add response compression.

## Key Files
- infra/modules/sql-db.bicep - SQL tier upgrade, remove sample data
- PuppyWeightWatcher.ApiService/Data/PuppyDbContext.cs - Add database indexes on foreign keys
- PuppyWeightWatcher.ApiService/Program.cs - Add connection resilience and response compression

## Risks & Open Questions
- SQL tier upgrade increases cost from ~$5/mo to ~$15/mo
- Database indexes will be created on next EnsureCreatedAsync run (existing data unaffected)

**Progress**: 80% [████████░░]

**Last Updated**: 2026-03-14 03:49:48

## 📝 Plan Steps
- ✅ **Upgrade SQL tier from Basic to Standard S0 and remove AdventureWorksLT sample data in sql-db.bicep**
- ✅ **Add database indexes on PuppyId foreign key columns in PuppyDbContext**
- ✅ **Add EF Core connection resilience with retry logic in API service Program.cs**
- ⏭️ **Add response compression to the API service Program.cs**
- ✅ **Build and verify changes compile successfully**

