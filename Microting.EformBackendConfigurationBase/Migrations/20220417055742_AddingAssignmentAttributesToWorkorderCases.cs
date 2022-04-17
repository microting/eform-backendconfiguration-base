using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingAssignmentAttributesToWorkorderCases : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastAssignedToName",
                table: "WorkorderCaseVersions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedByName",
                table: "WorkorderCaseVersions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LastAssignedToName",
                table: "WorkorderCases",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedByName",
                table: "WorkorderCases",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastAssignedToName",
                table: "WorkorderCaseVersions");

            migrationBuilder.DropColumn(
                name: "LastUpdatedByName",
                table: "WorkorderCaseVersions");

            migrationBuilder.DropColumn(
                name: "LastAssignedToName",
                table: "WorkorderCases");

            migrationBuilder.DropColumn(
                name: "LastUpdatedByName",
                table: "WorkorderCases");
        }
    }
}
