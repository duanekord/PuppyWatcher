targetScope = 'resourceGroup'

@description('Base name for the Key Vault')
param name string
@description('Location of the resource')
param location string = resourceGroup().location
@description('Tags to apply to the resource')
param tags object = {}

var resourceSuffix = take(uniqueString(subscription().id, resourceGroup().name, name), 6)
var keyVaultName = toLower('${name}-${resourceSuffix}')

resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' = {
  name: keyVaultName
  location: location
  tags: union(tags, {
    'azd-service-name': name
  })
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true
    enablePurgeProtection: true
    enableSoftDelete: true
    publicNetworkAccess: 'Enabled'
    accessPolicies: []
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Allow'
    }
  }
}

output keyVaultUri string = keyVault.properties.vaultUri
output keyVaultName string = keyVault.name
