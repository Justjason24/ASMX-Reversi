@description('Project name used for resource naming')
param projectName string = 'reversi'

@description('Container image tag to deploy')
param imageTag string = 'latest'

var location = resourceGroup().location
var acrName = '${projectName}acr${uniqueString(resourceGroup().id)}'
var envName = '${projectName}-env'

module acr 'modules/acr.bicep' = {
  name: 'acr'
  params: {
    acrName: acrName
    location: location
  }
}

module environment 'modules/environment.bicep' = {
  name: 'environment'
  params: {
    envName: envName
    location: location
    projectName: projectName
  }
}

module apiApp 'modules/api-app.bicep' = {
  name: 'api-app'
  params: {
    appName: '${projectName}-api'
    location: location
    environmentId: environment.outputs.environmentId
    acrLoginServer: acr.outputs.loginServer
    acrUsername: acr.outputs.adminUsername
    acrPassword: acr.outputs.adminPassword
    imageTag: imageTag
    frontendUrl: 'https://${frontendApp.outputs.fqdn}'
  }
}

module frontendApp 'modules/frontend-app.bicep' = {
  name: 'frontend-app'
  params: {
    appName: '${projectName}-frontend'
    location: location
    environmentId: environment.outputs.environmentId
    acrLoginServer: acr.outputs.loginServer
    acrUsername: acr.outputs.adminUsername
    acrPassword: acr.outputs.adminPassword
    imageTag: imageTag
  }
}

output acrLoginServer string = acr.outputs.loginServer
output apiUrl string = 'https://${apiApp.outputs.fqdn}'
output frontendUrl string = 'https://${frontendApp.outputs.fqdn}'
