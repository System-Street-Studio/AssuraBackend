using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelAndFixDrift : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetAttachment_AssetRequests_AssetRequestId",
                table: "AssetAttachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AssetAttachment",
                table: "AssetAttachment");

            migrationBuilder.RenameTable(
                name: "AssetAttachment",
                newName: "AssetAttachments");

            migrationBuilder.RenameIndex(
                name: "IX_AssetAttachment_AssetRequestId",
                table: "AssetAttachments",
                newName: "IX_AssetAttachments_AssetRequestId");

            migrationBuilder.AddColumn<string>(
                name: "AssetCategory",
                table: "Requests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AssetName",
                table: "Requests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "DivisionId",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "Requests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcessedByName",
                table: "Requests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ProcessorRemarks",
                table: "Requests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Requests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Requests",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedDate",
                table: "Requests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SoldPrice",
                table: "AccPendingItems",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuyerId",
                table: "AccDiscardedItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "SoldPrice",
                table: "AccDiscardedItems",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestId",
                table: "AssetAttachments",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssetAttachments",
                table: "AssetAttachments",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_DivisionId",
                table: "Requests",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetAttachments_RequestId",
                table: "AssetAttachments",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetAttachments_AssetRequests_AssetRequestId",
                table: "AssetAttachments",
                column: "AssetRequestId",
                principalTable: "AssetRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetAttachments_Requests_RequestId",
                table: "AssetAttachments",
                column: "RequestId",
                principalTable: "Requests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Divisions_DivisionId",
                table: "Requests",
                column: "DivisionId",
                principalTable: "Divisions",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetAttachments_AssetRequests_AssetRequestId",
                table: "AssetAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_AssetAttachments_Requests_RequestId",
                table: "AssetAttachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Divisions_DivisionId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_DivisionId",
                table: "Requests");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AssetAttachments",
                table: "AssetAttachments");

            migrationBuilder.DropIndex(
                name: "IX_AssetAttachments_RequestId",
                table: "AssetAttachments");

            migrationBuilder.DropColumn(
                name: "AssetCategory",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "AssetName",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "DivisionId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ProcessedByName",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ProcessorRemarks",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "SubmittedDate",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "SoldPrice",
                table: "AccPendingItems");

            migrationBuilder.DropColumn(
                name: "BuyerId",
                table: "AccDiscardedItems");

            migrationBuilder.DropColumn(
                name: "SoldPrice",
                table: "AccDiscardedItems");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "AssetAttachments");

            migrationBuilder.RenameTable(
                name: "AssetAttachments",
                newName: "AssetAttachment");

            migrationBuilder.RenameIndex(
                name: "IX_AssetAttachments_AssetRequestId",
                table: "AssetAttachment",
                newName: "IX_AssetAttachment_AssetRequestId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AssetAttachment",
                table: "AssetAttachment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetAttachment_AssetRequests_AssetRequestId",
                table: "AssetAttachment",
                column: "AssetRequestId",
                principalTable: "AssetRequests",
                principalColumn: "Id");
        }
    }
}
