# 🎯 Fix SQL Server Password Mismatch and Add Persistent Storage

## Understanding
The user's production SQL Server container in ACA has data they need to preserve. The SA password in the API's connection string doesn't match what the SQL Server was initialized with (likely due to a redeployment that changed the password parameter but didn't reinitialize SQL Server). They also need persistent storage so future container restarts don't lose data.

## Assumptions
- The SQL Server container is running and has data
- The `sql_password` secured parameter value changed between deployments, causing the mismatch
- The SQL Server container's `MSSQL_SA_PASSWORD` env var holds the password it was originally initialized with, but SA password is set at first startup and doesn't change when the env var changes
- Azure Files can be used for persistent storage in ACA

## Approach
First, fix the credential mismatch by exec'ing into the running SQL container to change the SA password to match what the API expects. Use the current `MSSQL_SA_PASSWORD` env var (which SQL Server accepted at init time) to authenticate with `sqlcmd`, then `ALTER LOGIN` to set the new password.

Second, modify `sql.tmpl.yaml` to add an Azure Files persistent volume mount at `/var/opt/mssql/data` so SQL Server data files survive container restarts.

## Key Files
- `PuppyWeightWatcher.AppHost/infra/sql.tmpl.yaml` - needs volume mount for persistence

## Risks & Open Questions
- The user needs to know their resource group name and current `sql_password` parameter value
- Azure Files storage must be created in the same region as the ACA environment
- The exec approach requires the SQL container to have `sqlcmd` available (standard SQL Server images include it)

**Progress**: 100% [██████████]

**Last Updated**: 2026-03-14 06:31:23

## 📝 Plan Steps
- ✅ **Provide CLI commands to fix the SA password on the running SQL container**
- ✅ **Modify sql.tmpl.yaml to add Azure Files persistent volume mount for SQL Server data**

