using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microting.EformBackendConfigurationBase.Migrations
{
    /// <inheritdoc />
    public partial class DeviceTokenIdentityModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Hand-ordered. EF's generated ordering adds AppId/InstallationId
            // as NOT NULL and creates the unique index before anything has a
            // value, which cannot work on a table that already has rows.

            migrationBuilder.DropIndex(
                name: "IX_DeviceTokens_WorkerId_FcmToken",
                table: "DeviceTokens");

            migrationBuilder.DropIndex(
                name: "IX_DeviceTokens_WorkerId",
                table: "DeviceTokens");

            migrationBuilder.RenameColumn(
                name: "WorkerId",
                table: "DeviceTokens",
                newName: "SdkSiteId");

            migrationBuilder.RenameColumn(
                name: "WorkerId",
                table: "DeviceTokenVersions",
                newName: "SdkSiteId");

            // Nullable first so existing rows survive the add.
            migrationBuilder.AddColumn<string>(
                name: "AppId",
                table: "DeviceTokens",
                type: "varchar(32)",
                maxLength: 32,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "InstallationId",
                table: "DeviceTokens",
                type: "varchar(128)",
                maxLength: 128,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AppId",
                table: "DeviceTokenVersions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "InstallationId",
                table: "DeviceTokenVersions",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // Backfill. Every pre-existing row in this table is flutter-adhoc:
            // flutter-eform has never registered (its PushBootstrap is not
            // wired into the app). VERIFY BY ROW INSPECTION BEFORE PROD.
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
            migrationBuilder.Sql(
                "UPDATE `DeviceTokens` SET `AppId` = 'adhoc', " +
                "`InstallationId` = CONCAT('legacy:', `Id`) " +
                "WHERE `AppId` IS NULL;");

            // Version rows key off DeviceTokenId (the FK to the row they
            // snapshot), not their own Id, so a snapshot carries the same
            // synthetic InstallationId as its parent.
            migrationBuilder.Sql(
                "UPDATE `DeviceTokenVersions` SET `AppId` = 'adhoc', " +
                "`InstallationId` = CONCAT('legacy:', `DeviceTokenId`) " +
                "WHERE `AppId` IS NULL;");

            // Now safe to tighten. NOT NULL matters beyond hygiene: a MariaDB
            // unique index treats every NULL as distinct, so a nullable
            // (AppId, InstallationId) would not constrain anything.
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

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_AppId_InstallationId",
                table: "DeviceTokens",
                columns: new[] { "AppId", "InstallationId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_AppId_SdkSiteId_WorkflowState",
                table: "DeviceTokens",
                columns: new[] { "AppId", "SdkSiteId", "WorkflowState" });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTokens_FcmToken",
                table: "DeviceTokens",
                column: "FcmToken");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Lossy: the pre-migration table had no notion of an app, so
            // rolling back merges the two apps' tokens into one
            // undifferentiated set. Unavoidable on a rollback path.
            //
            // Re-creating the unique (WorkerId, FcmToken) index can also fail
            // if rows written under the new model violate it - two installs
            // that legitimately share an owner and a token. Rare, but a
            // rollback is not guaranteed to succeed unattended.
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
