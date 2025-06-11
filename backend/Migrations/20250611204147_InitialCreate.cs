using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VoidBitzPromptWorkshop.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Color = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PromptTemplates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    YamlTemplate = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<string>(type: "TEXT", nullable: true),
                    Tags = table.Column<string>(type: "TEXT", nullable: false),
                    Variables = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromptTemplates_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PromptExecutions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    PromptTemplateId = table.Column<string>(type: "TEXT", nullable: false),
                    Variables = table.Column<string>(type: "TEXT", nullable: false),
                    Result = table.Column<string>(type: "TEXT", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptExecutions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PromptExecutions_PromptTemplates_PromptTemplateId",
                        column: x => x.PromptTemplateId,
                        principalTable: "PromptTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Color", "CreatedAt", "Description", "Name" },
                values: new object[,]
                {
                    { "cat-content", "#10B981", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Prompts for content creation and writing", "Content Creation" },
                    { "cat-web", "#3B82F6", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Prompts for web development tasks", "Web Development" }
                });

            migrationBuilder.InsertData(
                table: "Tags",
                columns: new[] { "Id", "Color", "Description", "Name" },
                values: new object[,]
                {
                    { "tag-chain-of-thought", "#F59E0B", "Encourages the model to break down complex reasoning into intermediate steps for better structured output", "Chain of Thought" },
                    { "tag-few-shot", "#10B981", "Prompts with one or more examples to help the model understand the desired input-output pairs", "Few-Shot" },
                    { "tag-zero-shot", "#3B82F6", "Direct prompts without examples or additional context - ideal for idea generation, summarization, and translation", "Zero-Shot" },
                    { "tag-zero-shot-cot", "#8B5CF6", "Combines chain of thought with zero-shot prompting for better reasoning without examples", "Zero-Shot CoT" }
                });

            migrationBuilder.InsertData(
                table: "PromptTemplates",
                columns: new[] { "Id", "CategoryId", "Content", "CreatedAt", "Description", "Name", "Tags", "UpdatedAt", "Variables", "YamlTemplate" },
                values: new object[,]
                {
                    { "prompt-1", "cat-web", "Please review the following {{language}} code and provide constructive feedback:\n\n{{code}}\n\nFocus on:\n- Code quality\n- Best practices\n- Performance\n- Security", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Reviews code and provides feedback", "Code Review Assistant", "[\"tag-zero-shot\",\"tag-chain-of-thought\"]", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[{\"Name\":\"language\",\"Description\":\"Programming language\",\"Type\":\"string\",\"Required\":true,\"DefaultValue\":null},{\"Name\":\"code\",\"Description\":\"Code to review\",\"Type\":\"string\",\"Required\":true,\"DefaultValue\":null}]", "" },
                    { "prompt-2", "cat-content", "Write a {{word_count}} word blog post about {{topic}}.\n\nTarget audience: {{audience}}\nTone: {{tone}}\n\nInclude:\n- Engaging introduction\n- {{sections}} main sections\n- Conclusion with call-to-action", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Generates blog post content", "Blog Post Generator", "[\"tag-few-shot\",\"tag-zero-shot\"]", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "[{\"Name\":\"topic\",\"Description\":\"Blog post topic\",\"Type\":\"string\",\"Required\":true,\"DefaultValue\":null},{\"Name\":\"word_count\",\"Description\":\"Target word count\",\"Type\":\"number\",\"Required\":true,\"DefaultValue\":\"800\"},{\"Name\":\"audience\",\"Description\":\"Target audience\",\"Type\":\"string\",\"Required\":true,\"DefaultValue\":\"General\"},{\"Name\":\"tone\",\"Description\":\"Writing tone\",\"Type\":\"string\",\"Required\":false,\"DefaultValue\":\"Professional\"},{\"Name\":\"sections\",\"Description\":\"Number of main sections\",\"Type\":\"number\",\"Required\":false,\"DefaultValue\":\"3\"}]", "" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PromptExecutions_PromptTemplateId",
                table: "PromptExecutions",
                column: "PromptTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_PromptTemplates_CategoryId",
                table: "PromptTemplates",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromptExecutions");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "PromptTemplates");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
