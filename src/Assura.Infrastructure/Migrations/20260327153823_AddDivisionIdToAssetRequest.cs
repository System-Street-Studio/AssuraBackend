using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDivisionIdToAssetRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AssetRequests",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "AssetRequests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DivisionId",
                table: "AssetRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AssetRequests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AssetRequests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "AssetRequests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_DivisionId",
                table: "AssetRequests",
                column: "DivisionId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetRequests_Divisions_DivisionId",
                table: "AssetRequests",
                column: "DivisionId",
                principalTable: "Divisions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetRequests_Divisions_DivisionId",
                table: "AssetRequests");

            migrationBuilder.DropIndex(
                name: "IX_AssetRequests_DivisionId",
                table: "AssetRequests");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AssetRequests");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "AssetRequests");

            migrationBuilder.DropColumn(
                name: "DivisionId",
                table: "AssetRequests");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AssetRequests");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AssetRequests");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "AssetRequests");
        }
    }
}
