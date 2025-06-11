using Microsoft.AspNetCore.Mvc;
using VoidBitzPromptWorkshop.API.Services;
using VoidBitzPromptWorkshop.API.DTOs;

namespace VoidBitzPromptWorkshop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromptsController : ControllerBase
{
    private readonly IPromptService _promptService;
    private readonly ILogger<PromptsController> _logger;

    public PromptsController(IPromptService promptService, ILogger<PromptsController> logger)
    {
        _promptService = promptService;
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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing prompt from YAML");
            return StatusCode(500, "An error occurred while importing the prompt");
        }
    }
}
