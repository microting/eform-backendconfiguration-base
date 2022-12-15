using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    /// <inheritdoc />
    public partial class AddingStatusToPlanningSite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PlanningSitesVersions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PlanningSites",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "PlanningSitesVersions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "PlanningSites");
        }
    }
}
