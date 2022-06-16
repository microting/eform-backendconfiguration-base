using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingEntitySearchListPoolWorkers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntitySearchListPoolWorkers",
                table: "PropertieVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntitySearchListPoolWorkers",
                table: "Properties",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntitySearchListPoolWorkers",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "EntitySearchListPoolWorkers",
                table: "Properties");
        }
    }
}
