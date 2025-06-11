using Microsoft.EntityFrameworkCore;
using VoidBitzPromptWorkshop.API.Models;
using System.Text.Json;

namespace VoidBitzPromptWorkshop.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<PromptTemplate> PromptTemplates { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<PromptExecution> PromptExecutions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure PromptTemplate
        modelBuilder.Entity<PromptTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Content).IsRequired();
            
            // Configure JSON serialization for Tags list
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());
            
            // Configure JSON serialization for Variables list
            entity.Property(e => e.Variables)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<PromptVariable>>(v, (JsonSerializerOptions?)null) ?? new List<PromptVariable>());

            // Configure relationship with Category
            entity.HasOne(e => e.Category)
                .WithMany(c => c.PromptTemplates)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure Tag
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure PromptExecution
        modelBuilder.Entity<PromptExecution>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            // Configure JSON serialization for Variables dictionary
            entity.Property(e => e.Variables)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>());

            // Configure relationship with PromptTemplate
            entity.HasOne(e => e.PromptTemplate)
                .WithMany()
                .HasForeignKey(e => e.PromptTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Categories
        var webCategory = new Category
        {
            Id = "cat-web",
            Name = "Web Development",
            Description = "Prompts for web development tasks",
            Color = "#3B82F6",
            CreatedAt = DateTime.UtcNow
        };

        var contentCategory = new Category
        {
            Id = "cat-content",
            Name = "Content Creation",
            Description = "Prompts for content creation and writing",
            Color = "#10B981",
            CreatedAt = DateTime.UtcNow
        };

        modelBuilder.Entity<Category>().HasData(webCategory, contentCategory);

        // Seed Tags
        var codingTag = new Tag
        {
            Id = "tag-coding",
            Name = "Coding",
            Color = "#8B5CF6"
        };

        var writingTag = new Tag
        {
            Id = "tag-writing",
            Name = "Writing",
            Color = "#F59E0B"
        };

        var aiTag = new Tag
        {
            Id = "tag-ai",
            Name = "AI",
            Color = "#EF4444"
        };

        modelBuilder.Entity<Tag>().HasData(codingTag, writingTag, aiTag);        // Seed Sample Prompts
        var samplePrompt1 = new PromptTemplate
        {
            Id = "prompt-1",
            Name = "Code Review Assistant",
            Description = "Reviews code and provides feedback",
            Content = "Please review the following {{language}} code and provide constructive feedback:\n\n{{code}}\n\nFocus on:\n- Code quality\n- Best practices\n- Performance\n- Security",
            YamlTemplate = "",
            CategoryId = webCategory.Id,
            Tags = new List<string> { codingTag.Id, aiTag.Id },
            Variables = new List<PromptVariable>
            {
                new() { Name = "language", Description = "Programming language", Type = "string", Required = true },
                new() { Name = "code", Description = "Code to review", Type = "string", Required = true }
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var samplePrompt2 = new PromptTemplate
        {
            Id = "prompt-2",
            Name = "Blog Post Generator",
            Description = "Generates blog post content",
            Content = "Write a {{word_count}} word blog post about {{topic}}.\n\nTarget audience: {{audience}}\nTone: {{tone}}\n\nInclude:\n- Engaging introduction\n- {{sections}} main sections\n- Conclusion with call-to-action",
            YamlTemplate = "",
            CategoryId = contentCategory.Id,
            Tags = new List<string> { writingTag.Id, aiTag.Id },
            Variables = new List<PromptVariable>
            {
                new() { Name = "topic", Description = "Blog post topic", Type = "string", Required = true },
                new() { Name = "word_count", Description = "Target word count", Type = "number", Required = true, DefaultValue = "800" },
                new() { Name = "audience", Description = "Target audience", Type = "string", Required = true, DefaultValue = "General" },
                new() { Name = "tone", Description = "Writing tone", Type = "string", Required = false, DefaultValue = "Professional" },
                new() { Name = "sections", Description = "Number of main sections", Type = "number", Required = false, DefaultValue = "3" }
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        modelBuilder.Entity<PromptTemplate>().HasData(samplePrompt1, samplePrompt2);
    }
}
