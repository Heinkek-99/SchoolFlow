using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SchoolFlow.Infrastructure.Data.Migrations
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Libelle = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArchiveReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnneeScolaires", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Familles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NomPere = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PrenomPere = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TelephonePere = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EmailPere = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ProfessionPere = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NomMere = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PrenomMere = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TelephoneMere = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailMere = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfessionMere = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Adresse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Ville = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    QuartierCommune = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TelephonePrincipal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TelephoneSecondaire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArchiveReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Familles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Matieres",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Libelle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Coefficient = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArchiveReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matieres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TypeFrais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Libelle = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Categorie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsRecurrent = table.Column<bool>(type: "bit", nullable: false),
                    IsObligatoire = table.Column<bool>(type: "bit", nullable: false),
                    GenerationAutomatique = table.Column<bool>(type: "bit", nullable: false),
                    MontantsParNiveau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArchiveReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeFrais", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilisateurs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockedUntil = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArchiveReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilisateurs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Niveau = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Section = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CapaciteMax = table.Column<int>(type: "int", nullable: false),
                    AnneeScolaireId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArchiveReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Libelle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateFin = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnneeScolaireId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArchiveReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArchiveReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Paiements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumeroPaiement = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FamilleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MontantTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DatePaiement = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModePaiement = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Commentaire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EnregistrePar = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArchiveReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Eleves",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Matricule = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Prenom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DateNaissance = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LieuNaissance = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Sexe = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhotoPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FamilleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClasseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnneeScolaireId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nationalite = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupeSanguin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Allergies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactUrgence = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Statut = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateInscription = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArchiveReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EleveId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TypeFraisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Montant = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    MontantPaye = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DateEcheance = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Commentaire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArchiveReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EleveId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatiereId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PeriodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Valeur = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    NoteSur = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Commentaire = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArchiveReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notes_Eleves_EleveId",
                        column: x => x.EleveId,
                        principalTable: "Eleves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notes_Matieres_MatiereId",
                        column: x => x.MatiereId,
                        principalTable: "Matieres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notes_Periodes_PeriodeId",
                        column: x => x.PeriodeId,
                        principalTable: "Periodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VentilationsPaiement",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaiementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FraisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MontantVentile = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ArchiveReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentilationsPaiement", x => x.Id);
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
                values: new object[] { new Guid("b659f064-efea-4080-b440-64f46624034d"), null, null, null, new DateTime(2025, 12, 11, 14, 3, 7, 248, DateTimeKind.Utc).AddTicks(6736), new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), true, false, "2024-2025", null });

            migrationBuilder.InsertData(
                table: "TypeFrais",
                columns: new[] { "Id", "ArchiveReason", "ArchivedAt", "ArchivedBy", "Categorie", "Code", "CreatedAt", "Description", "GenerationAutomatique", "IsArchived", "IsObligatoire", "IsRecurrent", "Libelle", "MontantsParNiveau", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("094e9e46-6cf0-4b0b-aca7-514fcf3d4897"), null, null, null, "Scolarite", "SCOL", new DateTime(2025, 12, 11, 14, 3, 8, 416, DateTimeKind.Utc).AddTicks(1893), "Frais de scolarité trimestriel", true, false, true, true, "Scolarité", "{\"CP\":60000,\"CE1\":60000,\"CE2\":60000,\"CM1\":70000,\"CM2\":70000,\"Sixieme\":80000,\"Cinquieme\":80000,\"Quatrieme\":85000,\"Troisieme\":85000,\"Seconde\":90000,\"Premiere\":90000,\"Terminale\":90000}", null },
                    { new Guid("8d78f55a-2937-48b0-9567-7b87b255a939"), null, null, null, "Inscription", "INSC", new DateTime(2025, 12, 11, 14, 3, 8, 416, DateTimeKind.Utc).AddTicks(1884), null, true, false, true, false, "Frais d'inscription", "{\"CP\":5000,\"CE1\":5000,\"CE2\":5000,\"CM1\":5000,\"CM2\":5000,\"Sixieme\":5000,\"Cinquieme\":5000,\"Quatrieme\":5000,\"Troisieme\":5000,\"Seconde\":5000,\"Premiere\":5000,\"Terminale\":5000}", null },
                    { new Guid("a5b21246-bd82-4c04-95d2-f3752f1801d1"), null, null, null, "Cantine", "CANT", new DateTime(2025, 12, 11, 14, 3, 8, 416, DateTimeKind.Utc).AddTicks(1933), "Frais de cantine mensuel", false, false, false, true, "Cantine", "{\"CP\":15000,\"CE1\":15000,\"CE2\":15000,\"CM1\":15000,\"CM2\":15000,\"Sixieme\":15000,\"Cinquieme\":15000,\"Quatrieme\":15000,\"Troisieme\":15000,\"Seconde\":15000,\"Premiere\":15000,\"Terminale\":15000}", null }
                });

            migrationBuilder.InsertData(
                table: "Utilisateurs",
                columns: new[] { "Id", "ArchiveReason", "ArchivedAt", "ArchivedBy", "CreatedAt", "Email", "FailedLoginAttempts", "IsActive", "IsArchived", "LastLoginAt", "LockedUntil", "Nom", "PasswordHash", "Prenom", "Role", "Telephone", "UpdatedAt", "Username" },
                values: new object[] { new Guid("f0adb616-893b-4767-89d5-a3cbaa6b29f7"), null, null, null, new DateTime(2025, 12, 11, 14, 3, 8, 415, DateTimeKind.Utc).AddTicks(8839), "admin@schoolflow.com", 0, true, false, null, null, "Administrateur", "$2a$11$3y2kbFt4EdZZWxvCq1C4muosp5Juf1IasLotnXxSTvQB4IpUhteEi", "Système", "Admin", null, null, "admin" });

            migrationBuilder.InsertData(
                table: "Classes",
                columns: new[] { "Id", "AnneeScolaireId", "ArchiveReason", "ArchivedAt", "ArchivedBy", "CapaciteMax", "Code", "CreatedAt", "IsArchived", "Niveau", "Nom", "Section", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("144ada19-4991-4261-b0c1-045bce67fc1f"), new Guid("b659f064-efea-4080-b440-64f46624034d"), null, null, null, 40, "QUATRIEME", new DateTime(2025, 12, 11, 14, 3, 8, 416, DateTimeKind.Utc).AddTicks(246), false, "Quatrieme", "4ème", null, null },
                    { new Guid("1ac9af0f-b51d-4dd8-b530-899e554308b1"), new Guid("b659f064-efea-4080-b440-64f46624034d"), null, null, null, 40, "PREMIERE", new DateTime(2025, 12, 11, 14, 3, 8, 416, DateTimeKind.Utc).AddTicks(282), false, "Premiere", "1ère", null, null },
                    { new Guid("3adfa213-1689-4ab0-97a0-b30bc4c52c73"), new Guid("b659f064-efea-4080-b440-64f46624034d"), null, null, null, 40, "CM2", new DateTime(2025, 12, 11, 14, 3, 8, 416, DateTimeKind.Utc).AddTicks(174), false, "CM2", "CM2", null, null },
                    { new Guid("3f435c70-859f-4585-8d9f-1fe91263b745"), new Guid("b659f064-efea-4080-b440-64f46624034d"), null, null, null, 40, "SECONDE", new DateTime(2025, 12, 11, 14, 3, 8, 416, DateTimeKind.Utc).AddTicks(267), false, "Seconde", "2nde", null, null },
                    { new Guid("5f18d29b-689e-47fa-9836-ff3b34eea14f"), new Guid("b659f064-efea-4080-b440-64f46624034d"), null, null, null, 40, "SIXIEME", new DateTime(2025, 12, 11, 14, 3, 8, 416, DateTimeKind.Utc).AddTicks(208), false, "Sixieme", "6ème", null, null },
                    { new Guid("84dad474-0c3f-4c18-ba6d-9a708935e49b"), new Guid("b659f064-efea-4080-b440-64f46624034d"), null, null, null, 40, "TROISIEME", new DateTime(2025, 12, 11, 14, 3, 8, 416, DateTimeKind.Utc).AddTicks(256), false, "Troisieme", "3ème", null, null },
                    { new Guid("89695367-a589-4783-8e92-7d994f9024cc"), new Guid("b659f064-efea-4080-b440-64f46624034d"), null, null, null, 40, "CP", new DateTime(2025, 12, 11, 14, 3, 8, 416, DateTimeKind.Utc).AddTicks(79), false, "CP", "CP", null, null },
                    { new Guid("8c1df256-b401-4bc6-8c7a-c1b74e02c618"), new Guid("b659f064-efea-4080-b440-64f46624034d"), null, null, null, 40, "CE1", new DateTime(2025, 12, 11, 14, 3, 8, 416, DateTimeKind.Utc).AddTicks(110), false, "CE1", "CE1", null, null },
                    { new Guid("92eaeb5a-c83b-4187-a830-75a247b1353b"), new Guid("b659f064-efea-4080-b440-64f46624034d"), null, null, null, 40, "CE2", new DateTime(2025, 12, 11, 14, 3, 8, 416, DateTimeKind.Utc).AddTicks(147), false, "CE2", "CE2", null, null },
                    { new Guid("cce44d79-8b86-46e4-acc4-373c941c57f7"), new Guid("b659f064-efea-4080-b440-64f46624034d"), null, null, null, 40, "CINQUIEME", new DateTime(2025, 12, 11, 14, 3, 8, 416, DateTimeKind.Utc).AddTicks(234), false, "Cinquieme", "5ème", null, null },
                    { new Guid("eaadf8f5-998a-4105-a20a-a169ce9e83ff"), new Guid("b659f064-efea-4080-b440-64f46624034d"), null, null, null, 40, "TERMINALE", new DateTime(2025, 12, 11, 14, 3, 8, 416, DateTimeKind.Utc).AddTicks(291), false, "Terminale", "Tle", null, null },
                    { new Guid("f6d13458-6e2c-4790-bc01-b293d5a33975"), new Guid("b659f064-efea-4080-b440-64f46624034d"), null, null, null, 40, "CM1", new DateTime(2025, 12, 11, 14, 3, 8, 416, DateTimeKind.Utc).AddTicks(161), false, "CM1", "CM1", null, null }
                });

            migrationBuilder.InsertData(
                table: "Periodes",
                columns: new[] { "Id", "AnneeScolaireId", "ArchiveReason", "ArchivedAt", "ArchivedBy", "CreatedAt", "DateDebut", "DateFin", "IsArchived", "Libelle", "Numero", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("2e8bf0fe-2652-41a6-9c4d-1665d8a752e0"), new Guid("b659f064-efea-4080-b440-64f46624034d"), null, null, null, new DateTime(2025, 12, 11, 14, 3, 7, 248, DateTimeKind.Utc).AddTicks(7591), new DateTime(2025, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Trimestre 2", 2, "Trimestre", null },
                    { new Guid("d98967b9-d30e-421d-bc11-c24936347e6b"), new Guid("b659f064-efea-4080-b440-64f46624034d"), null, null, null, new DateTime(2025, 12, 11, 14, 3, 7, 248, DateTimeKind.Utc).AddTicks(7596), new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Trimestre 3", 3, "Trimestre", null },
                    { new Guid("eaa9ca46-5eca-485f-964c-bc36a3a1b51c"), new Guid("b659f064-efea-4080-b440-64f46624034d"), null, null, null, new DateTime(2025, 12, 11, 14, 3, 7, 248, DateTimeKind.Utc).AddTicks(7561), new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 12, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Trimestre 1", 1, "Trimestre", null }
                });

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
