using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingLanguageIdAndSdkSiteIdToChemicalProductProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LanguageId",
                table: "ChemicalProductPropertyVersionSites",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LanguageId",
                table: "ChemicalProductPropertyVersions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SdkSiteId",
                table: "ChemicalProductPropertyVersions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LanguageId",
                table: "ChemicalProductPropertieSites",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LanguageId",
                table: "ChemicalProductProperties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SdkSiteId",
                table: "ChemicalProductProperties",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LanguageId",
                table: "ChemicalProductPropertyVersionSites");

            migrationBuilder.DropColumn(
                name: "LanguageId",
                table: "ChemicalProductPropertyVersions");

            migrationBuilder.DropColumn(
                name: "SdkSiteId",
                table: "ChemicalProductPropertyVersions");

            migrationBuilder.DropColumn(
                name: "LanguageId",
                table: "ChemicalProductPropertieSites");

            migrationBuilder.DropColumn(
                name: "LanguageId",
                table: "ChemicalProductProperties");

            migrationBuilder.DropColumn(
                name: "SdkSiteId",
                table: "ChemicalProductProperties");
        }
    }
}
