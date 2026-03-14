targetScope = 'resourceGroup'

@description('Base name for the Azure Container Registry')
param name string
@description('Location of the resource')
param location string = resourceGroup().location
@description('Tags to apply to the resource')
param tags object = {}

var resourceSuffix = take(uniqueString(subscription().id, resourceGroup().name, name), 6)
var registryName = '${name}${resourceSuffix}'

resource acr 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: registryName
  location: location
  sku: {
    name: 'Basic'
  }
  tags: union(tags, {
    'azd-service-name': name
  })
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
  }
}

output registryName string = acr.name
output registryLoginServer string = acr.properties.loginServer
output registryResourceId string = acr.id
