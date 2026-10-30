using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    /// <inheritdoc />
    public partial class AddLederAssignmentSiteId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "EformUserId",
                table: "LederAssignmentVersions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "SiteId",
                table: "LederAssignmentVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EformUserId",
                table: "LederAssignments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "SiteId",
                table: "LederAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LederAssignments_SiteId",
                table: "LederAssignments",
                column: "SiteId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LederAssignments_SiteId",
                table: "LederAssignments");

            migrationBuilder.DropColumn(
                name: "SiteId",
                table: "LederAssignmentVersions");

            migrationBuilder.DropColumn(
                name: "SiteId",
                table: "LederAssignments");

            migrationBuilder.AlterColumn<int>(
                name: "EformUserId",
                table: "LederAssignmentVersions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EformUserId",
                table: "LederAssignments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
