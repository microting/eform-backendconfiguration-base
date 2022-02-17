using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingAttributesToWorkorderCase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntityItemIdForArea",
                table: "WorkorderCaseVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedAreaName",
                table: "WorkorderCaseVersions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "EntityItemIdForArea",
                table: "WorkorderCases",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedAreaName",
                table: "WorkorderCases",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityItemIdForArea",
                table: "WorkorderCaseVersions");

            migrationBuilder.DropColumn(
                name: "SelectedAreaName",
                table: "WorkorderCaseVersions");

            migrationBuilder.DropColumn(
                name: "EntityItemIdForArea",
                table: "WorkorderCases");

            migrationBuilder.DropColumn(
                name: "SelectedAreaName",
                table: "WorkorderCases");
        }
    }
}
