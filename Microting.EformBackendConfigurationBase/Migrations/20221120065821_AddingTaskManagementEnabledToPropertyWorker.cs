using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingTaskManagementEnabledToPropertyWorker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TaskManagementEnabled",
                table: "PropertyWorkerVersions",
                type: "tinyint(1)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TaskManagementEnabled",
                table: "PropertyWorkers",
                type: "tinyint(1)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaskManagementEnabled",
                table: "PropertyWorkerVersions");

            migrationBuilder.DropColumn(
                name: "TaskManagementEnabled",
                table: "PropertyWorkers");
        }
    }
}
