using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SchoolFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnneeScolaires",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Libelle = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DateDebut = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateFin = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnneeScolaires", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Familles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NomPere = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PrenomPere = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TelephonePere = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EmailPere = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ProfessionPere = table.Column<string>(type: "text", nullable: true),
                    NomMere = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PrenomMere = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TelephoneMere = table.Column<string>(type: "text", nullable: true),
                    EmailMere = table.Column<string>(type: "text", nullable: true),
                    ProfessionMere = table.Column<string>(type: "text", nullable: true),
                    Adresse = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Ville = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    QuartierCommune = table.Column<string>(type: "text", nullable: true),
                    TelephonePrincipal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TelephoneSecondaire = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Familles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Matieres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Libelle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Coefficient = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matieres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TypeFrais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Libelle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Categorie = table.Column<string>(type: "text", nullable: false),
                    IsRecurrent = table.Column<bool>(type: "boolean", nullable: false),
                    IsObligatoire = table.Column<bool>(type: "boolean", nullable: false),
                    GenerationAutomatique = table.Column<bool>(type: "boolean", nullable: false),
                    MontantsParNiveau = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeFrais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telephone = table.Column<string>(type: "text", nullable: true),
                    Role = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "integer", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nom = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Niveau = table.Column<string>(type: "text", nullable: false),
                    Section = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    CapaciteMax = table.Column<int>(type: "integer", nullable: false),
                    AnneeScolaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Classes_AnneeScolaires_AnneeScolaireId",
                        column: x => x.AnneeScolaireId,
                        principalTable: "AnneeScolaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Periodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Libelle = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateFin = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    AnneeScolaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Periodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Periodes_AnneeScolaires_AnneeScolaireId",
                        column: x => x.AnneeScolaireId,
                        principalTable: "AnneeScolaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UtilisateurId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldValues = table.Column<string>(type: "text", nullable: true),
                    NewValues = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Paiements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroPaiement = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FamilleId = table.Column<Guid>(type: "uuid", nullable: false),
                    MontantTotal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DatePaiement = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModePaiement = table.Column<string>(type: "text", nullable: false),
                    Reference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Commentaire = table.Column<string>(type: "text", nullable: true),
                    EnregistrePar = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paiements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Paiements_Familles_FamilleId",
                        column: x => x.FamilleId,
                        principalTable: "Familles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Paiements_Utilisateurs_EnregistrePar",
                        column: x => x.EnregistrePar,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Eleves",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Matricule = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateNaissance = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    LieuNaissance = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Sexe = table.Column<string>(type: "text", nullable: false),
                    PhotoPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FamilleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClasseId = table.Column<Guid>(type: "uuid", nullable: true),
                    AnneeScolaireId = table.Column<Guid>(type: "uuid", nullable: true),
                    Nationalite = table.Column<string>(type: "text", nullable: true),
                    GroupeSanguin = table.Column<string>(type: "text", nullable: true),
                    Allergies = table.Column<string>(type: "text", nullable: true),
                    ContactUrgence = table.Column<string>(type: "text", nullable: true),
                    Remarques = table.Column<string>(type: "text", nullable: true),
                    Statut = table.Column<string>(type: "text", nullable: false),
                    DateInscription = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Eleves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Eleves_AnneeScolaires_AnneeScolaireId",
                        column: x => x.AnneeScolaireId,
                        principalTable: "AnneeScolaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Eleves_Classes_ClasseId",
                        column: x => x.ClasseId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Eleves_Familles_FamilleId",
                        column: x => x.FamilleId,
                        principalTable: "Familles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Frais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EleveId = table.Column<Guid>(type: "uuid", nullable: false),
                    TypeFraisId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Montant = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantPaye = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DateEcheance = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Remarques = table.Column<string>(type: "text", nullable: true),
                    Commentaire = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Frais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Frais_Eleves_EleveId",
                        column: x => x.EleveId,
                        principalTable: "Eleves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Frais_Periodes_PeriodeId",
                        column: x => x.PeriodeId,
                        principalTable: "Periodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Frais_TypeFrais_TypeFraisId",
                        column: x => x.TypeFraisId,
                        principalTable: "TypeFrais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EleveId = table.Column<Guid>(type: "uuid", nullable: true),
                    MatiereId = table.Column<Guid>(type: "uuid", nullable: true),
                    PeriodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Valeur = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    NoteSur = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Commentaire = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notes_Eleves_EleveId",
                        column: x => x.EleveId,
                        principalTable: "Eleves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notes_Matieres_MatiereId",
                        column: x => x.MatiereId,
                        principalTable: "Matieres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notes_Periodes_PeriodeId",
                        column: x => x.PeriodeId,
                        principalTable: "Periodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VentilationsPaiement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaiementId = table.Column<Guid>(type: "uuid", nullable: false),
                    EleveId = table.Column<Guid>(type: "uuid", nullable: false),
                    FraisId = table.Column<Guid>(type: "uuid", nullable: false),
                    MontantVentile = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarque = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentilationsPaiement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentilationsPaiement_Eleves_EleveId",
                        column: x => x.EleveId,
                        principalTable: "Eleves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentilationsPaiement_Frais_FraisId",
                        column: x => x.FraisId,
                        principalTable: "Frais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentilationsPaiement_Paiements_PaiementId",
                        column: x => x.PaiementId,
                        principalTable: "Paiements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AnneeScolaires",
                columns: new[] { "Id", "ArchiveReason", "ArchivedAt", "ArchivedBy", "CreatedAt", "DateDebut", "DateFin", "IsActive", "IsArchived", "Libelle", "UpdatedAt" },
                values: new object[] { new Guid("fcc3f2ef-a2e1-453f-9282-0f702508570e"), null, null, null, new DateTime(2026, 3, 5, 8, 51, 2, 208, DateTimeKind.Utc).AddTicks(2682), new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 6, 30, 23, 59, 59, 0, DateTimeKind.Utc), true, false, "2024-2025", null });

            migrationBuilder.InsertData(
                table: "TypeFrais",
                columns: new[] { "Id", "ArchiveReason", "ArchivedAt", "ArchivedBy", "Categorie", "Code", "CreatedAt", "Description", "GenerationAutomatique", "IsArchived", "IsObligatoire", "IsRecurrent", "Libelle", "MontantsParNiveau", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("4a67847b-41f7-4dcd-aa35-32330bab9c07"), null, null, null, "Inscription", "INSC", new DateTime(2026, 3, 5, 8, 51, 3, 547, DateTimeKind.Utc).AddTicks(1884), null, true, false, true, false, "Frais d'inscription", "{\"CP\":5000,\"CE1\":5000,\"CE2\":5000,\"CM1\":5000,\"CM2\":5000,\"Sixieme\":5000,\"Cinquieme\":5000,\"Quatrieme\":5000,\"Troisieme\":5000,\"Seconde\":5000,\"Premiere\":5000,\"Terminale\":5000}", null },
                    { new Guid("93637d31-7703-4a9a-8074-a4e4b0d9611c"), null, null, null, "Scolarite", "SCOL", new DateTime(2026, 3, 5, 8, 51, 3, 547, DateTimeKind.Utc).AddTicks(1894), "Frais de scolarité trimestriel", true, false, true, true, "Scolarité", "{\"CP\":60000,\"CE1\":60000,\"CE2\":60000,\"CM1\":70000,\"CM2\":70000,\"Sixieme\":80000,\"Cinquieme\":80000,\"Quatrieme\":85000,\"Troisieme\":85000,\"Seconde\":90000,\"Premiere\":90000,\"Terminale\":90000}", null },
                    { new Guid("f65de0a1-1d13-4fd2-9c09-8617a46476aa"), null, null, null, "Cantine", "CANT", new DateTime(2026, 3, 5, 8, 51, 3, 547, DateTimeKind.Utc).AddTicks(1909), "Frais de cantine mensuel", false, false, false, true, "Cantine", "{\"CP\":15000,\"CE1\":15000,\"CE2\":15000,\"CM1\":15000,\"CM2\":15000,\"Sixieme\":15000,\"Cinquieme\":15000,\"Quatrieme\":15000,\"Troisieme\":15000,\"Seconde\":15000,\"Premiere\":15000,\"Terminale\":15000}", null }
                });

            migrationBuilder.InsertData(
                table: "Utilisateurs",
                columns: new[] { "Id", "ArchiveReason", "ArchivedAt", "ArchivedBy", "CreatedAt", "Email", "FailedLoginAttempts", "IsActive", "IsArchived", "LastLoginAt", "LockedUntil", "Nom", "PasswordHash", "Prenom", "Role", "Telephone", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { new Guid("251a13fe-e29b-441b-a086-8375ca210cc7"), null, null, null, new DateTime(2026, 3, 5, 8, 51, 3, 244, DateTimeKind.Utc).AddTicks(1028), "comptable@schoolflow.com", 0, true, false, null, null, "Martin", "$2a$11$2EmB5T8DQOOKhh7WTb369O6Lmo6Gm9mdrJJp.hhm43eUJlmnst9Wa", "Sophie", "Comptable", null, null, "comptable" },
                    { new Guid("5bf789b9-d9b9-4811-9cec-f362d84aff41"), null, null, null, new DateTime(2026, 3, 5, 8, 51, 2, 930, DateTimeKind.Utc).AddTicks(4150), "directeur@schoolflow.com", 0, true, false, null, null, "Durand", "$2a$11$gXAMsjt/Dmi2lTvl.wOrMOyU7QYBidnQ6aeTmhNN/2OTNoQuQY4/y", "Marie", "Directeur", null, null, "directeur" },
                    { new Guid("70a0836d-bde0-4ef1-936a-3918f91b21ed"), null, null, null, new DateTime(2026, 3, 5, 8, 51, 2, 587, DateTimeKind.Utc).AddTicks(7537), "admin@schoolflow.com", 0, true, false, null, null, "Administrateur", "$2a$11$YGqNP90LbqbZgI5pJ0Y8zeN0k7YbmrkC5Fk1TWUnEeWO8YbXK6f/2", "Système", "Admin", null, null, "admin" },
                    { new Guid("f6edb4a1-b142-47c7-bf3c-df195e44085f"), null, null, null, new DateTime(2026, 3, 5, 8, 51, 3, 546, DateTimeKind.Utc).AddTicks(9442), "secretaire@schoolflow.com", 0, true, false, null, null, "Leroy", "$2a$11$XQjqhc/V9fqMCXC06xYhGu48y8jP5SR5zM47EN606n9pN9nQRRTiS", "Paul", "Secretaire", null, null, "secretaire" }
                });

            migrationBuilder.InsertData(
                table: "Classes",
                columns: new[] { "Id", "AnneeScolaireId", "ArchiveReason", "ArchivedAt", "ArchivedBy", "CapaciteMax", "Code", "CreatedAt", "IsArchived", "Niveau", "Nom", "Section", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("00177369-1ae8-45f9-aa4e-822def27955d"), new Guid("fcc3f2ef-a2e1-453f-9282-0f702508570e"), null, null, null, 60, "CE1", new DateTime(2026, 3, 5, 8, 51, 3, 547, DateTimeKind.Utc).AddTicks(261), false, "CE1", "CE1", null, null },
                    { new Guid("3e87fc8f-ef6e-4dfc-ae46-60ed2ef8ef1d"), new Guid("fcc3f2ef-a2e1-453f-9282-0f702508570e"), null, null, null, 60, "CM2", new DateTime(2026, 3, 5, 8, 51, 3, 547, DateTimeKind.Utc).AddTicks(334), false, "CM2", "CM2", null, null },
                    { new Guid("605ca321-32d0-4aae-9398-9e44d8f272a6"), new Guid("fcc3f2ef-a2e1-453f-9282-0f702508570e"), null, null, null, 60, "CE2", new DateTime(2026, 3, 5, 8, 51, 3, 547, DateTimeKind.Utc).AddTicks(289), false, "CE2", "CE2", null, null },
                    { new Guid("8a53d050-0916-4dfe-a650-ca829b5ca069"), new Guid("fcc3f2ef-a2e1-453f-9282-0f702508570e"), null, null, null, 60, "CINQUIEME", new DateTime(2026, 3, 5, 8, 51, 3, 547, DateTimeKind.Utc).AddTicks(482), false, "Cinquieme", "5ème", null, null },
                    { new Guid("9abbfeb6-8919-4387-a620-2bec5aefc82d"), new Guid("fcc3f2ef-a2e1-453f-9282-0f702508570e"), null, null, null, 60, "QUATRIEME", new DateTime(2026, 3, 5, 8, 51, 3, 547, DateTimeKind.Utc).AddTicks(494), false, "Quatrieme", "4ème", null, null },
                    { new Guid("9f6788f4-a7b2-46bb-8e19-91cc42263bb1"), new Guid("fcc3f2ef-a2e1-453f-9282-0f702508570e"), null, null, null, 60, "CP", new DateTime(2026, 3, 5, 8, 51, 3, 547, DateTimeKind.Utc).AddTicks(230), false, "CP", "CP", null, null },
                    { new Guid("ab8d07bb-31ca-461f-adc9-bda972bc4fa5"), new Guid("fcc3f2ef-a2e1-453f-9282-0f702508570e"), null, null, null, 60, "SIXIEME", new DateTime(2026, 3, 5, 8, 51, 3, 547, DateTimeKind.Utc).AddTicks(435), false, "Sixieme", "6ème", null, null },
                    { new Guid("b0e6ae13-2496-4d27-a17c-72abb751ba40"), new Guid("fcc3f2ef-a2e1-453f-9282-0f702508570e"), null, null, null, 60, "SECONDE", new DateTime(2026, 3, 5, 8, 51, 3, 547, DateTimeKind.Utc).AddTicks(616), false, "Seconde", "2nde", null, null },
                    { new Guid("b2042240-0d00-415f-ad6e-8fb96106adbd"), new Guid("fcc3f2ef-a2e1-453f-9282-0f702508570e"), null, null, null, 60, "TERMINALE", new DateTime(2026, 3, 5, 8, 51, 3, 547, DateTimeKind.Utc).AddTicks(643), false, "Terminale", "Tle", null, null },
                    { new Guid("b6f7277b-ef50-469a-a659-921c0446048c"), new Guid("fcc3f2ef-a2e1-453f-9282-0f702508570e"), null, null, null, 60, "CM1", new DateTime(2026, 3, 5, 8, 51, 3, 547, DateTimeKind.Utc).AddTicks(325), false, "CM1", "CM1", null, null },
                    { new Guid("bec759e8-efa0-4cad-810c-6d1daeaf187e"), new Guid("fcc3f2ef-a2e1-453f-9282-0f702508570e"), null, null, null, 60, "PREMIERE", new DateTime(2026, 3, 5, 8, 51, 3, 547, DateTimeKind.Utc).AddTicks(633), false, "Premiere", "1ère", null, null },
                    { new Guid("e78a72d3-aa28-4fa4-be87-be108899561d"), new Guid("fcc3f2ef-a2e1-453f-9282-0f702508570e"), null, null, null, 60, "TROISIEME", new DateTime(2026, 3, 5, 8, 51, 3, 547, DateTimeKind.Utc).AddTicks(506), false, "Troisieme", "3ème", null, null }
                });

            migrationBuilder.InsertData(
                table: "Periodes",
                columns: new[] { "Id", "AnneeScolaireId", "ArchiveReason", "ArchivedAt", "ArchivedBy", "CreatedAt", "DateDebut", "DateFin", "IsArchived", "Libelle", "Numero", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("7f55028a-1323-4b40-a7ec-d253f01625b3"), new Guid("fcc3f2ef-a2e1-453f-9282-0f702508570e"), null, null, null, new DateTime(2026, 3, 5, 8, 51, 2, 208, DateTimeKind.Utc).AddTicks(3347), new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 6, 30, 23, 59, 59, 0, DateTimeKind.Utc), false, "Trimestre 3", 3, "Trimestre", null },
                    { new Guid("8cbbf1e5-c55f-44ac-ac00-d23826fca666"), new Guid("fcc3f2ef-a2e1-453f-9282-0f702508570e"), null, null, null, new DateTime(2026, 3, 5, 8, 51, 2, 208, DateTimeKind.Utc).AddTicks(3341), new DateTime(2025, 1, 7, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2025, 3, 31, 23, 59, 59, 0, DateTimeKind.Utc), false, "Trimestre 2", 2, "Trimestre", null },
                    { new Guid("a6b92551-0982-438d-ace8-3e415b925a53"), new Guid("fcc3f2ef-a2e1-453f-9282-0f702508570e"), null, null, null, new DateTime(2026, 3, 5, 8, 51, 2, 208, DateTimeKind.Utc).AddTicks(3332), new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2024, 12, 15, 23, 59, 59, 0, DateTimeKind.Utc), false, "Trimestre 1", 1, "Trimestre", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType",
                table: "AuditLogs",
                column: "EntityType");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UtilisateurId",
                table: "AuditLogs",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_AnneeScolaireId",
                table: "Classes",
                column: "AnneeScolaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Eleves_AnneeScolaireId",
                table: "Eleves",
                column: "AnneeScolaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Eleves_ClasseId",
                table: "Eleves",
                column: "ClasseId");

            migrationBuilder.CreateIndex(
                name: "IX_Eleves_FamilleId",
                table: "Eleves",
                column: "FamilleId");

            migrationBuilder.CreateIndex(
                name: "IX_Eleves_Matricule",
                table: "Eleves",
                column: "Matricule",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Frais_EleveId",
                table: "Frais",
                column: "EleveId");

            migrationBuilder.CreateIndex(
                name: "IX_Frais_PeriodeId",
                table: "Frais",
                column: "PeriodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Frais_TypeFraisId",
                table: "Frais",
                column: "TypeFraisId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_EleveId",
                table: "Notes",
                column: "EleveId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_MatiereId",
                table: "Notes",
                column: "MatiereId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_PeriodeId",
                table: "Notes",
                column: "PeriodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_EnregistrePar",
                table: "Paiements",
                column: "EnregistrePar");

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_FamilleId",
                table: "Paiements",
                column: "FamilleId");

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_NumeroPaiement",
                table: "Paiements",
                column: "NumeroPaiement",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Periodes_AnneeScolaireId",
                table: "Periodes",
                column: "AnneeScolaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Username",
                table: "Utilisateurs",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VentilationsPaiement_EleveId",
                table: "VentilationsPaiement",
                column: "EleveId");

            migrationBuilder.CreateIndex(
                name: "IX_VentilationsPaiement_FraisId",
                table: "VentilationsPaiement",
                column: "FraisId");

            migrationBuilder.CreateIndex(
                name: "IX_VentilationsPaiement_PaiementId",
                table: "VentilationsPaiement",
                column: "PaiementId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Notes");

            migrationBuilder.DropTable(
                name: "VentilationsPaiement");

            migrationBuilder.DropTable(
                name: "Matieres");

            migrationBuilder.DropTable(
                name: "Frais");

            migrationBuilder.DropTable(
                name: "Paiements");

            migrationBuilder.DropTable(
                name: "Eleves");

            migrationBuilder.DropTable(
                name: "Periodes");

            migrationBuilder.DropTable(
                name: "TypeFrais");

            migrationBuilder.DropTable(
                name: "Utilisateurs");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Familles");

            migrationBuilder.DropTable(
                name: "AnneeScolaires");
        }
    }
}
