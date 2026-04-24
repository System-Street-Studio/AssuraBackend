using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransferEntityWithAssetRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Users_AssignedToId",
                table: "Transfers");

            migrationBuilder.RenameColumn(
                name: "AssignedToId",
                table: "Transfers",
                newName: "AssetRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_Transfers_AssignedToId",
                table: "Transfers",
                newName: "IX_Transfers_AssetRequestId");

            migrationBuilder.AddColumn<int>(
                name: "CurrentHolderId",
                table: "Transfers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_CurrentHolderId",
                table: "Transfers",
                column: "CurrentHolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_AssetRequests_AssetRequestId",
                table: "Transfers",
                column: "AssetRequestId",
                principalTable: "AssetRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Users_CurrentHolderId",
                table: "Transfers",
                column: "CurrentHolderId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_AssetRequests_AssetRequestId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Users_CurrentHolderId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_CurrentHolderId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "CurrentHolderId",
                table: "Transfers");

            migrationBuilder.RenameColumn(
                name: "AssetRequestId",
                table: "Transfers",
                newName: "AssignedToId");

            migrationBuilder.RenameIndex(
                name: "IX_Transfers_AssetRequestId",
                table: "Transfers",
                newName: "IX_Transfers_AssignedToId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Users_AssignedToId",
                table: "Transfers",
                column: "AssignedToId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
