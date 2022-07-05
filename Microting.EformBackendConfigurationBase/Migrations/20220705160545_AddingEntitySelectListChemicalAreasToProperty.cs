using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingEntitySelectListChemicalAreasToProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntitySelectListChemicalAreas",
                table: "PropertieVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntitySelectListChemicalAreas",
                table: "Properties",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntitySelectListChemicalAreas",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "EntitySelectListChemicalAreas",
                table: "Properties");
        }
    }
}
