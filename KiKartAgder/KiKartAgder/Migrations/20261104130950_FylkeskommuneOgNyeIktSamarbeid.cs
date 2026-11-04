using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KiKartAgder.Migrations
{
    /// <inheritdoc />
    public partial class FylkeskommuneOgNyeIktSamarbeid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ErFylkeskommune",
                table: "Kommuner",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "IktSamarbeider",
                columns: new[] { "IktSamarbeidId", "Beskrivelse", "ErAktiv", "Navn", "Rekkefolge" },
                values: new object[,]
                {
                    { 4, "Kristiansand kommunes egen IKT-tjeneste", true, "Kristiansand", 4 },
                    { 5, "Felles IT-tjeneste for Lillesand og Birkenes", true, "Lillesand og Birkenes", 5 }
                });

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4201",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4202",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4203",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4204",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4205",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4206",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4207",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4211",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4212",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4213",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4214",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4215",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4216",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4217",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4218",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4219",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4220",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4221",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4222",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4223",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4224",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4225",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4226",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4227",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.UpdateData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4228",
                column: "ErFylkeskommune",
                value: false);

            migrationBuilder.InsertData(
                table: "Kommuner",
                columns: new[] { "KommuneNr", "ErAktiv", "ErFylkeskommune", "Fylke", "Navn" },
                values: new object[] { "4200", true, true, "Agder", "Agder fylkeskommune" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "IktSamarbeider",
                keyColumn: "IktSamarbeidId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "IktSamarbeider",
                keyColumn: "IktSamarbeidId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Kommuner",
                keyColumn: "KommuneNr",
                keyValue: "4200");

            migrationBuilder.DropColumn(
                name: "ErFylkeskommune",
                table: "Kommuner");
        }
    }
}
