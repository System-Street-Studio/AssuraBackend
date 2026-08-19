using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetIdToAssetInforming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssetId",
                table: "AssetInformings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetInformings_AssetId",
                table: "AssetInformings",
                column: "AssetId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetInformings_Assets_AssetId",
                table: "AssetInformings",
                column: "AssetId",
                principalTable: "Assets",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetInformings_Assets_AssetId",
                table: "AssetInformings");

            migrationBuilder.DropIndex(
                name: "IX_AssetInformings_AssetId",
                table: "AssetInformings");

            migrationBuilder.DropColumn(
                name: "AssetId",
                table: "AssetInformings");
        }
    }
}
