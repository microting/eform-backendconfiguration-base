using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingIndustryCodeIsFarmToProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IndustryCode",
                table: "PropertieVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFarm",
                table: "PropertieVersions",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "IndustryCode",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFarm",
                table: "Properties",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IndustryCode",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "IsFarm",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "IndustryCode",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "IsFarm",
                table: "Properties");
        }
    }
}
