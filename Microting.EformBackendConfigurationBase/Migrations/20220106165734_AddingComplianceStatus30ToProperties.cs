using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingComplianceStatus30ToProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComplianceStatusThirty",
                table: "PropertieVersions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ComplianceStatusThirty",
                table: "Properties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AreaRuleId",
                table: "AreaRulesPlanningVersions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComplianceStatusThirty",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "ComplianceStatusThirty",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "AreaRuleId",
                table: "AreaRulesPlanningVersions");

            migrationBuilder.AddColumn<int>(
                name: "PermissionId",
                table: "ProperyAreaFolders",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProperyAreaFolders_PluginPermissions_PermissionId",
                table: "ProperyAreaFolders",
                column: "PermissionId",
                principalTable: "PluginPermissions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
