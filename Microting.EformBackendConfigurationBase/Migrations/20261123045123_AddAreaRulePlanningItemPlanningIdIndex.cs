using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations;

/// <inheritdoc />
public partial class AddAreaRulePlanningItemPlanningIdIndex : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Additive only: one secondary index, nothing else. The SQL is EF's CreateIndex with
        // MariaDB's IF NOT EXISTS, so a re-run after a pod died before EF recorded the migration is a
        // no-op (same reasoning as DeviceTokenIdentityModel). InnoDB builds it online (INPLACE,
        // LOCK=NONE); on Galera it runs as TOI.
        migrationBuilder.Sql(
            "CREATE INDEX IF NOT EXISTS `IX_AreaRulePlannings_ItemPlanningId` ON `AreaRulePlannings` (`ItemPlanningId`);");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "DROP INDEX IF EXISTS `IX_AreaRulePlannings_ItemPlanningId` ON `AreaRulePlannings`;");
    }
}
