# VS Code Debugging Setup

This workspace is configured for optimal development experience in Visual Studio Code with full-stack debugging support.

## 🚀 Quick Start

1. **Open in VS Code**: Open the workspace folder in VS Code
2. **Install Extensions**: Accept the recommended extensions when prompted
3. **Start Debugging**: Press `F5` or select "Launch Full Stack (Frontend + Backend)" from the debug menu

## 🐛 Debug Configurations

### Primary Configurations

#### **Launch Full Stack (Frontend + Backend)** ⭐
- **Shortcut**: `F5`
- **Description**: Starts both backend and frontend with debugging enabled
- **Ports**: Backend on `http://localhost:5055`, Frontend on `http://localhost:3000`
- **Best for**: Full development workflow

#### **Launch Backend (.NET)**
- **Description**: Starts only the ASP.NET Core backend with debugging
- **Features**: Breakpoints, variable inspection, call stack
- **URL**: `http://localhost:5055` + Swagger UI at `/swagger`

#### **Launch Frontend (Next.js)**  
- **Description**: Starts only the Next.js frontend with debugging
- **Features**: JavaScript/TypeScript debugging, React DevTools support
- **URL**: `http://localhost:3000`

## ⚙️ Available Tasks

Access via `Ctrl+Shift+P` → "Tasks: Run Task":

### Build Tasks
- **`build-backend`** - Builds the .NET project
- **`build-frontend`** - Builds the Next.js project  
- **`clean-backend`** - Cleans .NET build artifacts

### Development Tasks
- **`start-backend-dev`** - Runs backend in development mode
- **`start-frontend-dev`** - Runs frontend in development mode
- **`start-full-stack`** - Runs both frontend and backend
- **`install-frontend-deps`** - Installs/updates npm dependencies

## 🔧 Workspace Settings

The workspace includes optimized settings for:

- **C# Development**: OmniSharp configuration, formatting
- **TypeScript/React**: ESLint, Prettier formatting
- **File Management**: Excludes build artifacts, shows relevant files
- **Performance**: Optimized file watching and searching

## 📦 Recommended Extensions

The following extensions are automatically recommended:

### Core Development
- **C# Dev Kit** - C# language support and debugging
- **ESLint** - JavaScript/TypeScript linting
- **Prettier** - Code formatting
- **Tailwind CSS IntelliSense** - CSS class autocomplete

### Enhanced Experience  
- **Auto Rename Tag** - HTML/JSX tag renaming
- **Path Intellisense** - File path autocomplete
- **IntelliCode** - AI-assisted development
- **SQLite Viewer** - View SQLite database files

## 🐛 Debugging Features

### Backend (.NET) Debugging
- **Breakpoints**: Set breakpoints in C# code
- **Variable Inspection**: Hover over variables to see values
- **Call Stack**: Navigate through method calls
- **Exception Handling**: Break on exceptions
- **Hot Reload**: Code changes apply without restart

### Frontend (Next.js) Debugging  
- **Source Maps**: Debug TypeScript/JSX source code
- **Browser DevTools**: Integrated Chrome debugging
- **React DevTools**: Component tree inspection
- **Network Requests**: Monitor API calls to backend

### Full Stack Debugging
- **Concurrent Debugging**: Debug both frontend and backend simultaneously
- **API Testing**: Use Swagger UI while debugging backend
- **Database Inspection**: View SQLite files with recommended extensions

## 🚀 Development Workflow

### 1. Start Development
```bash
# Option 1: Use VS Code (Recommended)
Press F5 → "Launch Full Stack"

# Option 2: Manual start
Ctrl+Shift+P → "Tasks: Run Task" → "start-full-stack"
```

### 2. Access Services
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5055  
- **Swagger UI**: http://localhost:5055/swagger

### 3. Debug Workflow
- Set breakpoints in both C# and TypeScript code
- Use browser DevTools for frontend debugging
- Monitor API requests in Network tab
- Inspect SQLite database with recommended extensions

## 🔍 Troubleshooting

### Common Issues

**Backend won't start**:
- Ensure port 5055 is available
- Check `dotnet --version` (requires .NET 9.0+)
- Run `dotnet restore` in backend folder

**Frontend won't start**:
- Ensure port 3000 is available  
- Check `node --version` (requires Node.js 18+)
- Run `npm install` in frontend folder

**Debugging not working**:
- Install recommended C# and Node.js extensions
- Restart VS Code after installing extensions
- Check console output for error messages

### Reset Environment
```bash
# Clean and rebuild everything
dotnet clean backend/
rm -rf frontend/node_modules
rm -rf frontend/.next
dotnet restore backend/
npm install --prefix frontend
```

## 💡 Pro Tips

- Use `Ctrl+Shift+D` to quickly access debug configurations
- Use `Ctrl+`` to open integrated terminal
- Use `Ctrl+Shift+P` to access all commands and tasks
- Install "Thunder Client" extension for API testing within VS Code
- Use "GitLens" extension for enhanced Git integration

Happy debugging! 🚀
