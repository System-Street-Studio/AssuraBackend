using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestedByToDiscardedNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Written with "IF NOT EXISTS" (MariaDB 10.0.2+) instead of the
            // generated AddColumn/CreateIndex/AddForeignKey calls: an earlier run of
            // this migration partially applied against the live DB before failing
            // (AssetInformings.Remarks already existed there from unrelated prior
            // drift, unrecorded in migration history) and MySQL/MariaDB DDL isn't
            // transactional, so DiscardedNotes' two new columns were already
            // committed. Making every statement idempotent lets this migration be
            // safely (re)applied regardless of how much of it already landed.
            migrationBuilder.Sql(
                "ALTER TABLE `DiscardedNotes` ADD COLUMN IF NOT EXISTS `RequestedByName` longtext CHARACTER SET utf8mb4 NULL;");

            migrationBuilder.Sql(
                "ALTER TABLE `DiscardedNotes` ADD COLUMN IF NOT EXISTS `RequestedByUserId` int NULL;");

            migrationBuilder.Sql(
                "ALTER TABLE `AssetInformings` ADD COLUMN IF NOT EXISTS `Remarks` longtext CHARACTER SET utf8mb4 NULL;");

            migrationBuilder.Sql(
                "ALTER TABLE `AssetInformings` ADD COLUMN IF NOT EXISTS `TargetEmployeeId` int NULL;");

            migrationBuilder.Sql(
                "CREATE INDEX IF NOT EXISTS `IX_AssetInformings_TargetEmployeeId` ON `AssetInformings` (`TargetEmployeeId`);");

            migrationBuilder.Sql(@"
                SET @fk_exists = (
                    SELECT COUNT(*) FROM information_schema.TABLE_CONSTRAINTS
                    WHERE CONSTRAINT_SCHEMA = DATABASE()
                    AND CONSTRAINT_NAME = 'FK_AssetInformings_Users_TargetEmployeeId'
                    AND TABLE_NAME = 'AssetInformings'
                );
                SET @sql = IF(@fk_exists = 0,
                    'ALTER TABLE `AssetInformings` ADD CONSTRAINT `FK_AssetInformings_Users_TargetEmployeeId` FOREIGN KEY (`TargetEmployeeId`) REFERENCES `Users` (`Id`);',
                    'SELECT 1;'
                );
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetInformings_Users_TargetEmployeeId",
                table: "AssetInformings");

            migrationBuilder.DropIndex(
                name: "IX_AssetInformings_TargetEmployeeId",
                table: "AssetInformings");

            migrationBuilder.DropColumn(
                name: "RequestedByName",
                table: "DiscardedNotes");

            migrationBuilder.DropColumn(
                name: "RequestedByUserId",
                table: "DiscardedNotes");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "AssetInformings");

            migrationBuilder.DropColumn(
                name: "TargetEmployeeId",
                table: "AssetInformings");
        }
    }
}
