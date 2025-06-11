# Azure OpenAI Integration Guide

This guide explains how to configure and use Azure OpenAI with the voidBitz Prompt Workshop application, following Azure best practices for authentication, security, and reliability.

## Overview

The application supports multiple AI providers with Azure OpenAI as the primary enterprise-grade option. The implementation follows Azure best practices including:

- **Managed Identity Authentication** (recommended for production)
- **API Key Authentication** (development fallback)
- **Retry Logic with Exponential Backoff**
- **Comprehensive Error Handling**
- **Security Best Practices**

## Configuration Options

### 1. Azure OpenAI with Managed Identity (Recommended)

**Best for: Production environments, Azure-hosted applications**

```json
{
  "AI": {
    "Provider": "azure"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-resource-name.openai.azure.com/",
    "DeploymentName": "gpt-35-turbo",
    "UseManagedIdentity": true,
    "ManagedIdentityClientId": "optional-client-id-for-user-assigned-identity"
  }
}
```

**Setup Steps:**
1. Create an Azure OpenAI resource in Azure Portal
2. Deploy a model (e.g., GPT-3.5-turbo, GPT-4)
3. Enable Managed Identity on your Azure service (App Service, Container Instances, etc.)
4. Grant the managed identity "Cognitive Services OpenAI User" role on the Azure OpenAI resource
5. Configure the application settings as shown above

### 2. Azure OpenAI with API Key

**Best for: Development, testing, non-Azure hosted environments**

```json
{
  "AI": {
    "Provider": "azure"
  },
  "AzureOpenAI": {
    "Endpoint": "https://your-resource-name.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-35-turbo",
    "UseManagedIdentity": false
  }
}
```

**Setup Steps:**
1. Create an Azure OpenAI resource in Azure Portal
2. Deploy a model (e.g., GPT-3.5-turbo, GPT-4)
3. Get the API key from Azure Portal > Azure OpenAI > Keys and Endpoint
4. Configure the application settings as shown above

**Security Note:** Never commit API keys to source control. Use Azure Key Vault or environment variables.

## Azure Resource Setup

### 1. Create Azure OpenAI Resource

```bash
# Using Azure CLI
az cognitiveservices account create \
  --name "your-openai-resource" \
  --resource-group "your-resource-group" \
  --location "East US" \
  --kind "OpenAI" \
  --sku "S0"
```

### 2. Deploy a Model

```bash
# Deploy GPT-3.5-turbo model
az cognitiveservices account deployment create \
  --resource-group "your-resource-group" \
  --name "your-openai-resource" \
  --deployment-name "gpt-35-turbo" \
  --model-name "gpt-35-turbo" \
  --model-version "0613" \
  --model-format "OpenAI" \
  --scale-settings-scale-type "Standard"
```

### 3. Configure Managed Identity (Production)

```bash
# Enable system-assigned managed identity on App Service
az webapp identity assign \
  --name "your-app-service" \
  --resource-group "your-resource-group"

# Grant access to Azure OpenAI
az role assignment create \
  --assignee "your-managed-identity-object-id" \
  --role "Cognitive Services OpenAI User" \
  --scope "/subscriptions/your-subscription-id/resourceGroups/your-resource-group/providers/Microsoft.CognitiveServices/accounts/your-openai-resource"
```

## Environment Variables

For containerized deployments or environment-specific configuration:

```bash
# Set environment variables
export AI__Provider="azure"
export AzureOpenAI__Endpoint="https://your-resource-name.openai.azure.com/"
export AzureOpenAI__DeploymentName="gpt-35-turbo"
export AzureOpenAI__UseManagedIdentity="true"

# For API key authentication (development only)
export AzureOpenAI__ApiKey="your-api-key"
export AzureOpenAI__UseManagedIdentity="false"
```

## Azure Key Vault Integration

For additional security, store sensitive configuration in Azure Key Vault:

```json
{
  "AzureOpenAI": {
    "Endpoint": "@Microsoft.KeyVault(SecretUri=https://your-keyvault.vault.azure.net/secrets/openai-endpoint/)",
    "ApiKey": "@Microsoft.KeyVault(SecretUri=https://your-keyvault.vault.azure.net/secrets/openai-apikey/)",
    "DeploymentName": "gpt-35-turbo"
  }
}
```

## Features Implemented

### 1. Authentication Methods
- **DefaultAzureCredential**: Automatically tries multiple authentication methods
- **Managed Identity**: Secure, no secrets to manage
- **API Key**: Simple setup for development

### 2. Error Handling & Reliability
- **Retry Logic**: Exponential backoff for transient failures
- **Circuit Breaker**: Prevents cascade failures
- **Timeout Management**: 2-minute timeout for prompt execution
- **Graceful Degradation**: Falls back to mock responses

### 3. Logging & Monitoring
- **Structured Logging**: JSON format for Azure Monitor
- **Performance Metrics**: Execution time tracking
- **Error Tracking**: Detailed error logging with context

### 4. Security Features
- **No Hardcoded Secrets**: All credentials from configuration
- **Principle of Least Privilege**: Minimal required permissions
- **Secure Transport**: HTTPS only connections

## Troubleshooting

### Common Issues

1. **Authentication Errors**
   ```
   Error: Unauthorized (401)
   ```
   - Check managed identity is enabled and has correct role assignment
   - Verify API key is correct and not expired

2. **Deployment Not Found**
   ```
   Error: The API deployment for this resource does not exist
   ```
   - Verify deployment name matches exactly
   - Check model is deployed and in "Succeeded" state

3. **Rate Limiting**
   ```
   Error: Rate limit exceeded (429)
   ```
   - Implemented automatic retry with exponential backoff
   - Consider upgrading to higher tier if needed

4. **Network Connectivity**
   ```
   Error: Timeout or connection issues
   ```
   - Check firewall rules and network security groups
   - Verify endpoint URL is correct

### Monitoring in Azure

1. **Application Insights**: Track performance and errors
2. **Azure Monitor**: Set up alerts for failures
3. **Log Analytics**: Query application logs

```kusto
// Query for AI service errors
traces
| where customDimensions.CategoryName == "VoidBitzPromptWorkshop.API.Services.SemanticKernelService"
| where severityLevel >= 3
| order by timestamp desc
```

## Cost Optimization

1. **Choose Appropriate Model**: Balance cost vs. capability
2. **Monitor Usage**: Use Azure Cost Management
3. **Implement Caching**: Cache common responses
4. **Set Budgets**: Configure spending alerts

## Best Practices Summary

✅ **Do:**
- Use Managed Identity in production
- Store secrets in Azure Key Vault
- Implement retry logic for resilience
- Monitor costs and usage
- Use least privilege access

❌ **Don't:**
- Hardcode API keys in source code
- Skip error handling
- Ignore rate limits
- Over-provision resources
- Use admin keys in production

## Additional Resources

- [Azure OpenAI Service Documentation](https://docs.microsoft.com/azure/cognitive-services/openai/)
- [Managed Identity Best Practices](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview)
- [Azure SDK for .NET](https://docs.microsoft.com/dotnet/azure/)
