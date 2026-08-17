using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestedByToQueueAndAccPendingItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestedById",
                table: "QueueItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RequestedByName",
                table: "QueueItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RequestedById",
                table: "AccPendingItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RequestedByName",
                table: "AccPendingItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RequestedByName",
                table: "AccDiscardedItems",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestedById",
                table: "QueueItems");

            migrationBuilder.DropColumn(
                name: "RequestedByName",
                table: "QueueItems");

            migrationBuilder.DropColumn(
                name: "RequestedById",
                table: "AccPendingItems");

            migrationBuilder.DropColumn(
                name: "RequestedByName",
                table: "AccPendingItems");

            migrationBuilder.DropColumn(
                name: "RequestedByName",
                table: "AccDiscardedItems");
        }
    }
}
