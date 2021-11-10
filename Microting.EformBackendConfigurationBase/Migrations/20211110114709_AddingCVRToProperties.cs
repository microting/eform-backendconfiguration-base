using Microsoft.EntityFrameworkCore.Migrations;

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingCVRToProperties : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CVR",
                table: "PropertieVersions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CVR",
                table: "Properties",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CVR",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "CVR",
                table: "Properties");
        }
    }
}
