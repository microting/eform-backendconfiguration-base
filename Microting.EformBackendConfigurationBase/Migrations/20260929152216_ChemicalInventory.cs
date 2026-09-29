using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    /// <inheritdoc />
    public partial class ChemicalInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChemicalProductProperties");

            migrationBuilder.DropTable(
                name: "ChemicalProductPropertieSites");

            migrationBuilder.DropTable(
                name: "ChemicalProductPropertyVersions");

            migrationBuilder.DropTable(
                name: "ChemicalProductPropertyVersionSites");

            migrationBuilder.DropColumn(
                name: "ChemicalLastUpdatedAt",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "EntitySearchListChemicalRegNos",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "EntitySearchListChemicals",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "EntitySelectListChemicalAreas",
                table: "PropertieVersions");

            migrationBuilder.DropColumn(
                name: "ChemicalLastUpdatedAt",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "EntitySearchListChemicalRegNos",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "EntitySearchListChemicals",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "EntitySelectListChemicalAreas",
                table: "Properties");

            migrationBuilder.CreateTable(
                name: "ChemicalAlertLogVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChemicalAlertLogId = table.Column<int>(type: "int", nullable: false),
                    PlacementId = table.Column<int>(type: "int", nullable: false),
                    Threshold = table.Column<int>(type: "int", nullable: false),
                    DeadlineKind = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
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
                    table.PrimaryKey("PK_ChemicalAlertLogVersions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChemicalLocations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhotoFileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ChemicalLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChemicalLocations_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChemicalLocationVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChemicalLocationId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PhotoFileName = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_ChemicalLocationVersions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChemicalPlacementVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChemicalPlacementId = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    ChemicalId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    PlacementNote = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisteredByUserId = table.Column<int>(type: "int", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RemovedByUserId = table.Column<int>(type: "int", nullable: true),
                    RemovedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RemovalReason = table.Column<int>(type: "int", nullable: true),
                    RemovalNote = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MovedFromPlacementId = table.Column<int>(type: "int", nullable: true),
                    ObservedStatus = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ChemicalPlacementVersions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChemicalPropertySettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    StockEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DigestRecipients = table.Column<string>(type: "longtext", nullable: true)
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
                    table.PrimaryKey("PK_ChemicalPropertySettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChemicalPropertySettings_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChemicalPropertySettingsVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChemicalPropertySettingsId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    StockEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    DigestRecipients = table.Column<string>(type: "longtext", nullable: true)
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
                    table.PrimaryKey("PK_ChemicalPropertySettingsVersions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChemicalStockEntryVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChemicalStockEntryId = table.Column<int>(type: "int", nullable: false),
                    PlacementId = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    ContainerSize = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ContainerCount = table.Column<int>(type: "int", nullable: true),
                    BatchLot = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Note = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ByUserId = table.Column<int>(type: "int", nullable: false),
                    At = table.Column<DateTime>(type: "datetime(6)", nullable: false),
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
                    table.PrimaryKey("PK_ChemicalStockEntryVersions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChemicalWorkerPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    WorkerId = table.Column<int>(type: "int", nullable: false),
                    View = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Register = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Remove = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Stock = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ManageLocations = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Admin = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_ChemicalWorkerPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChemicalWorkerPermissions_Properties_PropertyId",
                        column: x => x.PropertyId,
                        principalTable: "Properties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChemicalWorkerPermissionVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChemicalWorkerPermissionId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    WorkerId = table.Column<int>(type: "int", nullable: false),
                    View = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Register = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Remove = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Stock = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ManageLocations = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Admin = table.Column<bool>(type: "tinyint(1)", nullable: false),
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
                    table.PrimaryKey("PK_ChemicalWorkerPermissionVersions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChemicalPlacements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LocationId = table.Column<int>(type: "int", nullable: false),
                    ChemicalId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true),
                    PlacementNote = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RegisteredByUserId = table.Column<int>(type: "int", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RemovedByUserId = table.Column<int>(type: "int", nullable: true),
                    RemovedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RemovalReason = table.Column<int>(type: "int", nullable: true),
                    RemovalNote = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MovedFromPlacementId = table.Column<int>(type: "int", nullable: true),
                    ObservedStatus = table.Column<int>(type: "int", nullable: true),
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
                    table.PrimaryKey("PK_ChemicalPlacements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChemicalPlacements_ChemicalLocations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "ChemicalLocations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChemicalAlertLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlacementId = table.Column<int>(type: "int", nullable: false),
                    Threshold = table.Column<int>(type: "int", nullable: false),
                    DeadlineKind = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
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
                    table.PrimaryKey("PK_ChemicalAlertLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChemicalAlertLogs_ChemicalPlacements_PlacementId",
                        column: x => x.PlacementId,
                        principalTable: "ChemicalPlacements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChemicalStockEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PlacementId = table.Column<int>(type: "int", nullable: false),
                    Kind = table.Column<int>(type: "int", nullable: false),
                    ContainerSize = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true),
                    Unit = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    ContainerCount = table.Column<int>(type: "int", nullable: true),
                    BatchLot = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Note = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ByUserId = table.Column<int>(type: "int", nullable: false),
                    At = table.Column<DateTime>(type: "datetime(6)", nullable: false),
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
                    table.PrimaryKey("PK_ChemicalStockEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChemicalStockEntries_ChemicalPlacements_PlacementId",
                        column: x => x.PlacementId,
                        principalTable: "ChemicalPlacements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ChemicalAlertLogs_PlacementId_Threshold_DeadlineKind_Channel",
                table: "ChemicalAlertLogs",
                columns: new[] { "PlacementId", "Threshold", "DeadlineKind", "Channel" });

            migrationBuilder.CreateIndex(
                name: "IX_ChemicalLocations_PropertyId_WorkflowState",
                table: "ChemicalLocations",
                columns: new[] { "PropertyId", "WorkflowState" });

            migrationBuilder.CreateIndex(
                name: "IX_ChemicalPlacements_ChemicalId",
                table: "ChemicalPlacements",
                column: "ChemicalId");

            migrationBuilder.CreateIndex(
                name: "IX_ChemicalPlacements_LocationId_RemovedAt",
                table: "ChemicalPlacements",
                columns: new[] { "LocationId", "RemovedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChemicalPropertySettings_PropertyId",
                table: "ChemicalPropertySettings",
                column: "PropertyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChemicalStockEntries_PlacementId",
                table: "ChemicalStockEntries",
                column: "PlacementId");

            migrationBuilder.CreateIndex(
                name: "IX_ChemicalWorkerPermissions_PropertyId_WorkerId",
                table: "ChemicalWorkerPermissions",
                columns: new[] { "PropertyId", "WorkerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChemicalWorkerPermissions_WorkerId",
                table: "ChemicalWorkerPermissions",
                column: "WorkerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChemicalAlertLogs");

            migrationBuilder.DropTable(
                name: "ChemicalAlertLogVersions");

            migrationBuilder.DropTable(
                name: "ChemicalLocationVersions");

            migrationBuilder.DropTable(
                name: "ChemicalPlacementVersions");

            migrationBuilder.DropTable(
                name: "ChemicalPropertySettings");

            migrationBuilder.DropTable(
                name: "ChemicalPropertySettingsVersions");

            migrationBuilder.DropTable(
                name: "ChemicalStockEntries");

            migrationBuilder.DropTable(
                name: "ChemicalStockEntryVersions");

            migrationBuilder.DropTable(
                name: "ChemicalWorkerPermissions");

            migrationBuilder.DropTable(
                name: "ChemicalWorkerPermissionVersions");

            migrationBuilder.DropTable(
                name: "ChemicalPlacements");

            migrationBuilder.DropTable(
                name: "ChemicalLocations");

            migrationBuilder.AddColumn<DateTime>(
                name: "ChemicalLastUpdatedAt",
                table: "PropertieVersions",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntitySearchListChemicalRegNos",
                table: "PropertieVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntitySearchListChemicals",
                table: "PropertieVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntitySelectListChemicalAreas",
                table: "PropertieVersions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ChemicalLastUpdatedAt",
                table: "Properties",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntitySearchListChemicalRegNos",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntitySearchListChemicals",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EntitySelectListChemicalAreas",
                table: "Properties",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChemicalProductProperties",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChemicalId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    LastFolderName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Locations = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    SdkCaseId = table.Column<int>(type: "int", nullable: false),
                    SdkSiteId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    WorkflowState = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChemicalProductProperties", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChemicalProductPropertieSites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChemicalId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    SdkCaseId = table.Column<int>(type: "int", nullable: false),
                    SdkSiteId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    WorkflowState = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChemicalProductPropertieSites", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChemicalProductPropertyVersions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChemicalId = table.Column<int>(type: "int", nullable: false),
                    ChemicalProductPropertyId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    ExpireDate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    LastFolderName = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Locations = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    SdkCaseId = table.Column<int>(type: "int", nullable: false),
                    SdkSiteId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    WorkflowState = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChemicalProductPropertyVersions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ChemicalProductPropertyVersionSites",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ChemicalId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    LanguageId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    PropertyId = table.Column<int>(type: "int", nullable: false),
                    SdkCaseId = table.Column<int>(type: "int", nullable: false),
                    SdkSiteId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    WorkflowState = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChemicalProductPropertyVersionSites", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
