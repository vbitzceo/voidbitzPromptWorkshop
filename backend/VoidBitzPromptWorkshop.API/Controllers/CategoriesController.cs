using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoidBitzPromptWorkshop.API.Data;
using VoidBitzPromptWorkshop.API.Models;
using VoidBitzPromptWorkshop.API.DTOs;

namespace VoidBitzPromptWorkshop.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ApplicationDbContext context, ILogger<CategoriesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all categories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult> GetAllCategories()
    {
        try
        {
            var categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
            
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }

    /// <summary>
    /// Get a specific category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetCategoryById(string id)
    {
        try
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound($"Category with ID {id} not found");

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
            return StatusCode(500, "An error occurred while retrieving the category");
        }
    }

    /// <summary>
    /// Create a new category
    /// </summary>
    [HttpPost]
    public async Task<ActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Check if category name already exists
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == request.Name.ToLower());
            
            if (existingCategory != null)
                return Conflict($"Category with name '{request.Name}' already exists");

            var category = new Category
            {
                Name = request.Name,
                Description = request.Description,
                Color = request.Color,
                CreatedAt = DateTime.UtcNow
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.Id }, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category");
            return StatusCode(500, "An error occurred while creating the category");
        }
    }

    /// <summary>
    /// Update an existing category
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateCategory(string id, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound($"Category with ID {id} not found");

            // Check if new name conflicts with existing category
            if (!string.IsNullOrEmpty(request.Name) && request.Name != category.Name)
            {
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == request.Name.ToLower() && c.Id != id);
                
                if (existingCategory != null)
                    return Conflict($"Category with name '{request.Name}' already exists");
                
                category.Name = request.Name;
            }

            if (request.Description != null)
                category.Description = request.Description;
            
            if (request.Color != null)
                category.Color = request.Color;

            await _context.SaveChangesAsync();

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            return StatusCode(500, "An error occurred while updating the category");
        }
    }

    /// <summary>
    /// Delete a category
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCategory(string id)
    {
        try
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return NotFound($"Category with ID {id} not found");

            // Check if category has associated prompts
            var hasPrompts = await _context.PromptTemplates
                .AnyAsync(p => p.CategoryId == id);
            
            if (hasPrompts)
            {
                return BadRequest("Cannot delete category that has associated prompt templates. Please reassign or delete the prompts first.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            return StatusCode(500, "An error occurred while deleting the category");
        }
    }
}
