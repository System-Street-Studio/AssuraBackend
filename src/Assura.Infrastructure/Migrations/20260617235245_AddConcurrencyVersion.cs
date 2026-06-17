using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "UserDivisionRoles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Transfers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "TransferApprovals",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "TINs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Suppliers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Requests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "RepairingFirms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Receipts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "QueueItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "QRNs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "PurchasingOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "PurchasingOrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Products",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Maintenances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "LostItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "GRNs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "GINs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Divisions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "DiscountInfos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "DiscardedNotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "CustomReports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Buyers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "AuditLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Assets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "AssetRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "AssetInformings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "AccPendingItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "AccDiscardNotes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "AccDiscardedItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "UserDivisionRoles");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "TransferApprovals");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "TINs");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "RepairingFirms");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "QueueItems");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "QRNs");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "PurchasingOrders");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "PurchasingOrderItems");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "LostItems");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "GRNs");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "GINs");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Divisions");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "DiscountInfos");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "DiscardedNotes");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "CustomReports");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Buyers");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "AssetRequests");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "AssetInformings");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "AccPendingItems");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "AccDiscardNotes");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "AccDiscardedItems");
        }
    }
}
