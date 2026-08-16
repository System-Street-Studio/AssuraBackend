using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQueueItemIdToDiscardedNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QueueItemId",
                table: "DiscardedNotes",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QueueItemId",
                table: "DiscardedNotes");
        }
    }
}
