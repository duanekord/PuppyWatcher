# Puppy Weight Watcher

A .NET Aspire Blazor application for tracking puppy weight, vaccinations, and photos. Installable as a PWA on mobile devices.

## Live App

**https://webfrontend.bravetree-1f7b14aa.eastus2.azurecontainerapps.io/**

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

## CI/CD with GitHub Actions

This repository includes a GitHub Actions workflow (`.github/workflows/deploy.yml`) that automatically deploys to Azure on every push to the `master` branch.

### How It Works

1. Push code to `master` (or merge a pull request)
2. GitHub Actions runs the workflow automatically
3. The workflow authenticates to Azure using OIDC federated credentials (no secrets stored)
4. `azd up` provisions any infrastructure changes and deploys the updated app

You can also trigger a deployment manually from the **Actions** tab in GitHub using the "Run workflow" button.

### Configuration

The workflow uses these GitHub repository variables (already configured):

| Variable | Value |
|---|---|
| `AZURE_CLIENT_ID` | App registration client ID |
| `AZURE_TENANT_ID` | Azure AD tenant ID |
| `AZURE_SUBSCRIPTION_ID` | Azure subscription ID |
| `AZURE_ENV_NAME` | `puppyweightwatcher` |
| `AZURE_LOCATION` | `eastus2` |

These can be viewed/changed at **Settings** → **Secrets and variables** → **Actions** → **Variables** in your GitHub repository.
