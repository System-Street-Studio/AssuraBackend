using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchasingOrderLinkToRequestsAndAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PurchasingOrderId",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchasingOrderId",
                table: "AssetRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_PurchasingOrderId",
                table: "Requests",
                column: "PurchasingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_PurchasingOrderId",
                table: "AssetRequests",
                column: "PurchasingOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetRequests_PurchasingOrders_PurchasingOrderId",
                table: "AssetRequests",
                column: "PurchasingOrderId",
                principalTable: "PurchasingOrders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_PurchasingOrders_PurchasingOrderId",
                table: "Requests",
                column: "PurchasingOrderId",
                principalTable: "PurchasingOrders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetRequests_PurchasingOrders_PurchasingOrderId",
                table: "AssetRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_PurchasingOrders_PurchasingOrderId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_PurchasingOrderId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_AssetRequests_PurchasingOrderId",
                table: "AssetRequests");

            migrationBuilder.DropColumn(
                name: "PurchasingOrderId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "PurchasingOrderId",
                table: "AssetRequests");
        }
    }
}
