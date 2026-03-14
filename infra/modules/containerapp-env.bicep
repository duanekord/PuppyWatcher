targetScope = 'resourceGroup'

@description('Base name for the Container Apps Environment')
param name string
@description('Location of the resource')
param location string = resourceGroup().location
@description('Log Analytics Workspace resource ID')
param logAnalyticsWorkspaceId string
@description('Tags to apply to the resource')
param tags object = {}

var resourceSuffix = take(uniqueString(subscription().id, resourceGroup().name, name), 6)
var envName = '${name}-${resourceSuffix}'

resource containerAppsEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
  name: envName
  location: location
  tags: union(tags, {
    'azd-service-name': name
  })
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: reference(logAnalyticsWorkspaceId, '2023-04-01').customerId
        sharedKey: '' // secret is managed via RBAC/Key Vault
      }
    }
    zoneRedundant: false
  }
}

output environmentId string = containerAppsEnv.id
output environmentName string = containerAppsEnv.name
