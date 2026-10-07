using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AkademieTrutnov.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kurzy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nazev = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Anotace = table.Column<string>(type: "TEXT", nullable: false),
                    RozsahHodin = table.Column<int>(type: "INTEGER", nullable: false),
                    Cena = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: false),
                    Akreditovano = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kurzy", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lektori",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Jmeno = table.Column<string>(type: "TEXT", nullable: false),
                    Prijmeni = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Telefon = table.Column<string>(type: "TEXT", nullable: false),
                    Externista = table.Column<bool>(type: "INTEGER", nullable: false),
                    Specializace = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lektori", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Studenti",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Jmeno = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Prijmeni = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Telefon = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Organizace = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Studenti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ucebny",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Nazev = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Adresa = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    Kapacita = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ucebny", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Terminy",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    KurzId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UcebnaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DatumOd = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DatumDo = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Kapacita = table.Column<int>(type: "INTEGER", nullable: false),
                    Stav = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Terminy", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Terminy_Kurzy_KurzId",
                        column: x => x.KurzId,
                        principalTable: "Kurzy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Terminy_Ucebny_UcebnaId",
                        column: x => x.UcebnaId,
                        principalTable: "Ucebny",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DnyVyuky",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TerminId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LektorId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Datum = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    PocetHodin = table.Column<int>(type: "INTEGER", nullable: false),
                    Tema = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DnyVyuky", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DnyVyuky_Lektori_LektorId",
                        column: x => x.LektorId,
                        principalTable: "Lektori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DnyVyuky_Terminy_TerminId",
                        column: x => x.TerminId,
                        principalTable: "Terminy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Prihlasky",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TerminId = table.Column<Guid>(type: "TEXT", nullable: false),
                    DatumPrihlaseni = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Stav = table.Column<int>(type: "INTEGER", nullable: false),
                    PoradiVPoradniku = table.Column<int>(type: "INTEGER", nullable: true),
                    NabidkaExpirace = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DatumZruseni = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DuvodZruseni = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PoznamkaAdmina = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prihlasky", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Prihlasky_Studenti_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Studenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Prihlasky_Terminy_TerminId",
                        column: x => x.TerminId,
                        principalTable: "Terminy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TerminLektori",
                columns: table => new
                {
                    TerminId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LektorId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PocetHodin = table.Column<int>(type: "INTEGER", nullable: false),
                    Role = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerminLektori", x => new { x.TerminId, x.LektorId });
                    table.ForeignKey(
                        name: "FK_TerminLektori_Lektori_LektorId",
                        column: x => x.LektorId,
                        principalTable: "Lektori",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TerminLektori_Terminy_TerminId",
                        column: x => x.TerminId,
                        principalTable: "Terminy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dochazky",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    DenVyukyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StudentId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Pritomen = table.Column<bool>(type: "INTEGER", nullable: false),
                    Poznamka = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dochazky", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dochazky_DnyVyuky_DenVyukyId",
                        column: x => x.DenVyukyId,
                        principalTable: "DnyVyuky",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dochazky_Studenti_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Studenti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Osvedceni",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PrihlaskaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CisloOsvedceni = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DatumVydani = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Vysledek = table.Column<int>(type: "INTEGER", nullable: false),
                    DosazenaDochazkaProcent = table.Column<double>(type: "REAL", nullable: false),
                    Vystavil = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Osvedceni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Osvedceni_Prihlasky_PrihlaskaId",
                        column: x => x.PrihlaskaId,
                        principalTable: "Prihlasky",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DnyVyuky_LektorId",
                table: "DnyVyuky",
                column: "LektorId");

            migrationBuilder.CreateIndex(
                name: "IX_DnyVyuky_TerminId",
                table: "DnyVyuky",
                column: "TerminId");

            migrationBuilder.CreateIndex(
                name: "IX_Dochazky_DenVyukyId",
                table: "Dochazky",
                column: "DenVyukyId");

            migrationBuilder.CreateIndex(
                name: "IX_Dochazky_StudentId",
                table: "Dochazky",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Osvedceni_CisloOsvedceni",
                table: "Osvedceni",
                column: "CisloOsvedceni",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Osvedceni_PrihlaskaId",
                table: "Osvedceni",
                column: "PrihlaskaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Prihlasky_StudentId",
                table: "Prihlasky",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_Prihlasky_TerminId",
                table: "Prihlasky",
                column: "TerminId");

            migrationBuilder.CreateIndex(
                name: "IX_TerminLektori_LektorId",
                table: "TerminLektori",
                column: "LektorId");

            migrationBuilder.CreateIndex(
                name: "IX_Terminy_KurzId",
                table: "Terminy",
                column: "KurzId");

            migrationBuilder.CreateIndex(
                name: "IX_Terminy_UcebnaId",
                table: "Terminy",
                column: "UcebnaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dochazky");

            migrationBuilder.DropTable(
                name: "Osvedceni");

            migrationBuilder.DropTable(
                name: "TerminLektori");

            migrationBuilder.DropTable(
                name: "DnyVyuky");

            migrationBuilder.DropTable(
                name: "Prihlasky");

            migrationBuilder.DropTable(
                name: "Lektori");

            migrationBuilder.DropTable(
                name: "Studenti");

            migrationBuilder.DropTable(
                name: "Terminy");

            migrationBuilder.DropTable(
                name: "Kurzy");

            migrationBuilder.DropTable(
                name: "Ucebny");
        }
    }
}
