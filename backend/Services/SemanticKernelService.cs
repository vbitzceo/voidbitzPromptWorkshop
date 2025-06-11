using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using VoidBitzPromptWorkshop.API.Models;
using System.Text.RegularExpressions;
using Azure.Identity;
using Azure.AI.OpenAI;

namespace VoidBitzPromptWorkshop.API.Services;

public interface ISemanticKernelService
{
    Task<string> ExecutePromptAsync(PromptTemplate promptTemplate, Dictionary<string, object> variables);
    string ReplaceVariables(string content, Dictionary<string, object> variables);
}

public class SemanticKernelService : ISemanticKernelService
{
    private readonly Kernel _kernel;
    private readonly ILogger<SemanticKernelService> _logger;    public SemanticKernelService(ILogger<SemanticKernelService> logger, IConfiguration configuration)
    {
        _logger = logger;
        
        // Initialize Semantic Kernel
        var builder = Kernel.CreateBuilder();
        
        // Check for different AI service configurations
        var aiProvider = configuration["AI:Provider"]?.ToLower() ?? "openai";
        
        switch (aiProvider)
        {
            case "ollama":
                ConfigureOllama(builder, configuration);
                break;
            case "azure":
            case "azureopenai":
                ConfigureAzureOpenAI(builder, configuration);
                break;
            case "openai":
            default:
                ConfigureOpenAI(builder, configuration);
                break;
        }
        
        _kernel = builder.Build();
    }

