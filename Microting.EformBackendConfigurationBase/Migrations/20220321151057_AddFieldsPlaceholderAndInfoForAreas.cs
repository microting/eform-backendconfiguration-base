using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddFieldsPlaceholderAndInfoForAreas : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InfoBox",
                table: "AreaTranslationVersions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Placeholder",
                table: "AreaTranslationVersions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "InfoBox",
                table: "AreaTranslations",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Placeholder",
                table: "AreaTranslations",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InfoBox",
                table: "AreaTranslationVersions");

            migrationBuilder.DropColumn(
                name: "Placeholder",
                table: "AreaTranslationVersions");

            migrationBuilder.DropColumn(
                name: "InfoBox",
                table: "AreaTranslations");

            migrationBuilder.DropColumn(
                name: "Placeholder",
                table: "AreaTranslations");
        }
    }
}
