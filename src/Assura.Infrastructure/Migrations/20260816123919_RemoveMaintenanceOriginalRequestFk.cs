using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMaintenanceOriginalRequestFk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Maintenances_Requests_OriginalRequestId",
                table: "Maintenances");

            migrationBuilder.DropIndex(
                name: "IX_Maintenances_OriginalRequestId",
                table: "Maintenances");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Maintenances_OriginalRequestId",
                table: "Maintenances",
                column: "OriginalRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Maintenances_Requests_OriginalRequestId",
                table: "Maintenances",
                column: "OriginalRequestId",
                principalTable: "Requests",
                principalColumn: "Id");
        }
    }
}
