using Microsoft.EntityFrameworkCore;
using VoidBitzPromptWorkshop.API.Data;
using VoidBitzPromptWorkshop.API.Models;
using VoidBitzPromptWorkshop.API.DTOs;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VoidBitzPromptWorkshop.API.Services;

public interface IPromptService
{
    Task<IEnumerable<PromptTemplate>> GetAllPromptsAsync();
    Task<PromptTemplate?> GetPromptByIdAsync(string id);
    Task<PromptTemplate> CreatePromptAsync(CreatePromptTemplateRequest request);
    Task<PromptTemplate?> UpdatePromptAsync(string id, UpdatePromptTemplateRequest request);
    Task<bool> DeletePromptAsync(string id);
    Task<PromptExecution> ExecutePromptAsync(ExecutePromptRequest request);
    Task<string> ExportPromptToYamlAsync(string id);
    Task<PromptTemplate> ImportPromptFromYamlAsync(ImportYamlRequest request);
}

public class PromptService : IPromptService
{
    private readonly ApplicationDbContext _context;
    private readonly ISemanticKernelService _semanticKernelService;
    private readonly ILogger<PromptService> _logger;

    public PromptService(
        ApplicationDbContext context,
        ISemanticKernelService semanticKernelService,
        ILogger<PromptService> logger)
    {
        _context = context;
        _semanticKernelService = semanticKernelService;
        _logger = logger;
    }

    public async Task<IEnumerable<PromptTemplate>> GetAllPromptsAsync()
    {
        return await _context.PromptTemplates
            .Include(p => p.Category)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync();
    }

    public async Task<PromptTemplate?> GetPromptByIdAsync(string id)
    {
        return await _context.PromptTemplates
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PromptTemplate> CreatePromptAsync(CreatePromptTemplateRequest request)
    {
        var yamlTemplate = await GenerateYamlTemplateAsync(request);
        
        var prompt = new PromptTemplate
        {
            Name = request.Name,
            Description = request.Description,
            Content = request.Content,
            CategoryId = request.CategoryId,
            Tags = request.Tags,
            Variables = request.Variables,
            YamlTemplate = yamlTemplate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.PromptTemplates.Add(prompt);
        await _context.SaveChangesAsync();

        return await GetPromptByIdAsync(prompt.Id) ?? prompt;
    }

    public async Task<PromptTemplate?> UpdatePromptAsync(string id, UpdatePromptTemplateRequest request)
    {
        var prompt = await _context.PromptTemplates.FindAsync(id);
        if (prompt == null)
            return null;

        // Update only provided fields
        if (request.Name != null)
            prompt.Name = request.Name;
        
        if (request.Description != null)
            prompt.Description = request.Description;
        
        if (request.Content != null)
            prompt.Content = request.Content;
        
        if (request.CategoryId != null)
            prompt.CategoryId = request.CategoryId;
        
        if (request.Tags != null)
            prompt.Tags = request.Tags;
        
        if (request.Variables != null)
            prompt.Variables = request.Variables;

        prompt.UpdatedAt = DateTime.UtcNow;
        
        // Regenerate YAML template
        prompt.YamlTemplate = await GenerateYamlTemplateAsync(new CreatePromptTemplateRequest
        {
            Name = prompt.Name,
            Description = prompt.Description,
            Content = prompt.Content,
            CategoryId = prompt.CategoryId,
            Tags = prompt.Tags,
            Variables = prompt.Variables
        });

        await _context.SaveChangesAsync();

        return await GetPromptByIdAsync(id);
    }

    public async Task<bool> DeletePromptAsync(string id)
    {
        var prompt = await _context.PromptTemplates.FindAsync(id);
        if (prompt == null)
            return false;

        _context.PromptTemplates.Remove(prompt);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PromptExecution> ExecutePromptAsync(ExecutePromptRequest request)
    {
        var prompt = await GetPromptByIdAsync(request.PromptTemplateId);
        if (prompt == null)
            throw new ArgumentException($"Prompt template with ID {request.PromptTemplateId} not found");

        // Validate required variables
        var missingRequiredVars = prompt.Variables
            .Where(v => v.Required && !request.Variables.ContainsKey(v.Name))
            .Select(v => v.Name)
            .ToList();

        if (missingRequiredVars.Any())
            throw new ArgumentException($"Missing required variables: {string.Join(", ", missingRequiredVars)}");

        // Execute the prompt
        var result = await _semanticKernelService.ExecutePromptAsync(prompt, request.Variables);

        // Save execution record
        var execution = new PromptExecution
        {
            PromptTemplateId = request.PromptTemplateId,
            Variables = request.Variables,
            Result = result,
            ExecutedAt = DateTime.UtcNow
        };

        _context.PromptExecutions.Add(execution);
        await _context.SaveChangesAsync();

        return execution;
    }

    public async Task<string> ExportPromptToYamlAsync(string id)
    {
        var prompt = await GetPromptByIdAsync(id);
        if (prompt == null)
            throw new ArgumentException($"Prompt template with ID {id} not found");

        return await GenerateYamlTemplateAsync(new CreatePromptTemplateRequest
        {
            Name = prompt.Name,
            Description = prompt.Description,
            Content = prompt.Content,
            CategoryId = prompt.CategoryId,
            Tags = prompt.Tags,
            Variables = prompt.Variables
        });
    }

    public async Task<PromptTemplate> ImportPromptFromYamlAsync(ImportYamlRequest request)
    {
        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yamlData = deserializer.Deserialize<YamlPromptTemplate>(request.YamlContent);

            // Get category ID if category name is provided
            string? categoryId = null;
            if (!string.IsNullOrEmpty(yamlData.Category))
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name == yamlData.Category);
                categoryId = category?.Id;
            }

            // Get tag IDs if tag names are provided
            var tagIds = new List<string>();
            if (yamlData.Tags?.Any() == true)
            {
                var tags = await _context.Tags
                    .Where(t => yamlData.Tags.Contains(t.Name))
                    .Select(t => t.Id)
                    .ToListAsync();
                tagIds = tags;
            }

            var createRequest = new CreatePromptTemplateRequest
            {
                Name = yamlData.Name,
                Description = yamlData.Description ?? string.Empty,
                Content = yamlData.Content,
                CategoryId = categoryId,
                Tags = tagIds,
                Variables = yamlData.Variables ?? new List<PromptVariable>()
            };

            return await CreatePromptAsync(createRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing YAML prompt template");
            throw new ArgumentException("Invalid YAML format or content", ex);
        }
    }

    private async Task<string> GenerateYamlTemplateAsync(CreatePromptTemplateRequest request)
    {
        var yamlData = new YamlPromptTemplate
        {
            Name = request.Name,
            Description = request.Description,
            Content = request.Content,
            Variables = request.Variables
        };

        // Get category name if ID is provided
        if (!string.IsNullOrEmpty(request.CategoryId))
        {
            var category = await _context.Categories.FindAsync(request.CategoryId);
            yamlData.Category = category?.Name;
        }

        // Get tag names if IDs are provided
        if (request.Tags.Any())
        {
            var tags = await _context.Tags
                .Where(t => request.Tags.Contains(t.Id))
                .Select(t => t.Name)
                .ToListAsync();
            yamlData.Tags = tags;
        }

        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        return serializer.Serialize(yamlData);
    }

    private class YamlPromptTemplate
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Content { get; set; } = string.Empty;
        public List<PromptVariable>? Variables { get; set; }
        public string? Category { get; set; }
        public List<string>? Tags { get; set; }
    }
}
