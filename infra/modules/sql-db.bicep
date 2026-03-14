targetScope = 'resourceGroup'

@description('Base name for the SQL Server/Database')
param name string
@description('Location of the resource')
param location string = resourceGroup().location
@description('Tags to apply to the resource')
param tags object = {}

var resourceSuffix = take(uniqueString(subscription().id, resourceGroup().name, name), 6)
var serverName = toLower('${name}-sql-${resourceSuffix}')
var databaseName = toLower('${name}-db-${resourceSuffix}')

resource sqlServer 'Microsoft.Sql/servers@2023-08-01' = {
  name: serverName
  location: location
  tags: union(tags, { 'azd-service-name': name })
  properties: {
    administratorLogin: 'sqladminuser'
    administratorLoginPassword: '' // Admin password to be deployed separately via Key Vault!
    publicNetworkAccess: 'Disabled'
    version: '12.0'
  }
}

resource sqlDB 'Microsoft.Sql/servers/databases@2023-08-01' = {
  name: '${serverName}/${databaseName}'
  location: location
  tags: union(tags, { 'azd-service-name': name })
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
    maxSizeBytes: 2147483648
    readScale: 'Disabled'
    zoneRedundant: false
    sampleName: 'AdventureWorksLT'
  }
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
  dependsOn: [ sqlServer ]
}

output sqlServerName string = sqlServer.name
output sqlDatabaseName string = sqlDB.name
output sqlServerResourceId string = sqlServer.id
output sqlDatabaseResourceId string = sqlDB.id
