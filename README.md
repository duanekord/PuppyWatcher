# Puppy Weight Watcher

A .NET Aspire Blazor application for tracking puppy weight, vaccinations, and photos. Installable as a PWA on mobile devices.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://docs.docker.com/desktop/setup/install/windows-install/) (for local development)
- [Azure Developer CLI (`azd`)](https://aka.ms/azd-install) (for Azure deployment)
- [Azure CLI (`az`)](https://learn.microsoft.com/cli/azure/install-azure-cli) (for Azure deployment)

## Running Locally

1. Start Docker Desktop.
2. Set **PuppyWeightWatcher.AppHost** as the startup project.
3. Press **F5** in Visual Studio.

The Aspire dashboard URL will appear in the output window.

## Azure Deployment

### First-Time Setup

```powershell
# Log in to Azure
azd auth login
az login --tenant '4905713e-8e1f-48b5-bf17-c4fd8660b76e'

# Initialize the project (only needed once)
azd init -e puppyweightwatcher

# Set the Azure region
azd env set AZURE_LOCATION eastus2

# Deploy
azd up --no-prompt
```

### Redeploying After Code Changes

After making code changes, redeploy with a single command from the solution root:

```powershell
azd up --no-prompt
```

This will rebuild the container images, push them to Azure Container Registry, and update the running Container Apps. Infrastructure changes (if any) are applied automatically.

To deploy **only code** without re-provisioning infrastructure:

```powershell
azd deploy
```

### Tearing Down

To delete **all** Azure resources created by the deployment:

```powershell
azd down
```

To also purge soft-deleted resources (Key Vault, SQL Server) so the names can be reused:

```powershell
azd down --purge
```

> **Note:** `azd down` will permanently delete your Azure SQL database and all data. Make sure to back up any data you want to keep before running this command.
