using Microsoft.EntityFrameworkCore;
using VoidBitzPromptWorkshop.API.Models;
using System.Text.Json;

namespace VoidBitzPromptWorkshop.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }    public DbSet<PromptTemplate> PromptTemplates { get; set; }
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
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>())
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                    (c1, c2) => c1!.SequenceEqual(c2!),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()));
            
            // Configure JSON serialization for Variables list
            entity.Property(e => e.Variables)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<PromptVariable>>(v, (JsonSerializerOptions?)null) ?? new List<PromptVariable>())
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<PromptVariable>>(
                    (c1, c2) => c1!.Count == c2!.Count && c1.Zip(c2).All(p => p.First.Name == p.Second.Name),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Name.GetHashCode())),
                    c => c.ToList()));

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
        });        // Configure Tag
        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure PromptExecution
        modelBuilder.Entity<PromptExecution>(entity =>
        {
            entity.HasKey(e => e.Id);            // Configure JSON serialization for Variables dictionary
            entity.Property(e => e.Variables)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>())
                .Metadata.SetValueComparer(new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<Dictionary<string, object>>(
                    (c1, c2) => c1!.Count == c2!.Count && c1.All(kvp => c2.ContainsKey(kvp.Key) && Equals(kvp.Value, c2[kvp.Key])),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value != null ? v.Value.GetHashCode() : 0)),
                    c => new Dictionary<string, object>(c)));

            // Configure relationship with PromptTemplate
            entity.HasOne(e => e.PromptTemplate)
                .WithMany()
                .HasForeignKey(e => e.PromptTemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed data
        SeedData(modelBuilder);
    }    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Use a fixed date for seeding to avoid migration issues
        var seedDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Seed Categories
        var webCategory = new Category
        {
            Id = "cat-web",
            Name = "Web Development",
            Description = "Prompts for web development tasks",
            Color = "#3B82F6",
            CreatedAt = seedDate
        };

        var contentCategory = new Category
        {
            Id = "cat-content",
            Name = "Content Creation",
            Description = "Prompts for content creation and writing",
            Color = "#10B981",
            CreatedAt = seedDate
        };

        modelBuilder.Entity<Category>().HasData(webCategory, contentCategory);        // Seed Tags - Prompt Engineering Techniques
        var zeroShotTag = new Tag
        {
            Id = "tag-zero-shot",
            Name = "Zero-Shot",
            Description = "Direct prompts without examples or additional context - ideal for idea generation, summarization, and translation",
            Color = "#3B82F6"
        };

        var fewShotTag = new Tag
        {
            Id = "tag-few-shot",
            Name = "Few-Shot",
            Description = "Prompts with one or more examples to help the model understand the desired input-output pairs",
            Color = "#10B981"
        };

        var chainOfThoughtTag = new Tag
        {
            Id = "tag-chain-of-thought",
            Name = "Chain of Thought",
            Description = "Encourages the model to break down complex reasoning into intermediate steps for better structured output",
            Color = "#F59E0B"
        };

        var zeroShotCoTTag = new Tag
        {
            Id = "tag-zero-shot-cot",
            Name = "Zero-Shot CoT",
            Description = "Combines chain of thought with zero-shot prompting for better reasoning without examples",
            Color = "#8B5CF6"
        };

        modelBuilder.Entity<Tag>().HasData(zeroShotTag, fewShotTag, chainOfThoughtTag, zeroShotCoTTag);// Seed Sample Prompts
        var samplePrompt1 = new PromptTemplate
        {
            Id = "prompt-1",
            Name = "Code Review Assistant",
            Description = "Reviews code and provides feedback",
            Content = "Please review the following {{language}} code and provide constructive feedback:\n\n{{code}}\n\nFocus on:\n- Code quality\n- Best practices\n- Performance\n- Security",
            YamlTemplate = "",
            CategoryId = webCategory.Id,
            Tags = new List<string> { zeroShotTag.Id, chainOfThoughtTag.Id },            Variables = new List<PromptVariable>
            {
                new() { Name = "language", Description = "Programming language", Type = "string", Required = true },
                new() { Name = "code", Description = "Code to review", Type = "string", Required = true }
            },
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        };

        var samplePrompt2 = new PromptTemplate
        {
            Id = "prompt-2",
            Name = "Blog Post Generator",
            Description = "Generates blog post content",
            Content = "Write a {{word_count}} word blog post about {{topic}}.\n\nTarget audience: {{audience}}\nTone: {{tone}}\n\nInclude:\n- Engaging introduction\n- {{sections}} main sections\n- Conclusion with call-to-action",
            YamlTemplate = "",
            CategoryId = contentCategory.Id,
            Tags = new List<string> { fewShotTag.Id, zeroShotTag.Id },
            Variables = new List<PromptVariable>
            {
                new() { Name = "topic", Description = "Blog post topic", Type = "string", Required = true },
                new() { Name = "word_count", Description = "Target word count", Type = "number", Required = true, DefaultValue = "800" },
                new() { Name = "audience", Description = "Target audience", Type = "string", Required = true, DefaultValue = "General" },
                new() { Name = "tone", Description = "Writing tone", Type = "string", Required = false, DefaultValue = "Professional" },
                new() { Name = "sections", Description = "Number of main sections", Type = "number", Required = false, DefaultValue = "3" }
            },
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        };

        modelBuilder.Entity<PromptTemplate>().HasData(samplePrompt1, samplePrompt2);
    }
}
