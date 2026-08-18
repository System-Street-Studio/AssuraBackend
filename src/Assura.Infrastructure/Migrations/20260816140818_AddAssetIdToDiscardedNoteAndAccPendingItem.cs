using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetIdToDiscardedNoteAndAccPendingItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssetId",
                table: "DiscardedNotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssetId",
                table: "AccPendingItems",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "DiscardedNotes");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "AccPendingItems");
        }
    }
}