    private void ConfigureOllama(IKernelBuilder builder, IConfiguration configuration)
    {
        var ollamaEndpoint = configuration["Ollama:Endpoint"] ?? "http://localhost:11434";
        var modelName = configuration["Ollama:Model"] ?? "llama3.2";
        
        try
        {
            // Configure Ollama using OpenAI-compatible endpoint
            builder.AddOpenAIChatCompletion(
                modelId: modelName,
                apiKey: "ollama", // Ollama doesn't require a real API key
                endpoint: new Uri($"{ollamaEndpoint}/v1")
            );
            
            _logger.LogInformation("Configured Ollama with model: {Model} at endpoint: {Endpoint}", 
                modelName, ollamaEndpoint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure Ollama. Falling back to mock responses.");
        }
    }    private void ConfigureAzureOpenAI(IKernelBuilder builder, IConfiguration configuration)
    {
        var azureEndpoint = configuration["AzureOpenAI:Endpoint"];
        var azureApiKey = configuration["AzureOpenAI:ApiKey"];
        var deploymentName = configuration["AzureOpenAI:DeploymentName"];
        var useManagedIdentity = configuration.GetValue<bool>("AzureOpenAI:UseManagedIdentity", false);
        
        if (!string.IsNullOrEmpty(azureEndpoint) && !string.IsNullOrEmpty(deploymentName))
        {
            try
            {
                if (useManagedIdentity)
                {
                    // Use Managed Identity for authentication (Azure best practice)
                    _logger.LogInformation("Configuring Azure OpenAI with Managed Identity authentication");
                    
                    var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
                    {
                        // Try managed identity first, then fall back to other methods in development
                        ManagedIdentityClientId = configuration["AzureOpenAI:ManagedIdentityClientId"]
                    });
                      // Create Azure OpenAI client with managed identity
                    var azureClient = new AzureOpenAIClient(new Uri(azureEndpoint), credential);
                    
                    builder.AddAzureOpenAIChatCompletion(
                        deploymentName: deploymentName,
                        azureOpenAIClient: azureClient
                    );
                    
                    _logger.LogInformation("Configured Azure OpenAI with Managed Identity - Deployment: {DeploymentName} at endpoint: {Endpoint}", 
                        deploymentName, azureEndpoint);
                }
                else if (!string.IsNullOrEmpty(azureApiKey))
                {
                    // Use API Key authentication (fallback for development)
                    _logger.LogWarning("Using API Key authentication for Azure OpenAI. Consider using Managed Identity for production.");
                      builder.AddAzureOpenAIChatCompletion(
                        deploymentName: deploymentName,
                        endpoint: azureEndpoint,
                        apiKey: azureApiKey
                    );
                    
                    _logger.LogInformation("Configured Azure OpenAI with API Key - Deployment: {DeploymentName} at endpoint: {Endpoint}", 
                        deploymentName, azureEndpoint);
                }
                else
                {
                    _logger.LogError("Azure OpenAI configuration error: Either set UseManagedIdentity=true or provide an ApiKey");
                    throw new InvalidOperationException("Azure OpenAI requires either Managed Identity or API Key authentication");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to configure Azure OpenAI. Error: {ErrorMessage}", ex.Message);
                throw; // Re-throw to ensure proper error handling
            }
        }
        else
        {
            var missingConfig = new List<string>();
            if (string.IsNullOrEmpty(azureEndpoint)) missingConfig.Add("Endpoint");
            if (string.IsNullOrEmpty(deploymentName)) missingConfig.Add("DeploymentName");
            
            _logger.LogError("Azure OpenAI configuration incomplete. Missing: {MissingConfig}", string.Join(", ", missingConfig));
            throw new InvalidOperationException($"Azure OpenAI configuration incomplete. Missing: {string.Join(", ", missingConfig)}");
        }
    }

    private void ConfigureOpenAI(IKernelBuilder builder, IConfiguration configuration)
    {
        var openAiApiKey = configuration["OpenAI:ApiKey"];
        var openAiModel = configuration["OpenAI:Model"] ?? "gpt-3.5-turbo";
        
        if (!string.IsNullOrEmpty(openAiApiKey))
        {
            try
            {
                builder.AddOpenAIChatCompletion(
                    modelId: openAiModel,
                    apiKey: openAiApiKey);
                
                _logger.LogInformation("Configured OpenAI with model: {Model}", openAiModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to configure OpenAI. Falling back to mock responses.");
            }
        }
        else
        {
            _logger.LogWarning("No OpenAI API key configured. Using mock responses for demonstration.");
        }
    }

    public async Task<string> ExecutePromptAsync(PromptTemplate promptTemplate, Dictionary<string, object> variables)
    {
        const int maxRetries = 3;
        const int baseDelayMs = 1000;

        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                // Replace variables in the prompt content
                var processedContent = ReplaceVariables(promptTemplate.Content, variables);

                _logger.LogInformation("Executing prompt: {PromptName} (Attempt: {Attempt})", promptTemplate.Name, attempt + 1);
                _logger.LogDebug("Processed content: {Content}", processedContent);

                // Check if we have a configured chat service
                var chatService = _kernel.Services.GetService<IChatCompletionService>();
                if (chatService == null)
                {
                    _logger.LogWarning("No chat completion service configured. Using mock response.");
                    return GenerateMockResponse(promptTemplate, processedContent);
                }

                // Execute the prompt using Semantic Kernel with timeout
                using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(2));
                var result = await _kernel.InvokePromptAsync(processedContent, cancellationToken: cts.Token);

                _logger.LogInformation("Successfully executed prompt: {PromptName}", promptTemplate.Name);
                return result.ToString();
            }
            catch (OperationCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "Prompt execution timed out for {PromptName}", promptTemplate.Name);
                return GenerateMockResponse(promptTemplate, promptTemplate.Content);
            }
            catch (Exception ex) when (attempt < maxRetries && IsTransientError(ex))
            {
                var delay = TimeSpan.FromMilliseconds(baseDelayMs * Math.Pow(2, attempt));
                _logger.LogWarning(ex, "Transient error executing prompt {PromptName} (Attempt {Attempt}/{MaxRetries}). Retrying in {Delay}ms",
                    promptTemplate.Name, attempt + 1, maxRetries + 1, delay.TotalMilliseconds);

                await Task.Delay(delay);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing prompt {PromptName} (Attempt {Attempt})", promptTemplate.Name, attempt + 1);

                if (attempt == maxRetries)
                {
                    // Return a mock response as fallback for demonstration
                    return GenerateMockResponse(promptTemplate, promptTemplate.Content);
                }
            }
        }

        return GenerateMockResponse(promptTemplate, promptTemplate.Content);
    }

    private static bool IsTransientError(Exception ex)
    {
        // Check for common transient errors that should be retried
        return ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("rate limit", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("throttle", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("429", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("502", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("503", StringComparison.OrdinalIgnoreCase) ||
               ex.Message.Contains("504", StringComparison.OrdinalIgnoreCase);
    }

    public string ReplaceVariables(string content, Dictionary<string, object> variables)
    {
        if (string.IsNullOrEmpty(content))
            return content;

        // Replace variables in the format {{variableName}}
        var pattern = @"\{\{(\w+)\}\}";
        
        return Regex.Replace(content, pattern, match =>
        {
            var variableName = match.Groups[1].Value;
            
            if (variables.TryGetValue(variableName, out var value))
            {
                return value?.ToString() ?? string.Empty;
            }
            
            // If variable not found, keep the placeholder
            _logger.LogWarning("Variable {VariableName} not found in provided variables", variableName);
            return match.Value;
        });
    }

    private static string GenerateMockResponse(PromptTemplate promptTemplate, string processedContent)
    {
        // Generate a mock response based on the prompt type
        return promptTemplate.Name.ToLower() switch
        {
            var name when name.Contains("code") => GenerateCodeReviewMock(processedContent),
            var name when name.Contains("blog") => GenerateBlogPostMock(processedContent),
            var name when name.Contains("review") => GenerateCodeReviewMock(processedContent),
            var name when name.Contains("write") || name.Contains("content") => GenerateContentMock(processedContent),
            _ => GenerateGenericMock(promptTemplate.Name, processedContent)
        };
    }

    private static string GenerateCodeReviewMock(string content)
    {
        return @"## Code Review Results

### Positive Aspects:
- Code structure is well-organized
- Variable naming follows conventions
- Logic flow is clear and readable

### Areas for Improvement:
1. **Error Handling**: Consider adding try-catch blocks for better error handling
2. **Performance**: Some operations could be optimized for better performance
3. **Documentation**: Add more inline comments for complex logic

### Security Considerations:
- Input validation looks good
- No obvious security vulnerabilities detected

### Recommendations:
- Consider using async/await for I/O operations
- Add unit tests for better code coverage
- Follow SOLID principles for better maintainability

**Overall Rating: 8/10** - Good code with room for minor improvements.

*Note: This is a simulated response. Configure your OpenAI API key for actual AI-powered reviews.*";
    }

    private static string GenerateBlogPostMock(string content)
    {
        return @"# Sample Blog Post

## Introduction
Welcome to this engaging blog post that captures your attention from the very first sentence. In today's digital landscape, creating compelling content is more important than ever.

## Main Content Sections

### Section 1: Understanding the Basics
Every great piece of content starts with a solid foundation. Here we explore the fundamental concepts that form the backbone of our topic.

### Section 2: Advanced Techniques
Building on our foundation, we dive deeper into sophisticated strategies and methodologies that can take your understanding to the next level.

### Section 3: Practical Applications
Theory is valuable, but practical application is where real learning happens. Let's explore real-world examples and use cases.

## Conclusion
As we wrap up this comprehensive exploration, remember that consistent practice and continuous learning are key to mastering any subject. Take action today and start implementing these insights in your own work.

**Call to Action**: Share your thoughts in the comments below and let us know how you plan to apply these concepts!

*Note: This is a simulated response. Configure your OpenAI API key for actual AI-generated content.*";
    }

    private static string GenerateContentMock(string content)
    {
        return @"Here's your AI-generated content based on your prompt:

This is a sample response that demonstrates how the system would process your request. The content has been tailored to match your specifications and requirements.

Key points covered:
• Relevant and engaging information
• Well-structured presentation
• Clear and concise language
• Actionable insights

*Note: This is a simulated response. Configure your OpenAI API key for actual AI-powered content generation.*";
    }

    private static string GenerateGenericMock(string promptName, string content)
    {
        return $@"## Response to '{promptName}'

Thank you for your request. This is a simulated response to demonstrate the prompt execution functionality.

**Your prompt was processed successfully!**

The system has analyzed your input and generated this response based on the template configuration and provided variables.

### Features Demonstrated:
- Variable substitution
- Template processing
- Response generation
- Error handling

*Note: This is a simulated response. Configure your OpenAI API key in appsettings.json to enable actual AI-powered responses.*

**Sample Configuration:**
```json
{{
  ""OpenAI"": {{
    ""ApiKey"": ""your-openai-api-key"",
    ""Model"": ""gpt-3.5-turbo""
  }}
}}
```";
    }
}
