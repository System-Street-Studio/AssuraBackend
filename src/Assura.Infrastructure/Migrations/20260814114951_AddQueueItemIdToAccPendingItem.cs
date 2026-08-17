using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQueueItemIdToAccPendingItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: This migration originally also scaffolded a `DropTable "Specifications"` here.
            // That table exists in the live database but was already absent from the current EF
            // model (pre-existing drift, unrelated to this change) — dropping it is out of scope
            // for this fix and was removed to avoid a destructive, unrelated schema change.
            migrationBuilder.AddColumn<int>(
                name: "QueueItemId",
                table: "AccPendingItems",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QueueItemId",
                table: "AccPendingItems");
        }
    }
}
