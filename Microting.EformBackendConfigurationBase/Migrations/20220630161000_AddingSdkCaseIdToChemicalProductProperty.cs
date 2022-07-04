using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingSdkCaseIdToChemicalProductProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SdkCaseId",
                table: "ChemicalProductPropertyVersions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SdkCaseId",
                table: "ChemicalProductProperties",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SdkCaseId",
                table: "ChemicalProductPropertyVersions");

            migrationBuilder.DropColumn(
                name: "SdkCaseId",
                table: "ChemicalProductProperties");
        }
    }
}
