using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceAndRepairingFirms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRecords_Assets_AssetId",
                table: "MaintenanceRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_MaintenanceRecords_RepairingFirms_RepairingFirmId",
                table: "MaintenanceRecords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MaintenanceRecords",
                table: "MaintenanceRecords");

            migrationBuilder.RenameTable(
                name: "MaintenanceRecords",
                newName: "Maintenances");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceRecords_RepairingFirmId",
                table: "Maintenances",
                newName: "IX_Maintenances_RepairingFirmId");

            migrationBuilder.RenameIndex(
                name: "IX_MaintenanceRecords_AssetId",
                table: "Maintenances",
                newName: "IX_Maintenances_AssetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Maintenances",
                table: "Maintenances",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenances_Assets_AssetId",
                table: "Maintenances",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenances_RepairingFirms_RepairingFirmId",
                table: "Maintenances",
                column: "RepairingFirmId",
                principalTable: "RepairingFirms",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Maintenances_Assets_AssetId",
                table: "Maintenances");

            migrationBuilder.DropForeignKey(
                name: "FK_Maintenances_RepairingFirms_RepairingFirmId",
                table: "Maintenances");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Maintenances",
                table: "Maintenances");

            migrationBuilder.RenameTable(
                name: "Maintenances",
                newName: "MaintenanceRecords");

            migrationBuilder.RenameIndex(
                name: "IX_Maintenances_RepairingFirmId",
                table: "MaintenanceRecords",
                newName: "IX_MaintenanceRecords_RepairingFirmId");

            migrationBuilder.RenameIndex(
                name: "IX_Maintenances_AssetId",
                table: "MaintenanceRecords",
                newName: "IX_MaintenanceRecords_AssetId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MaintenanceRecords",
                table: "MaintenanceRecords",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRecords_Assets_AssetId",
                table: "MaintenanceRecords",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MaintenanceRecords_RepairingFirms_RepairingFirmId",
                table: "MaintenanceRecords",
                column: "RepairingFirmId",
                principalTable: "RepairingFirms",
                principalColumn: "Id");
        }
    }
}
