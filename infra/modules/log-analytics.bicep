targetScope = 'resourceGroup'

@description('Base name for the Log Analytics Workspace')
param name string
@description('Location of the resource')
param location string = resourceGroup().location
@description('Tags to apply to the resource')
param tags object = {}

var resourceSuffix = take(uniqueString(subscription().id, resourceGroup().name, name), 6)
var workspaceName = '${name}-${resourceSuffix}'

resource workspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: workspaceName
  location: location
  tags: union(tags, {
    'azd-service-name': name
  })
  properties: {
    features: {
      legacy: 0
      searchVersion: 1
    }
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
  }
}

output workspaceId string = workspace.id
output workspaceName string = workspace.name
