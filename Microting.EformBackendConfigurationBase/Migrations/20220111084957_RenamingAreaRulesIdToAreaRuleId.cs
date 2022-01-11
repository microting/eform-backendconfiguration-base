using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class RenamingAreaRulesIdToAreaRuleId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AreaRulesId",
                table: "AreaRulesPlanningVersions",
                "AreaRuleId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AreaRuleId",
                table: "AreaRulesPlanningVersions",
                "AreaRulesId");
        }
    }
}