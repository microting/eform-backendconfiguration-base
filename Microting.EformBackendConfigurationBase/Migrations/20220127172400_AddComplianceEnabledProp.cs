using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddComplianceEnabledProp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ComplianceEnabled",
                table: "AreaRulesPlanningVersions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ComplianceEnabled",
                table: "AreaRulePlannings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ComplianceEnabled",
                table: "AreaRuleInitialFields",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ComplianceEnabled",
                table: "AreaInitialFieldVersions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "ComplianceEnabled",
                table: "AreaInitialFields",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComplianceEnabled",
                table: "AreaRulesPlanningVersions");

            migrationBuilder.DropColumn(
                name: "ComplianceEnabled",
                table: "AreaRulePlannings");

            migrationBuilder.DropColumn(
                name: "ComplianceEnabled",
                table: "AreaRuleInitialFields");

            migrationBuilder.DropColumn(
                name: "ComplianceEnabled",
                table: "AreaInitialFieldVersions");

            migrationBuilder.DropColumn(
                name: "ComplianceEnabled",
                table: "AreaInitialFields");
        }
    }
}
