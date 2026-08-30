using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    /// <inheritdoc />
    public partial class DeviceTokenIdentityModel : Migration
    {
        private const string BackfillDeviceTokens =
            "UPDATE `DeviceTokens` SET " +
            "`AppId` = COALESCE(`AppId`, 'adhoc'), " +
            "`InstallationId` = COALESCE(`InstallationId`, CONCAT('legacy:', `Id`)) " +
            "WHERE `AppId` IS NULL OR `InstallationId` IS NULL;";

        // Version rows key off DeviceTokenId (the FK to the row they
        // snapshot), not their own Id, so a snapshot carries the same
        // synthetic InstallationId as its parent.
        private const string BackfillDeviceTokenVersions =
            "UPDATE `DeviceTokenVersions` SET " +
            "`AppId` = COALESCE(`AppId`, 'adhoc'), " +
            "`InstallationId` = COALESCE(`InstallationId`, CONCAT('legacy:', `DeviceTokenId`)) " +
            "WHERE `AppId` IS NULL OR `InstallationId` IS NULL;";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step roadmap: drop indexes -> rename -> add nullable -> backfill
            // -> tighten -> re-index -> verify.
            //
            // Hand-ordered. EF's generated ordering adds AppId/InstallationId
            // as NOT NULL and creates the unique index before anything has a
            // value, which cannot work on a table that already has rows.
            //
            // Every step is written so that re-running Up from the top is a
            // no-op wherever the step is already done. That is load-bearing,
            // not hygiene: MariaDB auto-commits each DDL statement, so this
            // migration is NOT atomic and __EFMigrationsHistory is only
            // written after the final statement. A failure partway leaves the
            // database renamed and backfilled with no history row, and the
            // next pod start re-runs Up from the top. Unguarded, that re-run
            // dies on the first DROP INDEX (ERROR 1091) and wedges the tenant
            // permanently - across the ~250 tenants that migrate at pod
            // startup, one bad re-run is a fleet outage.
            //
            // The guards use MariaDB's own IF [NOT] EXISTS DDL clauses rather
            // than information_schema probes driven by PREPARE, because the
            // @-prefixed user variables that pattern needs are rejected by
            // MySqlConnector unless the connection string sets
            // AllowUserVariables=True, which this plugin's does not. Every
            // deployment of this plugin runs MariaDB.
            //
            // One failure mode does NOT self-heal, and re-running cannot fix
            // it: a genuine duplicate (AppId, InstallationId) fails the CREATE
            // UNIQUE INDEX with ERROR 1062 identically on every pass, so that
            // tenant crash-loops until someone de-duplicates DeviceTokens by
            // hand. It cannot originate in the backfill below - 'legacy:<PK>'
            // is unique by construction - only in rows a client wrote during
            // the deploy window.
            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS `IX_DeviceTokens_WorkerId_FcmToken` ON `DeviceTokens`;");

            migrationBuilder.Sql(
                "DROP INDEX IF EXISTS `IX_DeviceTokens_WorkerId` ON `DeviceTokens`;");

            // CHANGE COLUMN IF EXISTS, not RENAME COLUMN: the guard is on the
            // OLD name still being present, so a re-run after a completed
            // rename does nothing.
            //
            // `int NOT NULL` restates the column's existing definition -
            // CHANGE COLUMN requires the full specification - it is not a
            // retype.
            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokens` " +
                "CHANGE COLUMN IF EXISTS `WorkerId` `SdkSiteId` int NOT NULL;");

            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokenVersions` " +
                "CHANGE COLUMN IF EXISTS `WorkerId` `SdkSiteId` int NOT NULL;");

            // Nullable first so existing rows survive the add.
            //
            // AppId is added WITH a default. A rolling deploy keeps old pods
            // serving while the new pod migrates, so an old pod registering a
            // device token between the backfill and the tightening below would
            // otherwise insert AppId = NULL, which then fails the tightening
            // (strict mode, ERROR 1138) or collides on the unique index
            // (non-strict, NULL -> '', ERROR 1062). The tightening drops the
            // default again.
            //
            // That default protects the FIRST pass only. ADD COLUMN IF NOT
            // EXISTS matches on column NAME alone, so on any retry after a
            // failure at or beyond the AppId tightening this add is a no-op
            // and does NOT restore DEFAULT 'adhoc' - AppId is by then NOT NULL
            // with no default, and an old pod's INSERT fails with ERROR 1364
            // for the whole retry window. Loud and client-retryable rather
            // than silently wrong, which is the acceptable trade here.
            //
            // InstallationId cannot be given a safe default - every constant
            // value collides with itself under the new unique index - so the
            // default only narrows the race. Re-runnability is what actually
            // makes it survivable: a row left with a NULL InstallationId fails
            // the tightening, and the re-run sweeps it before trying again.
            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokens` ADD COLUMN IF NOT EXISTS " +
                "`AppId` varchar(32) CHARACTER SET utf8mb4 NULL DEFAULT 'adhoc';");

            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokens` ADD COLUMN IF NOT EXISTS " +
                "`InstallationId` varchar(128) CHARACTER SET utf8mb4 NULL;");

            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokenVersions` ADD COLUMN IF NOT EXISTS " +
                "`AppId` longtext CHARACTER SET utf8mb4 NULL;");

            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokenVersions` ADD COLUMN IF NOT EXISTS " +
                "`InstallationId` longtext CHARACTER SET utf8mb4 NULL;");

            // Backfill. When this was written every row came from
            // flutter-adhoc: flutter-eform has a PushBootstrap but never wired
            // it into the app, so it has never registered a token.
            //
            // 'adhoc' must match the AppId the flutter-adhoc client sends at
            // register time. A mismatch does not fail anything loudly - it
            // orphans these rows for the send path until each client
            // re-registers under the name it does send.
            //
            // InstallationId is derived from the PRIMARY KEY, not from a hash
            // of FcmToken. This is deliberate and load-bearing. The outgoing
            // unique key was (WorkerId, FcmToken), which permits two different
            // workers to hold the SAME FcmToken - and such rows exist in
            // production by design: it is the shared-device duplication this
            // change exists to fix. Hashing FcmToken would give both rows an
            // identical InstallationId, so CREATE UNIQUE INDEX below would
            // abort the migration partway through a tenant database at pod
            // startup. Id is unique by construction, so no collision is
            // possible. It is also NULL-safe: FcmToken is a nullable column,
            // and SHA2(NULL) is NULL, which would then fail the NOT NULL
            // tightening.
            //
            // InstallationId is opaque, so a synthetic value is harmless: no
            // client ever sends 'legacy:<id>'. On its next register a client
            // sends its real UUID, a correct row is created, and the legacy
            // row is pruned when its token next returns UNREGISTERED.
            //
            // The IS NULL guard makes the backfill re-enterable after a
            // partially applied migration: it only touches rows that still
            // need a value. It is per column, not per row, because the DEFAULT
            // on AppId means a row can arrive with AppId already set and
            // InstallationId still NULL.
            migrationBuilder.Sql(BackfillDeviceTokens);
            migrationBuilder.Sql(BackfillDeviceTokenVersions);

            // Now safe to tighten. NOT NULL matters beyond hygiene: a MariaDB
            // unique index treats every NULL as distinct, so a nullable
            // (AppId, InstallationId) would not constrain anything.
            //
            // These two stay plain EF AlterColumn calls, deliberately
            // unguarded. EF emits them as `MODIFY COLUMN`, which restates the
            // column's whole definition, so applying it to an
            // already-tightened column is a no-op: MODIFY is self-idempotent.
            // Every statement above needed an explicit IF [NOT] EXISTS guard
            // precisely because DROP INDEX, ADD COLUMN and CHANGE COLUMN are
            // not.
            migrationBuilder.AlterColumn<string>(
                name: "AppId",
                table: "DeviceTokens",
                type: "varchar(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(32)",
                oldMaxLength: 32,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "InstallationId",
                table: "DeviceTokens",
                type: "varchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(128)",
                oldMaxLength: 128,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // Defence in depth, not the mechanism: the MODIFY above already
            // dropped AppId's default by restating the column without a
            // DEFAULT clause. This says it outright, so the intent survives a
            // provider that one day emits a definition which preserves it. The
            // default existed only to keep the migration window safe; the
            // model has no default, and leaving one would silently stamp
            // 'adhoc' on any future insert that forgot to set AppId.
            migrationBuilder.Sql(
                "ALTER TABLE `DeviceTokens` ALTER COLUMN IF EXISTS `AppId` DROP DEFAULT;");

            // Second sweep - the version table ONLY.
            //
            // The matching sweep of DeviceTokens would be dead code here: both
            // its columns are NOT NULL by this point, so
            // `WHERE AppId IS NULL OR InstallationId IS NULL` matches zero
            // rows, always. Nor could any re-ordering rescue a row an old pod
            // inserted while the tightenings ran - such a row carries a NULL
            // InstallationId and dies on the `MODIFY ... NOT NULL` above
            // (ERROR 1138 strict, 1265 non-strict), before control ever
            // reaches this line. That race is not closed by ordering and
            // cannot be: it is closed by the migration failing loudly there,
            // and by the guarded re-run (see the header) sweeping the
            // offending row on its next pass.
            //
            // DeviceTokenVersions is the different case: its
            // AppId/InstallationId stay nullable longtext, nothing ever
            // tightens them, so a version snapshot written in that same window
            // really can still be sitting here with NULLs. Hence this one
            // statement, and only this one.
            migrationBuilder.Sql(BackfillDeviceTokenVersions);

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX IF NOT EXISTS `IX_DeviceTokens_AppId_InstallationId` " +
                "ON `DeviceTokens` (`AppId`, `InstallationId`);");

            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS `IX_DeviceTokens_AppId_SdkSiteId_WorkflowState` " +
                "ON `DeviceTokens` (`AppId`, `SdkSiteId`, `WorkflowState`);");

            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS `IX_DeviceTokens_FcmToken` " +
                "ON `DeviceTokens` (`FcmToken`);");

            // Verify the identity constraint actually landed, because CREATE
            // INDEX IF NOT EXISTS matches on index NAME alone. A tenant that
            // somehow already carries an index called
            // IX_DeviceTokens_AppId_InstallationId which is non-unique, or on
            // different columns, gets Note 1061 and the CREATE above is
            // skipped - the entire point of this migration would then be
            // silently absent. No migration in this history creates that name,
            // so the probability is low; the failure mode is silent, which is
            // what makes it worth a statement.
            //
            // Counting information_schema.STATISTICS rows restricted to the
            // two expected column names with NON_UNIQUE = 0 catches all three
            // ways it can be wrong: not unique, too few columns, wrong
            // columns. Same BEGIN NOT ATOMIC form as the probe in Down.
            migrationBuilder.Sql(@"
BEGIN NOT ATOMIC
    IF (SELECT COUNT(*) FROM information_schema.STATISTICS
        WHERE `TABLE_SCHEMA` = DATABASE()
          AND `TABLE_NAME` = 'DeviceTokens'
          AND `INDEX_NAME` = 'IX_DeviceTokens_AppId_InstallationId'
          AND `NON_UNIQUE` = 0
          AND `COLUMN_NAME` IN ('AppId', 'InstallationId')) <> 2 THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT =
            'IX_DeviceTokens_AppId_InstallationId is not the expected 2-column UNIQUE index.';
    END IF;
END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Lossy: the pre-migration table had no notion of an app, so
            // rolling back merges the two apps' tokens into one
            // undifferentiated set. It also drops AppId and InstallationId
            // from DeviceTokenVersions, so the audit trail loses the identity
            // columns of every snapshot ever taken. Unavoidable on a rollback
            // path.
            //
            // Unlike Up, Down is deliberately NOT re-entrant - no IF EXISTS
            // guards, plain EF calls throughout. Rollbacks are attended and
            // one-shot; Up is the one that runs unattended at pod startup
            // across ~250 tenants. The absence of guards below is a decision,
            // not an oversight.
            //
            // Probe before destroying. Re-creating the unique
            // (WorkerId, FcmToken) index can fail on rows written under the
            // new model - two installs that legitimately share an owner and a
            // token - and that CREATE INDEX is the LAST statement here. Since
            // MariaDB auto-commits each DDL statement, failing there would
            // leave AppId/InstallationId already dropped and unrecoverable.
            // So the check runs first, while the data is still intact, and
            // aborts the rollback without having touched anything.
            //
            // The probe is TOCTOU: a still-serving pod can insert a colliding
            // pair between this check and the CreateIndex at the bottom, and
            // the rollback then fails in exactly the way the probe exists to
            // prevent. Stop writers before rolling back.
            //
            // Rows with a NULL FcmToken are excluded: a unique index treats
            // every NULL as distinct, so they cannot collide, while GROUP BY
            // treats them as equal and would report a false conflict.
            migrationBuilder.Sql(@"
BEGIN NOT ATOMIC
    IF EXISTS (SELECT 1 FROM `DeviceTokens`
               WHERE `FcmToken` IS NOT NULL
               GROUP BY `SdkSiteId`, `FcmToken`
               HAVING COUNT(*) > 1) THEN
        SIGNAL SQLSTATE '45000' SET MESSAGE_TEXT =
            'Rollback aborted: rows share (SdkSiteId, FcmToken). De-duplicate DeviceTokens first. Nothing was dropped.';
    END IF;
END");

            migrationBuilder.DropIndex(
                name: "IX_DeviceTokens_FcmToken",
                table: "DeviceTokens");

            migrationBuilder.DropIndex(
                name: "IX_DeviceTokens_AppId_SdkSiteId_WorkflowState",
                table: "DeviceTokens");

            migrationBuilder.DropIndex(
                name: "IX_DeviceTokens_AppId_InstallationId",
                table: "DeviceTokens");

            migrationBuilder.DropColumn(
                name: "InstallationId",
                table: "DeviceTokens");

            migrationBuilder.DropColumn(
                name: "AppId",
                table: "DeviceTokens");

            migrationBuilder.DropColumn(
                name: "InstallationId",
                table: "DeviceTokenVersions");

            migrationBuilder.DropColumn(
                name: "AppId",
                table: "DeviceTokenVersions");

            migrationBuilder.RenameColumn(
                name: "SdkSiteId",
                table: "DeviceTokens",
                newName: "WorkerId");

            migrationBuilder.RenameColumn(
                name: "SdkSiteId",
                table: "DeviceTokenVersions",
                newName: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_WorkerId",
                table: "DeviceTokens",
                column: "WorkerId");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_WorkerId_FcmToken",
                table: "DeviceTokens",
                columns: new[] { "WorkerId", "FcmToken" },
                unique: true);
        }
    }
}
