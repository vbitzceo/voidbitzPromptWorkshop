using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VoidBitzPromptWorkshop.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDescriptionToTags : Migration
    {        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Tags",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            // Update existing tags with descriptions
            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: "tag-coding",
                column: "Description",
                value: "Programming and software development related prompts");

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: "tag-writing",
                column: "Description",
                value: "Content creation, copywriting, and editorial prompts");

            migrationBuilder.UpdateData(
                table: "Tags",
                keyColumn: "Id",
                keyValue: "tag-ai",
                column: "Description",
                value: "AI-powered prompts and machine learning assistance");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Tags");
        }
    }
}
