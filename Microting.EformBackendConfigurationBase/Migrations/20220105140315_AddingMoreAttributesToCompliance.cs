using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingMoreAttributesToCompliance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MicrotingSdkCaseId",
                table: "ComplianceVersions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MicrotingSdkeFormId",
                table: "ComplianceVersions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MicrotingSdkCaseId",
                table: "Compliances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MicrotingSdkeFormId",
                table: "Compliances",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MicrotingSdkCaseId",
                table: "ComplianceVersions");

            migrationBuilder.DropColumn(
                name: "MicrotingSdkeFormId",
                table: "ComplianceVersions");

            migrationBuilder.DropColumn(
                name: "MicrotingSdkCaseId",
                table: "Compliances");

            migrationBuilder.DropColumn(
                name: "MicrotingSdkeFormId",
                table: "Compliances");
        }
    }
}
