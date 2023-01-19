using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    public partial class AddingMorePoolHourTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PoolAccidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AreaRuleId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Time = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    SolidFaeces = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DiarrheaLoose = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Vomit = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ContactedPersonId = table.Column<int>(type: "int", nullable: false),
                    OwnPersonId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    WorkflowState = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoolAccidents", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PoolAccidentVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PoolAccidentId = table.Column<int>(type: "int", nullable: false),
                    AreaRuleId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Time = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    SolidFaeces = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DiarrheaLoose = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Vomit = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ContactedPersonId = table.Column<int>(type: "int", nullable: false),
                    OwnPersonId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    WorkflowState = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoolAccidentVersions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PoolHourResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PoolHourId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SdkCaseId = table.Column<int>(type: "int", nullable: false),
                    PlanningId = table.Column<int>(type: "int", nullable: false),
                    AreaRuleId = table.Column<int>(type: "int", nullable: false),
                    PulseRateAtOpening = table.Column<double>(type: "double", nullable: false),
                    ReadPhValue = table.Column<double>(type: "double", nullable: false),
                    ReadFreeChlorine = table.Column<double>(type: "double", nullable: false),
                    ReadTemperature = table.Column<double>(type: "double", nullable: false),
                    NumberOfGuestsAtClosing = table.Column<double>(type: "double", nullable: false),
                    Clarity = table.Column<double>(type: "double", nullable: false),
                    MeasuredFreeChlorine = table.Column<double>(type: "double", nullable: false),
                    MeasuredTotalChlorine = table.Column<double>(type: "double", nullable: false),
                    MeasuredBoundChlorine = table.Column<double>(type: "double", nullable: false),
                    MeasuredPh = table.Column<double>(type: "double", nullable: false),
                    AcknowledgmentOfPulseRateAtOpening = table.Column<double>(type: "double", nullable: false),
                    MeasuredTempDuringTheDay = table.Column<double>(type: "double", nullable: false),
                    Comment = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DoneByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    WorkflowState = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoolHourResults", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PoolHourResultVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PoolHourResultId = table.Column<int>(type: "int", nullable: false),
                    PoolHourId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SdkCaseId = table.Column<int>(type: "int", nullable: false),
                    PlanningId = table.Column<int>(type: "int", nullable: false),
                    AreaRuleId = table.Column<int>(type: "int", nullable: false),
                    PulseRateAtOpening = table.Column<double>(type: "double", nullable: false),
                    ReadPhValue = table.Column<double>(type: "double", nullable: false),
                    ReadFreeChlorine = table.Column<double>(type: "double", nullable: false),
                    ReadTemperature = table.Column<double>(type: "double", nullable: false),
                    NumberOfGuestsAtClosing = table.Column<double>(type: "double", nullable: false),
                    Clarity = table.Column<double>(type: "double", nullable: false),
                    MeasuredFreeChlorine = table.Column<double>(type: "double", nullable: false),
                    MeasuredTotalChlorine = table.Column<double>(type: "double", nullable: false),
                    MeasuredBoundChlorine = table.Column<double>(type: "double", nullable: false),
                    MeasuredPh = table.Column<double>(type: "double", nullable: false),
                    AcknowledgmentOfPulseRateAtOpening = table.Column<double>(type: "double", nullable: false),
                    MeasuredTempDuringTheDay = table.Column<double>(type: "double", nullable: false),
                    Comment = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DoneByUserId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    WorkflowState = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoolHourResultVersions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PoolAccidents");

            migrationBuilder.DropTable(
                name: "PoolAccidentVersions");

            migrationBuilder.DropTable(
                name: "PoolHourResults");

            migrationBuilder.DropTable(
                name: "PoolHourResultVersions");
        }
    }
}
