using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoidBitzPromptWorkshop.API.Services;
using VoidBitzPromptWorkshop.API.DTOs;
using VoidBitzPromptWorkshop.API.Data;

namespace VoidBitzPromptWorkshop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromptsController : ControllerBase
{
    private readonly IPromptService _promptService;
    private readonly ISemanticKernelService _semanticKernelService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PromptsController> _logger;

    public PromptsController(IPromptService promptService, ISemanticKernelService semanticKernelService, ApplicationDbContext context, ILogger<PromptsController> logger)
    {
        _promptService = promptService;
        _semanticKernelService = semanticKernelService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all prompt templates
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetAllPrompts()
    {
        try
        {
            var prompts = await _promptService.GetAllPromptsAsync();
            return Ok(prompts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all prompts");
            return StatusCode(500, "An error occurred while retrieving prompts");
        }
    }

    /// <summary>
    /// Get a specific prompt template by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetPromptById(string id)
    {
        try
        {
            var prompt = await _promptService.GetPromptByIdAsync(id);
            if (prompt == null)
                return NotFound($"Prompt with ID {id} not found");

            return Ok(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prompt {PromptId}", id);
            return StatusCode(500, "An error occurred while retrieving the prompt");
        }
    }

    /// <summary>
    /// Create a new prompt template
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreatePrompt([FromBody] CreatePromptTemplateRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var prompt = await _promptService.CreatePromptAsync(request);
            return CreatedAtAction(nameof(GetPromptById), new { id = prompt.Id }, prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating prompt");
            return StatusCode(500, "An error occurred while creating the prompt");
        }
    }

    /// <summary>
    /// Update an existing prompt template
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdatePrompt(string id, [FromBody] UpdatePromptTemplateRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            request.Id = id; // Ensure the ID matches the route
            var prompt = await _promptService.UpdatePromptAsync(id, request);
            
            if (prompt == null)
                return NotFound($"Prompt with ID {id} not found");

            return Ok(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating prompt {PromptId}", id);
            return StatusCode(500, "An error occurred while updating the prompt");
        }
    }

    /// <summary>
    /// Delete a prompt template
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePrompt(string id)
    {
        try
        {
            var success = await _promptService.DeletePromptAsync(id);
            if (!success)
                return NotFound($"Prompt with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting prompt {PromptId}", id);
            return StatusCode(500, "An error occurred while deleting the prompt");
        }
    }

    /// <summary>
    /// Execute a prompt template with provided variables
    /// </summary>
    [HttpPost("execute")]
    public async Task<ActionResult> ExecutePrompt([FromBody] ExecutePromptRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var execution = await _promptService.ExecutePromptAsync(request);
            return Ok(execution);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid prompt execution request");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing prompt");
            return StatusCode(500, "An error occurred while executing the prompt");
        }
    }

    /// <summary>
    /// Export a prompt template as YAML
    /// </summary>
    [HttpGet("{id}/export-yaml")]
    public async Task<ActionResult> ExportPromptToYaml(string id)
    {
        try
        {
            var yamlContent = await _promptService.ExportPromptToYamlAsync(id);
            return Ok(yamlContent);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Prompt not found for export: {PromptId}", id);
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting prompt {PromptId} to YAML", id);
            return StatusCode(500, "An error occurred while exporting the prompt");
        }
    }

    /// <summary>
    /// Import a prompt template from YAML
    /// </summary>
    [HttpPost("import-yaml")]
    public async Task<ActionResult> ImportPromptFromYaml([FromBody] ImportYamlRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var prompt = await _promptService.ImportPromptFromYamlAsync(request);
            return CreatedAtAction(nameof(GetPromptById), new { id = prompt.Id }, prompt);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid YAML import request");
            return BadRequest(ex.Message);
        }        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing prompt from YAML");
            return StatusCode(500, "An error occurred while importing the prompt");
        }
    }    /// <summary>
    /// Get AI-powered suggestions for categories and tags based on prompt content
    /// </summary>
    [HttpPost("suggest")]
    public async Task<ActionResult> GetPromptSuggestions([FromBody] PromptSuggestionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get all available categories and tags
            var categories = await _context.Categories.ToListAsync();
            var tags = await _context.Tags.ToListAsync();

            // Use AI-powered suggestions
            var suggestions = await GenerateAISuggestions(request.Name, request.Content, categories, tags);
            
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating prompt suggestions");
            return StatusCode(500, "An error occurred while generating suggestions");
        }
    }

    private PromptSuggestionResponse GenerateSimpleSuggestions(string name, string content, List<Models.Category> categories, List<Models.Tag> tags)
    {
        var fullText = $"{name} {content}".ToLower();
        var response = new PromptSuggestionResponse();

        // Simple category matching
        var bestCategory = categories
            .Select(c => new { Category = c, Score = CalculateMatchScore(fullText, c.Name, c.Description) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .FirstOrDefault();

        if (bestCategory != null)
        {
            response.SuggestedCategoryId = bestCategory.Category.Id;
        }

        // Simple tag matching
        var suggestedTags = tags
            .Select(t => new { Tag = t, Score = CalculateMatchScore(fullText, t.Name, t.Description) })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(3)
            .Select(x => x.Tag.Id)
            .ToList();

        response.SuggestedTagIds = suggestedTags;
        response.Reasoning = "Generated using keyword-based analysis";        return response;
    }

    private async Task<PromptSuggestionResponse> GenerateAISuggestions(string name, string content, List<Models.Category> categories, List<Models.Tag> tags)
    {
        try
        {
            // Build the analysis prompt for the AI
            var categoriesJson = string.Join("\n", categories.Select(c => 
                $"- ID: {c.Id}, Name: \"{c.Name}\", Description: \"{c.Description}\""));
            
            var tagsJson = string.Join("\n", tags.Select(t => 
                $"- ID: {t.Id}, Name: \"{t.Name}\", Description: \"{t.Description}\""));

            var analysisPrompt = $@"You are an expert at analyzing prompts and categorizing them. Please analyze the following prompt and suggest the most appropriate category and tags.

PROMPT TO ANALYZE:
Name: {name}
Content: {content}

AVAILABLE CATEGORIES:
{categoriesJson}

AVAILABLE TAGS:
{tagsJson}

Please respond in the following JSON format:
{{
  ""suggestedCategoryId"": ""category-id-or-null"",
  ""suggestedTagIds"": [""tag-id-1"", ""tag-id-2"", ""tag-id-3""],
  ""reasoning"": ""Brief explanation of why these suggestions were made""
}}

Rules:
1. Choose only ONE category that best fits the prompt's purpose
2. Choose up to 3 tags that are most relevant
3. If no good match exists, use null for category or empty array for tags
4. Focus on the prompt's intent, domain, and technique
5. Return valid JSON only";

            // Use Semantic Kernel to get AI suggestions
            var promptTemplate = new Models.PromptTemplate
            {
                Id = "suggestion-analysis",
                Name = "Prompt Analysis for Suggestions",
                Content = analysisPrompt,
                Description = "Analyzes prompts to suggest categories and tags",
                YamlTemplate = "",
                Variables = new List<Models.PromptVariable>(),
                Tags = new List<string>(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var aiResponse = await _semanticKernelService.ExecutePromptAsync(promptTemplate, new Dictionary<string, object>());
            
            // Try to parse the AI response as JSON
            var response = ParseAIResponse(aiResponse, categories, tags);
            response.Reasoning += " (AI-powered analysis)";
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI suggestion failed, falling back to simple analysis");
            // Fallback to simple suggestions if AI fails
            var fallbackResponse = GenerateSimpleSuggestions(name, content, categories, tags);
            fallbackResponse.Reasoning += " (Fallback: AI unavailable)";
            return fallbackResponse;
        }
    }

    private PromptSuggestionResponse ParseAIResponse(string aiResponse, List<Models.Category> categories, List<Models.Tag> tags)
    {
        try
        {
            // Try to extract JSON from the AI response (it might contain extra text)
            var jsonStart = aiResponse.IndexOf('{');
            var jsonEnd = aiResponse.LastIndexOf('}');
            
            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var jsonContent = aiResponse.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var aiSuggestion = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonContent);
                
                var response = new PromptSuggestionResponse();                // Parse category suggestion
                if (aiSuggestion?.TryGetValue("suggestedCategoryId", out var categoryObj) == true && categoryObj != null)
                {
                    var categoryId = categoryObj.ToString();
                    if (!string.IsNullOrEmpty(categoryId) && categoryId != "null" && 
                        categories.Any(c => c.Id == categoryId))
                    {
                        response.SuggestedCategoryId = categoryId;
                    }
                }
                  // Parse tag suggestions
                if (aiSuggestion?.TryGetValue("suggestedTagIds", out var tagsObj) == true && tagsObj != null)
                {
                    if (tagsObj is System.Text.Json.JsonElement tagsElement && tagsElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        var validTagIds = new List<string>();
                        foreach (var tagElement in tagsElement.EnumerateArray())
                        {
                            var tagId = tagElement.GetString();
                            if (!string.IsNullOrEmpty(tagId) && tags.Any(t => t.Id == tagId))
                            {
                                validTagIds.Add(tagId);
                            }
                        }
                        response.SuggestedTagIds = validTagIds.Take(3).ToList();
                    }
                }
                  // Parse reasoning
                if (aiSuggestion?.TryGetValue("reasoning", out var reasoningObj) == true && reasoningObj != null)
                {
                    response.Reasoning = reasoningObj.ToString() ?? "AI analysis completed";
                }
                else
                {
                    response.Reasoning = "AI analysis completed";
                }
                
                return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse AI response: {Response}", aiResponse);
        }
        
        // If parsing fails, return empty response
        return new PromptSuggestionResponse
        {
            Reasoning = "AI response could not be parsed"
        };
    }

    private static int CalculateMatchScore(string text, string name, string description)
    {
        var score = 0;
        
        // Direct name match
        if (text.Contains(name.ToLower()))
            score += 10;
        
        // Description word matches
        if (!string.IsNullOrEmpty(description))
        {
            var words = description.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            score += words.Count(word => word.Length > 3 && text.Contains(word)) * 3;
        }        
        return score;
    }
}
