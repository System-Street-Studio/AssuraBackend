using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDivisionRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastVerifiedAt",
                table: "Assets",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastVerifiedByUserId",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "UserDivisionRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DivisionId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    JobTitle = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Notes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AssignedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsDeleted = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDivisionRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDivisionRoles_Divisions_DivisionId",
                        column: x => x.DivisionId,
                        principalTable: "Divisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDivisionRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Assets_LastVerifiedByUserId",
                table: "Assets",
                column: "LastVerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDivisionRoles_DivisionId",
                table: "UserDivisionRoles",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDivisionRoles_UserId",
                table: "UserDivisionRoles",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_Users_LastVerifiedByUserId",
                table: "Assets",
                column: "LastVerifiedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_Users_LastVerifiedByUserId",
                table: "Assets");

            migrationBuilder.DropTable(
                name: "UserDivisionRoles");

            migrationBuilder.DropIndex(
                name: "IX_Assets_LastVerifiedByUserId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "LastVerifiedAt",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "LastVerifiedByUserId",
                table: "Assets");
        }
    }
}
