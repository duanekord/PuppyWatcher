# 🎯 Deploy Aspire App to Azure

## Understanding
Set up the Azure deployment toolchain and deploy the PuppyWeightWatcher .NET Aspire app to Azure Container Apps with Azure SQL Database. The user has Docker and an Azure subscription but needs the Azure CLI and Azure Developer CLI installed.

## Assumptions
- The user has an active Azure subscription
- Docker Desktop is installed and running
- The Aspire AppHost is correctly configured with SQL Server, API service, and web frontend
- No code changes are needed — `azd` will map Aspire resources to Azure equivalents automatically

## Approach
Install the two required CLIs (Azure CLI and Azure Developer CLI), authenticate the user, then use `azd init` to detect the Aspire project structure and `azd up` to provision and deploy everything. The `azd` tool will automatically map `AddSqlServer` to Azure SQL, and `AddProject` to Azure Container Apps.

## Key Files
- PuppyWeightWatcher.AppHost/AppHost.cs - Aspire resource definitions that azd reads to provision Azure resources

## Risks & Open Questions
- CLI installs via winget require the terminal to be restarted for PATH changes to take effect
- `azd up` will prompt the user to select a subscription and region interactively
- Azure SQL firewall rules may need configuration for local access after deployment

**Progress**: 83% [████████░░]

**Last Updated**: 2026-03-14 02:25:06

## 📝 Plan Steps
- ✅ **Install Azure CLI via winget**
- ✅ **Install Azure Developer CLI (azd) via winget**
- ✅ **Restart terminal and verify both CLIs are available**
- ✅ **Log in to Azure with azd auth login**
- ✅ **Initialize the Aspire project for Azure deployment with azd init**
- 🔄 **Deploy to Azure with azd up**

