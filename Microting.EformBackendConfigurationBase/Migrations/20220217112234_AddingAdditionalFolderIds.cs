using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingAdditionalFolderIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FolderIdForCompletedTasks",
                table: "PropertieVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FolderIdForNewTasks",
                table: "PropertieVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FolderIdForOngoingTasks",
                table: "PropertieVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FolderIdForCompletedTasks",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FolderIdForNewTasks",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FolderIdForOngoingTasks",
                table: "Properties",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FolderIdForCompletedTasks",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "FolderIdForNewTasks",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "FolderIdForOngoingTasks",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "FolderIdForCompletedTasks",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "FolderIdForNewTasks",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "FolderIdForOngoingTasks",
                table: "Properties");
        }
    }
}
