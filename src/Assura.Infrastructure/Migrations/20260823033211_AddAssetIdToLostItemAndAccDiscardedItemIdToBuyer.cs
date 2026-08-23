using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetIdToLostItemAndAccDiscardedItemIdToBuyer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresOnboarding",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "AssetId",
                table: "LostItems",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccDiscardedItemId",
                table: "Buyers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiresOnboarding",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "LostItems");

            migrationBuilder.DropColumn(
                name: "AccDiscardedItemId",
                table: "Buyers");
        }
    }
}
