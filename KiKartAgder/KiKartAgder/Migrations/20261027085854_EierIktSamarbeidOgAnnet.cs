using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace KiKartAgder.Migrations
{
    /// <inheritdoc />
    public partial class EierIktSamarbeidOgAnnet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProsjekteierId",
                table: "Prosjekter",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SokerAnnet",
                table: "Prosjekter",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TilbyrAnnet",
                table: "Prosjekter",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IktSamarbeider",
                columns: table => new
                {
                    IktSamarbeidId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Navn = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Beskrivelse = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Rekkefolge = table.Column<int>(type: "integer", nullable: false),
                    ErAktiv = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IktSamarbeider", x => x.IktSamarbeidId);
                });

            migrationBuilder.CreateTable(
                name: "ProsjektIktSamarbeid",
                columns: table => new
                {
                    ProsjektId = table.Column<int>(type: "integer", nullable: false),
                    IktSamarbeidId = table.Column<int>(type: "integer", nullable: false),
                    Rekkefolge = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProsjektIktSamarbeid", x => new { x.ProsjektId, x.IktSamarbeidId });
                    table.ForeignKey(
                        name: "FK_ProsjektIktSamarbeid_IktSamarbeider_IktSamarbeidId",
                        column: x => x.IktSamarbeidId,
                        principalTable: "IktSamarbeider",
                        principalColumn: "IktSamarbeidId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProsjektIktSamarbeid_Prosjekter_ProsjektId",
                        column: x => x.ProsjektId,
                        principalTable: "Prosjekter",
                        principalColumn: "ProsjektId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "IktSamarbeider",
                columns: new[] { "IktSamarbeidId", "Beskrivelse", "ErAktiv", "Navn", "Rekkefolge" },
                values: new object[,]
                {
                    { 1, "Interkommunalt IKT-selskap", true, "IKT Agder", 1 },
                    { 2, "Det Digitale Vestre Agder", true, "DDV", 2 },
                    { 3, "Digitaliseringssamarbeidet i Agder", true, "Digi Agder", 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prosjekter_ProsjekteierId",
                table: "Prosjekter",
                column: "ProsjekteierId");

            migrationBuilder.CreateIndex(
                name: "IX_IktSamarbeider_Navn",
                table: "IktSamarbeider",
                column: "Navn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProsjektIktSamarbeid_IktSamarbeidId",
                table: "ProsjektIktSamarbeid",
                column: "IktSamarbeidId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prosjekter_Kontaktpersoner_ProsjekteierId",
                table: "Prosjekter",
                column: "ProsjekteierId",
                principalTable: "Kontaktpersoner",
                principalColumn: "KontaktpersonId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prosjekter_Kontaktpersoner_ProsjekteierId",
                table: "Prosjekter");

            migrationBuilder.DropTable(
                name: "ProsjektIktSamarbeid");

            migrationBuilder.DropTable(
                name: "IktSamarbeider");

            migrationBuilder.DropIndex(
                name: "IX_Prosjekter_ProsjekteierId",
                table: "Prosjekter");

            migrationBuilder.DropColumn(
                name: "ProsjekteierId",
                table: "Prosjekter");

            migrationBuilder.DropColumn(
                name: "SokerAnnet",
                table: "Prosjekter");

            migrationBuilder.DropColumn(
                name: "TilbyrAnnet",
                table: "Prosjekter");
        }
    }
}
