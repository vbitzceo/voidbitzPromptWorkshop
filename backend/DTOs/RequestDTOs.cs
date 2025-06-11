using System.ComponentModel.DataAnnotations;
using VoidBitzPromptWorkshop.API.Models;

namespace VoidBitzPromptWorkshop.API.DTOs;

public class CreatePromptTemplateRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public string? CategoryId { get; set; }
    
    public List<string> Tags { get; set; } = new();
    
    public List<PromptVariable> Variables { get; set; } = new();
}

public class UpdatePromptTemplateRequest
{
    [Required]
    public string Id { get; set; } = string.Empty;
    
    public string? Name { get; set; }
    
    public string? Description { get; set; }
    
    public string? Content { get; set; }
    
    public string? CategoryId { get; set; }
    
    public List<string>? Tags { get; set; }
    
    public List<PromptVariable>? Variables { get; set; }
}

public class ExecutePromptRequest
{
    [Required]
    public string PromptTemplateId { get; set; } = string.Empty;
    
    public Dictionary<string, object> Variables { get; set; } = new();
}

public class ImportYamlRequest
{
    [Required]
    public string YamlContent { get; set; } = string.Empty;
}

public class CreateCategoryRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Color { get; set; } = "#3B82F6";
}

public class UpdateCategoryRequest
{
    public string? Name { get; set; }
    
    public string? Description { get; set; }
    
    public string? Color { get; set; }
}

public class CreateTagRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Color { get; set; } = "#3B82F6";
}

public class UpdateTagRequest
{
    public string? Name { get; set; }
    
    public string? Description { get; set; }
    
    public string? Color { get; set; }
}
