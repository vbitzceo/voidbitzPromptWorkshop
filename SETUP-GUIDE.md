# VoidBitz Prompt Workshop Setup Guide

## Quick Start

### Local Development Setup

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd voidBitzPromptWorkshop
   ```

2. **Backend Setup**
   ```bash
   cd backend
   dotnet restore
   ```

3. **Configure AI Provider** in `appsettings.Development.json`:
   ```json
   {
     "AI": {
       "Provider": "openai"  // or "ollama" for local inference
     },
     "OpenAI": {
       "ApiKey": "your-openai-api-key-here"
     }
   }
   ```

4. **Run the backend**:
   ```bash
   dotnet run
   ```
   - API will be available at: http://localhost:5055
   - Swagger UI: http://localhost:5055/swagger

5. **Frontend Setup** (in a new terminal):
   ```bash
   cd frontend
   npm install
   npm run dev
   ```
   - Frontend will be available at: http://localhost:3000

## Azure Production Deployment

### Backend Configuration

#### Option 1: Azure OpenAI with Managed Identity (Recommended for Production)

Configure `appsettings.Production.json`:
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
3. Enable Managed Identity on your Azure App Service
4. Grant the managed identity "Cognitive Services OpenAI User" role on the Azure OpenAI resource
5. Deploy to Azure App Service - Production configuration will be automatically used

#### Option 2: Azure OpenAI with API Key (Development/Testing)

Configure `appsettings.Production.json`:
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

2. **Deploy to Azure App Service** - The production configuration will be automatically used

### Environment Variables (Azure App Service)

If using API keys instead of Managed Identity, set these in Azure App Service Configuration:

- `AzureOpenAI__ApiKey` - Your Azure OpenAI API key
- `OpenAI__ApiKey` - Your OpenAI API key (if using OpenAI as fallback)

## Database

### Overview
- **Type:** SQLite (platform-agnostic, file-based database)
- **Development:** `VoidBitzPromptWorkshop_Dev.db`
- **Production:** `VoidBitzPromptWorkshop_Production.db`
- **Migrations:** Applied automatically on application startup
- **Seed Data:** Created automatically with sample prompts, categories, and tags

### Benefits of SQLite
✅ **Platform Agnostic** - Works on Windows, macOS, and Linux  
✅ **No Server Required** - File-based database  
✅ **Persistent Data** - Survives application restarts  
✅ **Easy Deployment** - Just copy the .db file  
✅ **Perfect for Development** - Lightweight and fast  
✅ **Production Ready** - Suitable for small to medium applications

### Database Files Created
- **Main Database:** `VoidBitzPromptWorkshop_[Environment].db`
- **Supporting Files:** 
  - `*.db-shm` (Shared memory file)
  - `*.db-wal` (Write-Ahead Log)

## AI Provider Options

| Provider | Use Case | Configuration Required |
|----------|----------|----------------------|
| **OpenAI** | Development, General use | API key |
| **Azure OpenAI** | Production, Enterprise | Endpoint + API key or Managed Identity |
| **Ollama** | Local inference, Privacy | Local Ollama installation |

### Setting up Ollama (Optional)

1. Install Ollama: https://ollama.ai/
2. Pull a model: `ollama pull llama3.2`
3. Update configuration:
   ```json
   {
     "AI": {
       "Provider": "ollama"
     },
     "Ollama": {
       "Endpoint": "http://localhost:11434",
       "Model": "llama3.2"
     }
   }
   ```

## Features

- ✅ **Prompt Template Management** - Create, edit, and organize prompt templates
- ✅ **Category Organization** - Group prompts by categories
- ✅ **Tag System** - Tag prompts with descriptions for easy filtering and understanding
- ✅ **Variable Substitution** - Dynamic variables in prompts
- ✅ **AI Integration** - OpenAI, Azure OpenAI, and Ollama support
- ✅ **Modern UI** - Next.js frontend with Tailwind CSS
- ✅ **RESTful API** - Clean ASP.NET Core Web API
- ✅ **Platform Agnostic** - SQLite database works everywhere

## API Endpoints

- `GET /api/prompts` - List all prompt templates
- `POST /api/prompts` - Create new prompt template
- `PUT /api/prompts/{id}` - Update prompt template
- `DELETE /api/prompts/{id}` - Delete prompt template
- `GET /api/categories` - List all categories
- `POST /api/categories` - Create new category
- `PUT /api/categories/{id}` - Update category
- `DELETE /api/categories/{id}` - Delete category
- `GET /api/tags` - List all tags with descriptions
- `POST /api/tags` - Create new tag with description
- `PUT /api/tags/{id}` - Update tag including description
- `DELETE /api/tags/{id}` - Delete tag

## Troubleshooting

### Common Issues

#### Database Issues
1. **Database not found**: Migrations are applied automatically on startup
2. **Permission errors**: Ensure the application has write permissions to the directory
3. **Migration errors**: Delete the database files and restart the application to recreate

#### AI Provider Issues
1. **Azure OpenAI not working**: 
   - Check API endpoint URL format
   - Verify deployment name matches your Azure OpenAI model deployment
   - Ensure Managed Identity has proper permissions
2. **OpenAI API errors**: Verify API key is valid and has sufficient credits
3. **Ollama connection failed**: Ensure Ollama is running locally on port 11434

#### Configuration Issues
1. **Wrong environment config**: Check `ASPNETCORE_ENVIRONMENT` variable
2. **Missing API keys**: Verify configuration in appsettings files
3. **CORS issues**: CORS is configured for development (localhost:3000)

### Logs and Debugging

#### Development
- Check console output for detailed error information
- Use Swagger UI (http://localhost:5055/swagger) to test API endpoints
- SQLite database files are created in the backend directory

#### Production (Azure)
- Check Azure App Service logs in Azure Portal
- Use Application Insights for detailed monitoring
- Verify environment variables in App Service Configuration

## Development Tips

### VS Code Setup (Recommended)

This project includes VS Code configuration for optimal development experience:

#### Debugging Configuration
- **Full Stack Launch**: Press `F5` or use "Launch Full Stack (Frontend + Backend)" to start both services
- **Individual Services**: 
  - "Launch Backend (.NET)" - Starts only the backend with debugging
  - "Launch Frontend (Next.js)" - Starts only the frontend with debugging
- **Breakpoints**: Set breakpoints in both C# and TypeScript code

#### Available Tasks (Ctrl+Shift+P → "Tasks: Run Task")
- `build-backend` - Build the .NET backend
- `build-frontend` - Build the Next.js frontend
- `start-full-stack` - Start both frontend and backend in development mode
- `clean-backend` - Clean the .NET build artifacts

#### Recommended Extensions
The project includes extension recommendations that will be suggested when you open the workspace.

### General Development Tips

- Use the Swagger UI (http://localhost:5055/swagger) to test API endpoints
- SQLite database files are created in the backend directory
- Configuration is environment-specific (Development vs Production)
- Hot reload is enabled for both frontend and backend during development
- Database migrations are applied automatically on startup
