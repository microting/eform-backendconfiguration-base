using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    /// <inheritdoc />
    public partial class AddRepeatOrdinalWeekToAreaRulePlanning : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RepeatOrdinalWeek",
                table: "AreaRulesPlanningVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepeatOrdinalWeek",
                table: "AreaRulePlannings",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepeatOrdinalWeek",
                table: "AreaRulesPlanningVersions");

            migrationBuilder.DropColumn(
                name: "RepeatOrdinalWeek",
                table: "AreaRulePlannings");
        }
    }
}
