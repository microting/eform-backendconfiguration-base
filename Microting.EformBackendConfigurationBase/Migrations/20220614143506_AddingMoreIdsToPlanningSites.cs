using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingMoreIdsToPlanningSites : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "PlanningSitesVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AreaRuleId",
                table: "PlanningSitesVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AreaId",
                table: "PlanningSites",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AreaRuleId",
                table: "PlanningSites",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "PlanningSitesVersions");

            migrationBuilder.DropColumn(
                name: "AreaRuleId",
                table: "PlanningSitesVersions");

            migrationBuilder.DropColumn(
                name: "AreaId",
                table: "PlanningSites");

            migrationBuilder.DropColumn(
                name: "AreaRuleId",
                table: "PlanningSites");
        }
    }
}
