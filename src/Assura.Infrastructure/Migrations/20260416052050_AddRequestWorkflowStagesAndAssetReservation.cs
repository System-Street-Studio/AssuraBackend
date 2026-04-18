using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestWorkflowStagesAndAssetReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DivisionHeadReviewedAt",
                table: "Requests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DivisionHeadReviewerId",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PickupConfirmedAt",
                table: "Requests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiresDivisionHeadApproval",
                table: "Requests",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StorekeeperProcessedAt",
                table: "Requests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StorekeeperProcessorId",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TemporarilyAssignedAt",
                table: "Requests",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReservedByRequestId",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReservedForUserId",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReservedUntilUtc",
                table: "Assets",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DivisionHeadReviewedAt",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "DivisionHeadReviewerId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "PickupConfirmedAt",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "RequiresDivisionHeadApproval",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "StorekeeperProcessedAt",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "StorekeeperProcessorId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "TemporarilyAssignedAt",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ReservedByRequestId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "ReservedForUserId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "ReservedUntilUtc",
                table: "Assets");
        }
    }
}
