using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransferEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnDate",
                table: "Transfers",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Transfers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TargetUserId",
                table: "Transfers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_TargetUserId",
                table: "Transfers",
                column: "TargetUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Users_TargetUserId",
                table: "Transfers",
                column: "TargetUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Users_TargetUserId",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_TargetUserId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "ReturnDate",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "TargetUserId",
                table: "Transfers");
        }
    }
}
