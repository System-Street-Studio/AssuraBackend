using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchasingOrderIdToAssetInforming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PurchasingOrderId",
                table: "AssetInformings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetInformings_PurchasingOrderId",
                table: "AssetInformings",
                column: "PurchasingOrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetInformings_PurchasingOrders_PurchasingOrderId",
                table: "AssetInformings",
                column: "PurchasingOrderId",
                principalTable: "PurchasingOrders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetInformings_PurchasingOrders_PurchasingOrderId",
                table: "AssetInformings");

            migrationBuilder.DropIndex(
                name: "IX_AssetInformings_PurchasingOrderId",
                table: "AssetInformings");

            migrationBuilder.DropColumn(
                name: "PurchasingOrderId",
                table: "AssetInformings");
        }
    }
}
