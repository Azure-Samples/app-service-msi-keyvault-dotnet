## Prerequisites
To run and deploy this sample, you need the following:
1. Azure subscription to create an App Service and a Key Vault. 
2. [Azure CLI 2.0](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) (To run the application on your local development machine)

## Step 1: Create an App Service with a Managed Service Identity (MSI), a Key Vault, and grant the App Service access to the Key Vault. 
<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fazsamples.blob.core.windows.net%2Ftemplates%2Fazuredeploy.json" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>

Use the "Deploy to Azure" button to deploy an ARM template to create these resources. In addition to creating an App Service and Key Vault, the ARM template also grants the App Service access to fetch secrets from the Key Vault.

Review the resources created using the Azure portal. You should see an App Service and a Key Vault. View the access policies of the Key Vault to see that the App Service has access to it. 

## Step 2: Grant yourself data plane access to the Key Vault
Using the Azure Portal, in the Key Vault's access policies, grant yourself **Secret Management** access to the Key Vault. This will allow you to run the application on your local development machine. 

## Step 3: Clone the repo 
Clone the repo to your development machine. 

The project has two Nuget packages added in addition to the ones that a default Web App has. These are:
1. Microsoft.Azure.Services.AppAuthentication - this is in Preview and makes it easy to fetch access tokens for service to Azure service authentication scenarios. 
2. Microsoft.Azure.KeyVault - contains methods for interacting with Azure Key Vault. 

The relevant code is in WebAppKeyVault/WebAppKeyVault/Controllers/HomeController.cs file. The AzureServiceTokenProvider class (which is part of Microsoft.Azure.Services.AppAuthentication) tries the following methods to get an access token, to call Key Vault:-
1. Managed Service Identity (MSI) - for scenarios where the code is deployed to Azure, and the Azure resource supports MSI. 
2. [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) (for local development) - Azure CLI version 2.0.12 and above supports the **get-access-token** option. AzureServiceTokenProvider uses this option to get an access token for local development. 
3. Active Directory Integrated Authentication (for local development). To use integrated Windows authentication, your domain’s Active Directory must be federated with Azure Active Directory. Your application must be running on a domain-joined machine under a user’s domain credentials.

```csharp
    
public async System.Threading.Tasks.Task<ActionResult> Index()
{
    AzureServiceTokenProvider azureServiceTokenProvider = new AzureServiceTokenProvider();

    var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));

    var secret = await keyVaultClient.GetSecretAsync("https://keyvaultname.vault.azure.net/secrets/secret").ConfigureAwait(false);

    ViewBag.Secret = secret.Value;

    ViewBag.Principal = azureServiceTokenProvider.PrincipalUsed;

    return View();
}
```

## Step 4: Change the key vault name
In the HomeController.cs file, change the Key Vault name to the one you just created. Replace **KeyVaultName** with the name of your Key Vault. 

## Step 5: Run the application on your local development machine
Since this is on the development machine, AzureServiceTokenProvider will use the developer's security context to get a token to authenticate to Key Vault. This removes the need to create a service principal, and share it with the development team. It also prevents credentials from being checked in to source code. 
AzureServiceTokenProvider will use **Azure CLI** or **Active Directory Integrated Authentication** to authenticate to Azure AD to get a token. That token will be used to fetch the secret from Azure Key Vault. 

Azure CLI will work if the following conditions are met:
 1. You have [Azure CLI 2.0](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) installed. Version 2.0.12 supports the get-access-token option used by AzureServiceTokenProvider. If you have an earlier version, please upgrade. 
 2. You are logged into Azure CLI. You can login using **az login** command.
 
Azure Active Directory Authentication will only work if the following conditions are met:
 1. Your on-premise active directory is synced with Azure AD. 
 2. You are running this code on a domain joined machine.   

Since your developer account has access to the Key Vault, you should see the secret on the web page. Principal Used will show type "User" and your user account. 

You can also use a service principal to run the application on your local development machine. See the section "Running the application using a service principal" later in the tutorial on how to do this. 

## Step 6: Deploy the Web App to Azure
Use any of the methods outlined on [Deploy your app to Azure App Service](https://docs.microsoft.com/en-us/azure/app-service-web/web-sites-deploy) to publish the Web App to Azure. 
After you deploy it, browse to the web app. You should see the secret on the web page, and this time the Principal Used will show "App", since it ran under the context of the App Service. 
The AppId of the MSI will be displayed. 

## Summary
The web app was able to get a secret at runtime from Azure Key Vault using your developer account during development, and using MSI when deployed to Azure. 
As a result, you did not have to explicitly handle a service principal credential to authenticate to Azure AD to get a token to call Key Vault. You do not have to worry about renewing the service principal credential either, since MSI takes care of that.  

## Troubleshooting

### Common issues during local development:

1. Azure CLI is not installed, or you are not logged in, or you do not have the latest version. 
Run **az account get-access-token** to see if Azure CLI shows a token for you. If it says no such program found, please install Azure CLI 2.0. If you have installed it, you may be prompted to login. 

2. AzureServiceTokenProvider cannot find the path for Azure CLI.
AzureServiceTokenProvider finds Azure CLI at its default install locations. If it cannot find Azure CLI, please set environment variable **AzureCLIPath** to the Azure CLI installation folder. AzureServiceTokenProvider will add the environment variable to the Path environment variable.

### Common issues when deployed to Azure App Service:

1. MSI is not setup on the App Service. 

Check the environment variables MSI_ENDPOINT and MSI_SECRET exist using [Kudu debug console](https://azure.microsoft.com/en-us/resources/videos/super-secret-kudu-debug-console-for-azure-web-sites/). If these environment variables do not exist, MSI is not enabled on the App Service. 

### Common issues across environments:

1. Access denied

The principal used does not have access to the Key Vault. The principal used in show on the web page. Grant that user (in case of developer context) or application "Get secret" access to the Key Vault. 

## Running the application using a service principal in local development environment

Note: It is recommended to use your developer context for local development, since you do not need to create or share a service principal for that. If that does not work for you, you can use a service principal, but do not check in the certificate or secret in source repos, and share them securely. 

To run the application using a service principal in the local development environment, follow these steps

Service principal using a certificate:
1. Create a service principal certificate. Follow steps [here](https://docs.microsoft.com/en-us/azure/key-vault/key-vault-use-from-web-application) to create a service principal and grant it permissions to the Key Vault. 
2. Set an environment variable named **AzureServicesAuthConnectionString** to **RunAs=App;AppId=AppId;TenantId=TenantId;CertificateThumbprint=Thumbprint;CertificateStoreLocation=CurrentUser**. 
You need to replace AppId, TenantId, and Thumbprint with actual values from step #1.
3. Run the application in your local development environment. No code change is required. AzureServiceTokenProvider will use this environment variable and use the certificate to authenticate to Azure AD. 

Service principal using a password:
1. Create a service principal with a password. Follow steps [here](https://docs.microsoft.com/en-us/azure/key-vault/key-vault-use-from-web-application) to create a service principal and grant it permissions to the Key Vault. 
2. Set an environment variable named **AzureServicesAuthConnectionString** to **RunAs=App;AppId=AppId;TenantId=TenantId;AppKey=Secret**. You need to replace AppId, TenantId, and Secret with actual values from step #1. 
3. Run the application in your local development environment. No code change is required. AzureServiceTokenProvider will use this environment variable and use the service principal to authenticate to Azure AD. 