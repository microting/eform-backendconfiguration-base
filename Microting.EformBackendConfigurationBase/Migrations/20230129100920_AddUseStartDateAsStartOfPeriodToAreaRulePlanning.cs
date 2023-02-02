using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    /// <inheritdoc />
    public partial class AddUseStartDateAsStartOfPeriodToAreaRulePlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UseStartDateAsStartOfPeriod",
                table: "AreaRulesPlanningVersions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseStartDateAsStartOfPeriod",
                table: "AreaRulePlannings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UseStartDateAsStartOfPeriod",
                table: "AreaRulesPlanningVersions");

            migrationBuilder.DropColumn(
                name: "UseStartDateAsStartOfPeriod",
                table: "AreaRulePlannings");
        }
    }
}
