using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDivisionIdToPurchasingOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DivisionId",
                table: "PurchasingOrders",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PurchasingOrders_DivisionId",
                table: "PurchasingOrders",
                column: "DivisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchasingOrders_Divisions_DivisionId",
                table: "PurchasingOrders",
                column: "DivisionId",
                principalTable: "Divisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchasingOrders_Divisions_DivisionId",
                table: "PurchasingOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchasingOrders_DivisionId",
                table: "PurchasingOrders");

            migrationBuilder.DropColumn(
                name: "DivisionId",
                table: "PurchasingOrders");
        }
    }
}
