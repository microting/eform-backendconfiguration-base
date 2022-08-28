using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingSecondaryeFormIdToAreaRule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SecondaryeFormId",
                table: "AreaRuleVersions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryeFormName",
                table: "AreaRuleVersions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "SecondaryeFormId",
                table: "AreaRules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryeFormName",
                table: "AreaRules",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecondaryeFormId",
                table: "AreaRuleVersions");

            migrationBuilder.DropColumn(
                name: "SecondaryeFormName",
                table: "AreaRuleVersions");

            migrationBuilder.DropColumn(
                name: "SecondaryeFormId",
                table: "AreaRules");

            migrationBuilder.DropColumn(
                name: "SecondaryeFormName",
                table: "AreaRules");
        }
    }
}
