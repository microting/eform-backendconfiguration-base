using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingFolderIdToPoolRegistrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "PoolHourResultVersions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "PoolHourResults",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "PoolAccidentVersions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlanningId",
                table: "PoolAccidentVersions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SdkCaseId",
                table: "PoolAccidentVersions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "PoolAccidents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlanningId",
                table: "PoolAccidents",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SdkCaseId",
                table: "PoolAccidents",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "PoolHourResultVersions");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "PoolHourResults");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "PoolAccidentVersions");

            migrationBuilder.DropColumn(
                name: "PlanningId",
                table: "PoolAccidentVersions");

            migrationBuilder.DropColumn(
                name: "SdkCaseId",
                table: "PoolAccidentVersions");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "PoolAccidents");

            migrationBuilder.DropColumn(
                name: "PlanningId",
                table: "PoolAccidents");

            migrationBuilder.DropColumn(
                name: "SdkCaseId",
                table: "PoolAccidents");
        }
    }
}
