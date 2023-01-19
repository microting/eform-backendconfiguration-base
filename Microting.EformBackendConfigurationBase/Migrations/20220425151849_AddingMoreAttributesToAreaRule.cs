using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingMoreAttributesToAreaRule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ComplianceEnabled",
                table: "AreaRuleVersions",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ComplianceModifiable",
                table: "AreaRuleVersions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Notifications",
                table: "AreaRuleVersions",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotificationsModifiable",
                table: "AreaRuleVersions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ComplianceEnabled",
                table: "AreaRules",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ComplianceModifiable",
                table: "AreaRules",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Notifications",
                table: "AreaRules",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NotificationsModifiable",
                table: "AreaRules",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComplianceEnabled",
                table: "AreaRuleVersions");

            migrationBuilder.DropColumn(
                name: "ComplianceModifiable",
                table: "AreaRuleVersions");

            migrationBuilder.DropColumn(
                name: "Notifications",
                table: "AreaRuleVersions");

            migrationBuilder.DropColumn(
                name: "NotificationsModifiable",
                table: "AreaRuleVersions");

            migrationBuilder.DropColumn(
                name: "ComplianceEnabled",
                table: "AreaRules");

            migrationBuilder.DropColumn(
                name: "ComplianceModifiable",
                table: "AreaRules");

            migrationBuilder.DropColumn(
                name: "Notifications",
                table: "AreaRules");

            migrationBuilder.DropColumn(
                name: "NotificationsModifiable",
                table: "AreaRules");
        }
    }
}
