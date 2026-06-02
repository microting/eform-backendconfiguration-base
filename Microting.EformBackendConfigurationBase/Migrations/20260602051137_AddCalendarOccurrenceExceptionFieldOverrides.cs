using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarOccurrenceExceptionFieldOverrides : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BoardId",
                table: "CalendarOccurrenceExceptions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "CalendarOccurrenceExceptions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionHtml",
                table: "CalendarOccurrenceExceptions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "CalendarOccurrenceExceptions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoardId",
                table: "CalendarOccurrenceExceptions");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "CalendarOccurrenceExceptions");

            migrationBuilder.DropColumn(
                name: "DescriptionHtml",
                table: "CalendarOccurrenceExceptions");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "CalendarOccurrenceExceptions");
        }
    }
}
