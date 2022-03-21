using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingEntityItemIdToPropertyWorker : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntityItemId",
                table: "PropertyWorkerVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntityItemId",
                table: "PropertyWorkers",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityItemId",
                table: "PropertyWorkerVersions");

            migrationBuilder.DropColumn(
                name: "EntityItemId",
                table: "PropertyWorkers");
        }
    }
}
