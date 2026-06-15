using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Assura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetPoolEndpoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_AssetRequests_AssetRequestId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Users_TargetUserId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "AssetName",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "AssetTag",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "Specifications_Computer_Display",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Computer_GPU",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Computer_OS",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Computer_RAM",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Computer_Storage",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Furniture_Adjustability",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Furniture_Color",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Furniture_Height",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Furniture_Length",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Furniture_Material",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Furniture_Width",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Networking_DataRate",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Networking_FormFactor",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Networking_MACAddress",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Networking_PortCount",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Printing_Connectivity",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Printing_PrintResolution",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Printing_PrintingTechnology",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Printing_Type",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Server_CPU",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Server_IPAddress",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Server_OS",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Server_RAM",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Specifications_Server_Storage",
                table: "Assets");

            migrationBuilder.AlterColumn<int>(
                name: "TargetUserId",
                table: "Transfers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "AssetRequestId",
                table: "Transfers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Products",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Specifications",
                columns: table => new
                {
                    AssetId = table.Column<int>(type: "int", nullable: false),
                    Computer_Display = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Computer_RAM = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Computer_GPU = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Computer_Storage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Computer_OS = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Server_OS = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Server_RAM = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Server_CPU = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Server_IPAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Server_Storage = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Networking_PortCount = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Networking_DataRate = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Networking_FormFactor = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Networking_MACAddress = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Printing_Type = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Printing_PrintingTechnology = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Printing_Connectivity = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Printing_PrintResolution = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Furniture_Material = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Furniture_Length = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Furniture_Width = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Furniture_Height = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Furniture_Color = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Furniture_Adjustability = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specifications", x => x.AssetId);
                    table.ForeignKey(
                        name: "FK_Specifications_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_AssetRequests_AssetRequestId",
                table: "Transfers",
                column: "AssetRequestId",
                principalTable: "AssetRequests",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Users_TargetUserId",
                table: "Transfers",
                column: "TargetUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_AssetRequests_AssetRequestId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Users_TargetUserId",
                table: "Transfers");

            migrationBuilder.DropTable(
                name: "Specifications");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Products");

            migrationBuilder.AlterColumn<int>(
                name: "TargetUserId",
                table: "Transfers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "AssetRequestId",
                table: "Transfers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssetName",
                table: "Transfers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "AssetTag",
                table: "Transfers",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Computer_Display",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Computer_GPU",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Computer_OS",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Computer_RAM",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Computer_Storage",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Furniture_Adjustability",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Furniture_Color",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Furniture_Height",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Furniture_Length",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Furniture_Material",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Furniture_Width",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Networking_DataRate",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Networking_FormFactor",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Networking_MACAddress",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Networking_PortCount",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Printing_Connectivity",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Printing_PrintResolution",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Printing_PrintingTechnology",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Printing_Type",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Server_CPU",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Server_IPAddress",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Server_OS",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Server_RAM",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Specifications_Server_Storage",
                table: "Assets",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_AssetRequests_AssetRequestId",
                table: "Transfers",
                column: "AssetRequestId",
                principalTable: "AssetRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Users_TargetUserId",
                table: "Transfers",
                column: "TargetUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
