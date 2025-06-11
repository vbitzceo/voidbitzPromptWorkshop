# voidBitz Prompt Workshop

A full-stack application for creating, managing, and executing LLM prompt templates with enterprise-grade Azure OpenAI integration and a focus on organization and reusability.

## 🌟 Features

### 🎨 **Modern User Experience**
- **Beautiful Modern UI**: Clean, responsive interface with gradient backgrounds, smooth animations, and polished styling
- **📊 Dashboard Analytics**: Visual stats showing prompt counts, categories, tags, and recent activity
- **🔍 Advanced Filtering**: Search, filter by category, and filter by multiple tags simultaneously
- **⚡ Real-time Updates**: Instant UI updates with loading states and success/error notifications

### 📝 **Prompt Management**
- **Rich Text Editing**: Create and edit prompt templates with syntax highlighting
- **📁 Smart Categorization**: Organize prompts by categories with custom colors and visual indicators
- **🏷️ Interactive Tagging**: Tag prompts with colorful labels for easy searching and filtering
- **🔧 Auto-Variable Detection**: Automatically detect and create variables from `{{variable}}` patterns
- **📤 YAML Import/Export**: Seamlessly import and export prompts in standardized YAML format

### 🤖 **AI Integration** 
- **🏢 Azure OpenAI**: Production-ready with Managed Identity authentication
- **🔑 OpenAI**: Alternative provider support
- **🏠 Ollama**: Local inference capabilities
- **🔄 Provider Switching**: Easy configuration-driven provider selection
- **🧪 Prompt Execution**: Test prompts with real AI responses or intelligent mock responses

### 🛡️ **Enterprise Features**
- **🔐 Managed Identity**: Secure Azure authentication without API keys
- **🔄 Retry Logic**: Exponential backoff for resilient AI service calls
- **📊 Monitoring**: Comprehensive logging and performance tracking
- **⏱️ Timeout Management**: Configurable operation timeouts
- **🛡️ Error Handling**: Graceful degradation with mock responses

## 🏗️ Architecture

### 🎨 **Frontend (Next.js 15)**
- **Framework**: Next.js 15 with TypeScript
- **Styling**: Tailwind CSS with custom animations and gradients
- **Design**: Modern glass-morphism UI with smooth transitions
- **Icons**: Lucide React with emoji accents
- **HTTP Client**: Axios with error handling
- **Notifications**: React Hot Toast with custom styling
- **Components**: Modular, reusable components with loading states

### ⚙️ **Backend (ASP.NET Core)**
- **Framework**: ASP.NET Core Web API (.NET 9)
- **AI Integration**: Microsoft Semantic Kernel with multi-provider support
- **Azure Integration**: Azure.AI.OpenAI v2.2.0 + Azure.Identity v1.12.0
- **Authentication**: Managed Identity (DefaultAzureCredential) + API Key fallback
- **Database**: Entity Framework with In-Memory Database (configurable)
- **Serialization**: System.Text.Json with circular reference handling
- **Documentation**: Swagger/OpenAPI with comprehensive API docs

### 🤖 **AI Provider Support**
- **Azure OpenAI**: Enterprise-grade with Managed Identity (Recommended)
- **OpenAI**: Direct API integration
- **Ollama**: Local inference server support
- **Mock Responses**: Intelligent fallback for demos and testing

## Getting Started

### Prerequisites
- Node.js 18+ and npm
- .NET 9 SDK
- (Optional) OpenAI API key for real AI responses

### Backend Setup

1. Navigate to the backend directory:
   ```powershell
   cd backend\VoidBitzPromptWorkshop.API
   ```

2. Restore dependencies:
   ```powershell
   dotnet restore
   ```

3. (Optional) Set OpenAI API key in `appsettings.Development.json`:
   ```json
   {
     "OpenAI": {
       "ApiKey": "your-api-key-here"
     }
   }
   ```

4. Run the backend:
   ```powershell
   dotnet run
   ```

The API will be available at `http://localhost:5055` with Swagger documentation at `http://localhost:5055/swagger`.

### Frontend Setup

1. Navigate to the frontend directory:
   ```powershell
   cd frontend
   ```

2. Install dependencies:
   ```powershell
   npm install
   ```

3. Start the development server:
   ```powershell
   npm run dev
   ```

The frontend will be available at `http://localhost:3000` (or `http://localhost:3001` if 3000 is in use).

## API Endpoints

### Prompts
- `GET /api/prompts` - Get all prompt templates
- `GET /api/prompts/{id}` - Get a specific prompt template
- `POST /api/prompts` - Create a new prompt template
- `PUT /api/prompts/{id}` - Update a prompt template
- `DELETE /api/prompts/{id}` - Delete a prompt template
- `POST /api/prompts/{id}/execute` - Execute a prompt template
- `POST /api/prompts/import-yaml` - Import prompts from YAML
- `GET /api/prompts/export-yaml` - Export all prompts to YAML

### Categories
- `GET /api/categories` - Get all categories
- `GET /api/categories/{id}` - Get a specific category
- `POST /api/categories` - Create a new category
- `PUT /api/categories/{id}` - Update a category
- `DELETE /api/categories/{id}` - Delete a category

### Tags
- `GET /api/tags` - Get all tags
- `GET /api/tags/{id}` - Get a specific tag
- `POST /api/tags` - Create a new tag
- `PUT /api/tags/{id}` - Update a tag
- `DELETE /api/tags/{id}` - Delete a tag

## Sample Data

The application comes with sample prompt templates, categories, and tags to demonstrate functionality:

### Categories
- **Content Creation**: For writing and content generation prompts
- **Web Development**: For coding and development-related prompts

### Tags
- **AI**: General AI-related prompts
- **Coding**: Programming and development prompts
- **Writing**: Content writing and creation prompts

### Sample Prompts
- **Code Review Assistant**: Reviews code and provides feedback
- **Blog Post Generator**: Generates blog post content with customizable parameters

## YAML Format

Prompts can be imported/exported in the following YAML format:

```yaml
prompts:
  - id: prompt-1
    name: "Example Prompt"
    description: "An example prompt template"
    content: "Write a {{type}} about {{topic}}"
    categoryId: cat-example
    tags: [tag-example]
    variables:
      - name: type
        description: "Type of content"
        type: string
        required: true
      - name: topic
        description: "Topic to write about"
        type: string
        required: true
        defaultValue: "AI"
```

## Development Notes

- The backend uses an in-memory database, so data is reset on each restart
- Mock AI responses are provided when no OpenAI API key is configured
- CORS is configured to allow frontend requests from localhost:3000 and localhost:3001
- JSON serialization uses circular reference handling to prevent infinite loops

## Technology Stack

### Frontend Dependencies
- React 18+ with Next.js 15
- TypeScript for type safety
- Tailwind CSS for styling
- Axios for HTTP requests
- Lucide React for icons
- React Hot Toast for notifications
- js-yaml for YAML parsing

### Backend Dependencies
- ASP.NET Core Web API
- Microsoft.SemanticKernel for AI integration
- Entity Framework Core with In-Memory provider
- YamlDotNet for YAML serialization
- Swashbuckle for API documentation

## License

This project is for educational and demonstration purposes.
