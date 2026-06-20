using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnterpriseMaintenanceWorkflowFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Maintenances",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedByUserId",
                table: "Maintenances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "Maintenances",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EscalatedToProcurementAt",
                table: "Maintenances",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IssueType",
                table: "Maintenances",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Maintenances",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "OriginalRequestId",
                table: "Maintenances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "Maintenances",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ReplacementAssetId",
                table: "Maintenances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestedByUserId",
                table: "Maintenances",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "Maintenances",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StorekeeperUserId",
                table: "Maintenances",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Maintenances_ApprovedByUserId",
                table: "Maintenances",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Maintenances_OriginalRequestId",
                table: "Maintenances",
                column: "OriginalRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Maintenances_ReplacementAssetId",
                table: "Maintenances",
                column: "ReplacementAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_Maintenances_RequestedByUserId",
                table: "Maintenances",
                column: "RequestedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Maintenances_StorekeeperUserId",
                table: "Maintenances",
                column: "StorekeeperUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenances_Assets_ReplacementAssetId",
                table: "Maintenances",
                column: "ReplacementAssetId",
                principalTable: "Assets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenances_Requests_OriginalRequestId",
                table: "Maintenances",
                column: "OriginalRequestId",
                principalTable: "Requests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenances_Users_ApprovedByUserId",
                table: "Maintenances",
                column: "ApprovedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenances_Users_RequestedByUserId",
                table: "Maintenances",
                column: "RequestedByUserId",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenances_Users_StorekeeperUserId",
                table: "Maintenances",
                column: "StorekeeperUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Maintenances_Assets_ReplacementAssetId",
                table: "Maintenances");

            migrationBuilder.DropForeignKey(
                name: "FK_Maintenances_Requests_OriginalRequestId",
                table: "Maintenances");

            migrationBuilder.DropForeignKey(
                name: "FK_Maintenances_Users_ApprovedByUserId",
                table: "Maintenances");

            migrationBuilder.DropForeignKey(
                name: "FK_Maintenances_Users_RequestedByUserId",
                table: "Maintenances");

            migrationBuilder.DropForeignKey(
                name: "FK_Maintenances_Users_StorekeeperUserId",
                table: "Maintenances");

            migrationBuilder.DropIndex(
                name: "IX_Maintenances_ApprovedByUserId",
                table: "Maintenances");

            migrationBuilder.DropIndex(
                name: "IX_Maintenances_OriginalRequestId",
                table: "Maintenances");

            migrationBuilder.DropIndex(
                name: "IX_Maintenances_ReplacementAssetId",
                table: "Maintenances");

            migrationBuilder.DropIndex(
                name: "IX_Maintenances_RequestedByUserId",
                table: "Maintenances");

            migrationBuilder.DropIndex(
                name: "IX_Maintenances_StorekeeperUserId",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "EscalatedToProcurementAt",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "IssueType",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "OriginalRequestId",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "ReplacementAssetId",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "RequestedByUserId",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "StorekeeperUserId",
                table: "Maintenances");
        }
    }
}
