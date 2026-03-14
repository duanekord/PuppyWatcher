targetScope = 'resourceGroup'

@description('Base name for the Container App')
param name string
@description('Location of the resource')
param location string = resourceGroup().location
@description('Container Apps Environment resource ID')
param containerAppEnvironmentId string
@description('Container image to run')
param containerImage string
@description('Tags to apply to the resource')
param tags object = {}

var resourceSuffix = take(uniqueString(subscription().id, resourceGroup().name, name), 6)
var appName = '${name}-${resourceSuffix}'

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${appName}-mi'
  location: location
  tags: union(tags, { 'azd-service-name': name })
}

resource app 'Microsoft.App/containerApps@2023-05-01' = {
  name: appName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  tags: union(tags, {
    'azd-service-name': name
  })
  properties: {
    managedEnvironmentId: containerAppEnvironmentId
    configuration: {
      ingress: {
        external: true
        targetPort: 80
      }
      secrets: [] // Use Key Vault references for secrets at deployment
    }
    template: {
      containers: [
        {
          name: appName
          image: containerImage
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          env: []
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
        rules: []
      }
    }
  }
}

output containerAppName string = app.name
output containerAppId string = app.id
output managedIdentityId string = managedIdentity.id
