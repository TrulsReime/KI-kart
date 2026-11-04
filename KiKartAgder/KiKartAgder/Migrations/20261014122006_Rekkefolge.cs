using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KiKartAgder.Migrations
{
    /// <inheritdoc />
    public partial class Rekkefolge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rekkefolge",
                table: "ProsjektStikkord",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Rekkefolge",
                table: "ProsjektDeltakere",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Rekkefolge",
                table: "ProsjektBehov",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rekkefolge",
                table: "ProsjektStikkord");

            migrationBuilder.DropColumn(
                name: "Rekkefolge",
                table: "ProsjektDeltakere");

            migrationBuilder.DropColumn(
                name: "Rekkefolge",
                table: "ProsjektBehov");
        }
    }
}
