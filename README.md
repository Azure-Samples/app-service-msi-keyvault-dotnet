---
page_type: sample
languages:
  - csharp
products:
  - azure
  - dotnet
  - aspnet
  - azure-app-service
  - azure-key-vault
description: "This sample shows how to use Azure KeyVault from App Service with Managed Service Identity (MSI)."
urlFragment: keyvault-msi-appservice-sample
---

# Use Key Vault from App Service with Managed Identity

## Background
For Service-to-Azure-Service authentication, the approach so far involved creating an Azure AD application and associated credential, and using that credential to get a token. The sample [here] shows how this approach is used to authenticate to Azure Key Vault from a Web App. While this approach works well, there are two shortcomings:
1. The Azure AD application credentials are typically hard coded in source code. Developers tend to push the code to source repositories as-is, which leads to credentials in source.
2. The Azure AD application credentials expire, and so need to be renewed, else can lead to application downtime.

With [Managed Identity], both problems are solved. This sample shows how a Web App can authenticate to Azure Key Vault without the need to explicitly create an Azure AD application or manage its credentials. 

>Here's another [sample] that shows how to programmatically deploy an ARM template from a .NET Console application running on an Azure VM with a Managed Identity.

>Here's another [.NET Core sample] that shows how to programmatically call Azure Services from an Azure Linux VM with a Managed Identity.

## Prerequisites

To complete this tutorial:

* Install [Azure CLI 2.0] to run the application on your local development machine.

If you don't have an Azure subscription, create a [free account] before you begin.

### Create an App Service with a Managed Identity
<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FAzure-Samples%2Fapp-service-msi-keyvault-dotnet%2Fmaster%2Fazuredeploy.json" target="_blank">
    <img src="http://azuredeploy.net/deploybutton.png"/>
</a>

Use the "Deploy to Azure" button to deploy an ARM template to create the following resources:
1. App Service with Managed Identity.
2. Key Vault with a secret, and an access policy that grants the App Service access to **Get Secrets**.
>Note: When filling out the template you will see a textbox labelled 'Key Vault Secret'. Enter a secret value there. A secret with the name 'secret' and value from what you entered will be created in the Key Vault.

Review the resources created using the Azure portal. You should see an App Service and a Key Vault. View the access policies of the Key Vault to see that the App Service has access to it.

### Grant yourself data plane access to the Key Vault

Step 1: Set access policy.

*  Go to the [Azure Portal] and log in using your Azure account
*  Search for your Key Vault in **Search Resources dialog box**
*  Select **Overview** > **Access policies**
*  Click on **Add Access Policy** > **Secret permissions** > **Get**
*  Click on **Select Principal**, add your account and pre created **system-assigned identity**
*  Click on "OK" to add the new Access Policy, then click "Save" to save the Access Policy

Step 2: Copy and save key vault url.

Select **Overview** > **DNS Name**, copy the associated **key vault url** to the clipboard, then paste it into a text editor for later use.

## Run the application
Clone the repo to your development machine. 

```bash
git clone https://github.com/Azure-Samples/app-service-msi-keyvault-dotnet.git
```

### Run the application on your local development machine
This solution requires a key vault url be stored in an environment variable on the machine running the sample, and require [register an application with the Microsoft identity platform],
then grant the access policy by `Step1: Set access policy`.

Linux

```bash
export KEY_VAULT_URI="<YourKeyVaultUrl>"
```

Windows

```cmd
setx KEY_VAULT_URI "<YourKeyVaultUrl>"
```

### Deploy the Web App to Azure
Use any of the methods outlined on [Deploy your app to Azure App Service] to publish the Web App to Azure.

Step 1: Set environment variable in app service.

*  Search for your app service in **Search Resources dialog box**
*  Select **Setting** > **Configuration** > **New application setting**
*  Set the name to **KEY_VAULT_URI** and value with your **key vault url**  

After you deploy it, browse to the web app. You should see the secret on the web page, and this time the Principal Used will show "App", since it ran under the context of the App Service. 
The AppId of the MSI will be displayed. 

## Summary
The web app was successfully able to get a secret at runtime from Azure Key Vault using your developer account during development, and using MSI when deployed to Azure, without any code change between local development environment and Azure. 
As a result, you did not have to explicitly handle a service principal credential to authenticate to Azure AD to get a token to call Key Vault. You do not have to worry about renewing the service principal credential either, since MSI takes care of that.  

## Troubleshooting

Please see the [troubleshooting section] of the AppAuthentication library documentation for troubleshooting of common issues.

<!-- LINKS -->
[here]: https://docs.microsoft.com/en-us/azure/key-vault/key-vault-use-from-web-application
[Managed Identity]: https://docs.microsoft.com/en-us/azure/app-service/app-service-managed-service-identity
[sample]: https://github.com/Azure-Samples/windowsvm-msi-arm-dotnet
[.NET Core sample]: https://github.com/Azure-Samples/linuxvm-msi-keyvault-arm-dotnet
[Azure CLI 2.0]: https://docs.microsoft.com/en-us/cli/azure/install-azure-cli?view=azure-cli-latest
[free account]: https://azure.microsoft.com/free/?WT.mc_id=A261C142F
[Azure Portal]: https://portal.azure.com
[register an application with the Microsoft identity platform]: https://docs.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app
[Deploy your app to Azure App Service]: https://docs.microsoft.com/en-us/azure/app-service-web/web-sites-deploy
[troubleshooting section]：https://docs.microsoft.com/en-us/azure/key-vault/service-to-service-authentication#appauthentication-troubleshooting