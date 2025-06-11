using System.ComponentModel.DataAnnotations;

namespace VoidBitzPromptWorkshop.API.Models;

public class PromptTemplate
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string Content { get; set; } = string.Empty;
    
    public string YamlTemplate { get; set; } = string.Empty;
    
    public string? CategoryId { get; set; }
    
    public List<string> Tags { get; set; } = new();
    
    public List<PromptVariable> Variables { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public Category? Category { get; set; }
}

public class PromptVariable
{
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Type { get; set; } = "string"; // string, number, boolean
    
    public bool Required { get; set; }
    
    public string? DefaultValue { get; set; }
}

public class Category
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Color { get; set; } = "#3B82F6";
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public List<PromptTemplate> PromptTemplates { get; set; } = new();
}

public class Tag
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string Color { get; set; } = "#3B82F6";
}

public class PromptExecution
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string PromptTemplateId { get; set; } = string.Empty;
    
    public Dictionary<string, object> Variables { get; set; } = new();
    
    public string Result { get; set; } = string.Empty;
    
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public PromptTemplate? PromptTemplate { get; set; }
}
