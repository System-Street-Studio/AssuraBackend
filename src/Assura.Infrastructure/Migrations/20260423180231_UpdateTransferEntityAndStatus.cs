using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransferEntityAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssignedToId",
                table: "Transfers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_AssignedToId",
                table: "Transfers",
                column: "AssignedToId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Users_AssignedToId",
                table: "Transfers",
                column: "AssignedToId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Users_AssignedToId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_AssignedToId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "AssignedToId",
                table: "Transfers");
        }
    }
}
