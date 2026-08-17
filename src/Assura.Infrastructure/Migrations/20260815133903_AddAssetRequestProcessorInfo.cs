using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetRequestProcessorInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "AssetRequests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessedByName",
                table: "AssetRequests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ProcessedByUserId",
                table: "AssetRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessorRemarks",
                table: "AssetRequests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "AssetRequests");

            migrationBuilder.DropColumn(
                name: "ProcessedByName",
                table: "AssetRequests");

            migrationBuilder.DropColumn(
                name: "ProcessedByUserId",
                table: "AssetRequests");

            migrationBuilder.DropColumn(
                name: "ProcessorRemarks",
                table: "AssetRequests");
        }
    }
}
