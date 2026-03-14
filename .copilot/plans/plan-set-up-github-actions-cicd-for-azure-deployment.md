# 🎯 Set Up GitHub Actions CI/CD for Azure Deployment

## Understanding
Create a complete GitHub Actions CI/CD pipeline that automatically deploys the PuppyWeightWatcher app to Azure whenever code is pushed to the master branch. This requires creating the workflow file, an Azure AD app with federated credentials, and configuring GitHub repository variables.

## Assumptions
- The user's GitHub repo is at `https://github.com/duanekord/PuppyWatcher` on branch `master`
- Azure subscription ID is `582d055a-b828-4677-adc8-230750602ecc`
- Azure tenant ID is `4905713e-8e1f-48b5-bf17-c4fd8660b76e`
- azd environment name is `puppyweightwatcher`, location is `eastus2`
- The `az` CLI is logged in from the previous session
- Need to check if `gh` CLI is available for setting GitHub variables

## Approach
Refresh the PATH and verify `az` login, then create the Azure AD app registration with OIDC federated credentials for GitHub Actions. Create a service principal with Contributor role. Create the workflow YAML file. Finally, use `gh` CLI (if available) or instruct the user to set GitHub repository variables.

## Key Files
- .github/workflows/deploy.yml - GitHub Actions workflow for CI/CD

## Risks & Open Questions
- The `gh` CLI may not be installed, in which case GitHub variables must be set manually
- The `az` CLI session may have expired

**Progress**: 100% [██████████]

**Last Updated**: 2026-03-14 03:29:03

## 📝 Plan Steps
- ✅ **Verify az CLI login is active and refresh PATH**
- ✅ **Create Azure AD app registration for GitHub Actions**
- ✅ **Add federated credentials for the GitHub repo master branch**
- ✅ **Create service principal and assign Contributor role on the subscription**
- ✅ **Create the .github/workflows/deploy.yml workflow file**
- ✅ **Configure GitHub repository variables using gh CLI or provide manual instructions**
- ✅ **Update README.md with CI/CD documentation**

