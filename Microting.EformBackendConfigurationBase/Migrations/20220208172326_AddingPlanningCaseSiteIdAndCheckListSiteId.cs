using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingPlanningCaseSiteIdAndCheckListSiteId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CheckListSiteId",
                table: "ComplianceVersions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlanningCaseSiteId",
                table: "ComplianceVersions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CheckListSiteId",
                table: "Compliances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlanningCaseSiteId",
                table: "Compliances",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckListSiteId",
                table: "ComplianceVersions");

            migrationBuilder.DropColumn(
                name: "PlanningCaseSiteId",
                table: "ComplianceVersions");

            migrationBuilder.DropColumn(
                name: "CheckListSiteId",
                table: "Compliances");

            migrationBuilder.DropColumn(
                name: "PlanningCaseSiteId",
                table: "Compliances");
        }
    }
}
