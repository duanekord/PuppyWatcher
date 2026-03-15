# 🎯 Integrate Azure Key Vault for Auth Secrets

## Understanding
The user wants to stop manually setting `AUTH_MICROSOFT_CLIENTID` and `AUTH_MICROSOFT_CLIENTSECRET` via `azd env set` on every fresh environment. The solution is to add Azure Key Vault to the Aspire AppHost so it's provisioned with the rest of the infra, store the auth secrets there once, and have the webfrontend read them from Key Vault as a configuration source.

## Assumptions
- The Aspire SDK is 13.1.0 (based on csproj), so we use `Aspire.Hosting.Azure.KeyVault` and `Aspire.Azure.Security.KeyVault` packages at compatible versions
- The existing `Authentication:Microsoft:ClientId` configuration key pattern in `Program.cs` should continue to work via Key Vault's config provider (which maps `Authentication--Microsoft--ClientId` secret names to `Authentication:Microsoft:ClientId` configuration keys)
- The managed identity already assigned to the container app will be granted access to Key Vault by Aspire's infrastructure generation
- Local development will continue using user secrets (unchanged)

## Approach
We'll add the `Aspire.Hosting.Azure.KeyVault` package to the [AppHost](PuppyWeightWatcher.AppHost/PuppyWeightWatcher.AppHost.csproj) and the `Aspire.Azure.Security.KeyVault` package to the [Web project](PuppyWeightWatcher.Web/PuppyWeightWatcher.Web.csproj). In [AppHost.cs](PuppyWeightWatcher.AppHost/AppHost.cs), we'll replace the parameter-based approach with a Key Vault resource referenced by the webfrontend. In [Program.cs](PuppyWeightWatcher.Web/Program.cs), we'll register the Key Vault client as a configuration source. Finally, we'll delete the stale infra templates so `azd` regenerates them with Key Vault included.

## Key Files
- PuppyWeightWatcher.AppHost/PuppyWeightWatcher.AppHost.csproj - add Aspire.Hosting.Azure.KeyVault package
- PuppyWeightWatcher.AppHost/AppHost.cs - replace parameters with Key Vault resource
- PuppyWeightWatcher.Web/PuppyWeightWatcher.Web.csproj - add Aspire.Azure.Security.KeyVault package
- PuppyWeightWatcher.Web/Program.cs - register Key Vault config source
- PuppyWeightWatcher.AppHost/infra/webfrontend.tmpl.yaml - delete stale template

## Risks & Open Questions
- After first deployment, user must manually add secrets to Key Vault once via Azure CLI
- Local development is unaffected (user secrets still work)

**Progress**: 100% [██████████]

**Last Updated**: 2026-03-15 00:21:16

## 📝 Plan Steps
- ✅ **Add Aspire.Hosting.Azure.KeyVault package to AppHost csproj**
- ✅ **Add Aspire.Azure.Security.KeyVault package to Web csproj**
- ✅ **Modify AppHost.cs to add Key Vault resource and remove parameter-based secrets**
- ✅ **Modify Web Program.cs to register Key Vault as a configuration source**
- ✅ **Delete stale infra templates so azd regenerates them with Key Vault**
- ✅ **Build to verify all changes compile**
- ✅ **Provide post-deployment instructions for setting secrets in Key Vault**

