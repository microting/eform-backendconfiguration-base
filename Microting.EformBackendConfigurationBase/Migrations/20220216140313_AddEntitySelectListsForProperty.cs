using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddEntitySelectListsForProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntitySelectListAreas",
                table: "PropertieVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntitySelectListDeviceUsers",
                table: "PropertieVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntitySelectListAreas",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntitySelectListDeviceUsers",
                table: "Properties",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntitySelectListAreas",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "EntitySelectListDeviceUsers",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "EntitySelectListAreas",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "EntitySelectListDeviceUsers",
                table: "Properties");
        }
    }
}
