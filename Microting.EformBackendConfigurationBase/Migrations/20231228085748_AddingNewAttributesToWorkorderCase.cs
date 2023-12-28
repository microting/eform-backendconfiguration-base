using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    /// <inheritdoc />
    public partial class AddingNewAttributesToWorkorderCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedToSdkSiteId",
                table: "WorkorderCaseVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBySdkSiteId",
                table: "WorkorderCaseVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBySdkSiteId",
                table: "WorkorderCaseVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssignedToSdkSiteId",
                table: "WorkorderCases",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBySdkSiteId",
                table: "WorkorderCases",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBySdkSiteId",
                table: "WorkorderCases",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedToSdkSiteId",
                table: "WorkorderCaseVersions");

            migrationBuilder.DropColumn(
                name: "CreatedBySdkSiteId",
                table: "WorkorderCaseVersions");

            migrationBuilder.DropColumn(
                name: "UpdatedBySdkSiteId",
                table: "WorkorderCaseVersions");

            migrationBuilder.DropColumn(
                name: "AssignedToSdkSiteId",
                table: "WorkorderCases");

            migrationBuilder.DropColumn(
                name: "CreatedBySdkSiteId",
                table: "WorkorderCases");

            migrationBuilder.DropColumn(
                name: "UpdatedBySdkSiteId",
                table: "WorkorderCases");
        }
    }
}
