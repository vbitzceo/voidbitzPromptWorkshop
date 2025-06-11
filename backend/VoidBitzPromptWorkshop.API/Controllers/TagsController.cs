using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoidBitzPromptWorkshop.API.Data;
using VoidBitzPromptWorkshop.API.Models;
using VoidBitzPromptWorkshop.API.DTOs;

namespace VoidBitzPromptWorkshop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TagsController> _logger;

    public TagsController(ApplicationDbContext context, ILogger<TagsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all tags
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetAllTags()
    {
        try
        {
            var tags = await _context.Tags
                .OrderBy(t => t.Name)
                .ToListAsync();
            
            return Ok(tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tags");
            return StatusCode(500, "An error occurred while retrieving tags");
        }
    }

    /// <summary>
    /// Get a specific tag by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetTagById(string id)
    {
        try
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                return NotFound($"Tag with ID {id} not found");

            return Ok(tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tag {TagId}", id);
            return StatusCode(500, "An error occurred while retrieving the tag");
        }
    }

    /// <summary>
    /// Create a new tag
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateTag([FromBody] CreateTagRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if tag name already exists
            var existingTag = await _context.Tags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == request.Name.ToLower());
            
            if (existingTag != null)
                return Conflict($"Tag with name '{request.Name}' already exists");

            var tag = new Tag
            {
                Name = request.Name,
                Color = request.Color
            };

            _context.Tags.Add(tag);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTagById), new { id = tag.Id }, tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tag");
            return StatusCode(500, "An error occurred while creating the tag");
        }
    }

    /// <summary>
    /// Update an existing tag
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTag(string id, [FromBody] UpdateTagRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                return NotFound($"Tag with ID {id} not found");

            // Check if new name conflicts with existing tag
            if (!string.IsNullOrEmpty(request.Name) && request.Name != tag.Name)
            {
                var existingTag = await _context.Tags
                    .FirstOrDefaultAsync(t => t.Name.ToLower() == request.Name.ToLower() && t.Id != id);
                
                if (existingTag != null)
                    return Conflict($"Tag with name '{request.Name}' already exists");
                
                tag.Name = request.Name;
            }

            if (request.Color != null)
                tag.Color = request.Color;

            await _context.SaveChangesAsync();

            return Ok(tag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tag {TagId}", id);
            return StatusCode(500, "An error occurred while updating the tag");
        }
    }

    /// <summary>
    /// Delete a tag
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTag(string id)
    {
        try
        {
            var tag = await _context.Tags.FindAsync(id);
            if (tag == null)
                return NotFound($"Tag with ID {id} not found");

            // Note: We allow deleting tags even if they're referenced in prompts
            // The tag IDs in prompts will become orphaned, which is acceptable
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tag {TagId}", id);
            return StatusCode(500, "An error occurred while deleting the tag");
        }
    }
}
