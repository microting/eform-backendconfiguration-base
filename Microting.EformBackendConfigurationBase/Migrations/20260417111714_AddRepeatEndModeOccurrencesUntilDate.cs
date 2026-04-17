using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    /// <inheritdoc />
    public partial class AddRepeatEndModeOccurrencesUntilDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RepeatEndMode",
                table: "AreaRulesPlanningVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepeatOccurrences",
                table: "AreaRulesPlanningVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RepeatUntilDate",
                table: "AreaRulesPlanningVersions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepeatEndMode",
                table: "AreaRulePlannings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RepeatOccurrences",
                table: "AreaRulePlannings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RepeatUntilDate",
                table: "AreaRulePlannings",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RepeatEndMode",
                table: "AreaRulesPlanningVersions");

            migrationBuilder.DropColumn(
                name: "RepeatOccurrences",
                table: "AreaRulesPlanningVersions");

            migrationBuilder.DropColumn(
                name: "RepeatUntilDate",
                table: "AreaRulesPlanningVersions");

            migrationBuilder.DropColumn(
                name: "RepeatEndMode",
                table: "AreaRulePlannings");

            migrationBuilder.DropColumn(
                name: "RepeatOccurrences",
                table: "AreaRulePlannings");

            migrationBuilder.DropColumn(
                name: "RepeatUntilDate",
                table: "AreaRulePlannings");
        }
    }
}
