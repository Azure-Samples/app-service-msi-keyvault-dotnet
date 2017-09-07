## Prerequisites
To run and deploy this sample, you need the following:
1. Azure subscription to create App Service and Key Vault. 
2. Visual Studio 2017
3. [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest) (To run the application on your local development machine)

## Step 1: Create an App Service with a Managed Service Identity, a Key Vault, and grant the App Service access to the Key Vault. 
[Deploy to Azure](https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fazsamples.blob.core.windows.net%2Ftemplates%2Fazuredeploy.json)

Use the "Deploy to Azure" link to deploy an ARM template to create these resources. The ARM template also grants the App Service access to fetch secrets from the Key Vault.

Review the resources created using the Azure portal. You should see an App Service and a Key Vault. View the access policies of the Key Vault to see that the App Service has access to it. 

## Step 2: Grant yourself "Get Secret" access to the Key Vault
Using the Azure Portal, in the Key Vault's access policies, grant yourself access to "Get secrets". This will allow you to run the application, and access the Key Vault in the local development environment. 

## Step 3: Clone the repo 
Clone the repo to your development machine, and open the solution using Visual Studio. 

The relevant code is in HomeController.cs file. The AzureServiceTokenProvider class tries the following methods to get an access token, to call Key Vault:-
1. Managed Service Identity (MSI) - for senarios where the code is deployed to Azure, and the Azure resource supports MSI. 
2. Azure CLI (for local development) - Azure CLI version 2.0.12 and above supports the get-access-token option. AzureServiceTokenProvider uses this option to get an access token for local development. 
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
In the HomeController.cs file, change the Key Vault name to the one you just created. Replace keyvaultname with the name of your Key Vault. 

## Step 5: Run the application on your local development machine
Since this is on the development machine, AzureServiceTokenProvider will run under the developer's security context. 
It will use **Azure CLI** or **Active Directory Integrated Authentication** to authenticate to Azure AD to get a token. That token will be used to fetch the secret from Azure Key Vault. 

Azure CLI will work if the following conditions are met:
 1. You have Azure CLI 2.0 installed. Version 2.0.12 supports the get-access-token option used by AzureServiceTokenProvider. If you have an earlier version, please upgrade. 
 2. You are logged into Azure CLI. You can login using "az login" command.
 
Azure Active Directory Authentication will only work if the following conditions are met:
 1. Your on-premise active directory is synced with Azure AD. 
 2. You are running this code on a domain joined machine.   

Since your developer account has access to the Key Vault, you should see the secret on the web page. Principal Used will show type "User" and your user account. 

## Step 6: Deploy the Web App to Azure
Right click on the project, and use the **Publish** option to deploy the Web App to Azure. Select the App Service you created using the ARM template, and publish to it. 
You should see the secret on the web page, and this time the Principal Used will show "App", since it ran under the context of the App Service. The AppId of the MSI will be displayed. 

## Summary
The web app was able to call Azure Key Vault using your developer account during development, and using MSI when deployed to Azure.
