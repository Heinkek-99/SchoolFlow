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
                        principalColumn: "Id");
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
                    UtilisateurId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                        onDelete: ReferentialAction.SetNull);
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
                        onDelete: ReferentialAction.Restrict);
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
                    ClasseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AnneeScolaireId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Nationalite = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupeSanguin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Allergies = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContactUrgence = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarques = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Eleves_Classes_ClasseId",
                        column: x => x.ClasseId,
                        principalTable: "Classes",
                        principalColumn: "Id");
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
                    Remarques = table.Column<string>(type: "nvarchar(max)", nullable: true),
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
                    EleveId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MatiereId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PeriodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
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
                        principalColumn: "Id");
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
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaiementId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EleveId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FraisId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MontantVentile = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarque = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                values: new object[] { new Guid("49ba4273-ba81-46e2-a044-47b46554cdcd"), null, null, null, new DateTime(2025, 12, 18, 12, 49, 29, 925, DateTimeKind.Utc).AddTicks(1765), new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), true, false, "2024-2025", null });

            migrationBuilder.InsertData(
                table: "TypeFrais",
                columns: new[] { "Id", "ArchiveReason", "ArchivedAt", "ArchivedBy", "Categorie", "Code", "CreatedAt", "Description", "GenerationAutomatique", "IsArchived", "IsObligatoire", "IsRecurrent", "Libelle", "MontantsParNiveau", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("790fa98b-e947-4c19-b1d6-3cd1b63208e4"), null, null, null, "Cantine", "CANT", new DateTime(2025, 12, 18, 12, 49, 31, 31, DateTimeKind.Utc).AddTicks(302), "Frais de cantine mensuel", false, false, false, true, "Cantine", "{\"CP\":15000,\"CE1\":15000,\"CE2\":15000,\"CM1\":15000,\"CM2\":15000,\"Sixieme\":15000,\"Cinquieme\":15000,\"Quatrieme\":15000,\"Troisieme\":15000,\"Seconde\":15000,\"Premiere\":15000,\"Terminale\":15000}", null },
                    { new Guid("91bb45e7-6e26-47a8-a74d-f0c2c46a1294"), null, null, null, "Scolarite", "SCOL", new DateTime(2025, 12, 18, 12, 49, 31, 31, DateTimeKind.Utc).AddTicks(296), "Frais de scolarité trimestriel", true, false, true, true, "Scolarité", "{\"CP\":60000,\"CE1\":60000,\"CE2\":60000,\"CM1\":70000,\"CM2\":70000,\"Sixieme\":80000,\"Cinquieme\":80000,\"Quatrieme\":85000,\"Troisieme\":85000,\"Seconde\":90000,\"Premiere\":90000,\"Terminale\":90000}", null },
                    { new Guid("f02d7a06-dfa7-4699-b070-653727fab283"), null, null, null, "Inscription", "INSC", new DateTime(2025, 12, 18, 12, 49, 31, 31, DateTimeKind.Utc).AddTicks(288), null, true, false, true, false, "Frais d'inscription", "{\"CP\":5000,\"CE1\":5000,\"CE2\":5000,\"CM1\":5000,\"CM2\":5000,\"Sixieme\":5000,\"Cinquieme\":5000,\"Quatrieme\":5000,\"Troisieme\":5000,\"Seconde\":5000,\"Premiere\":5000,\"Terminale\":5000}", null }
                });

            migrationBuilder.InsertData(
                table: "Utilisateurs",
                columns: new[] { "Id", "ArchiveReason", "ArchivedAt", "ArchivedBy", "CreatedAt", "Email", "FailedLoginAttempts", "IsActive", "IsArchived", "LastLoginAt", "LockedUntil", "Nom", "PasswordHash", "Prenom", "Role", "Telephone", "UpdatedAt", "Username" },
                values: new object[,]
                {
                    { new Guid("0a00556b-f5f0-4f6a-beca-90dc07a827ff"), null, null, null, new DateTime(2025, 12, 18, 12, 49, 30, 479, DateTimeKind.Utc).AddTicks(5246), "directeur@schoolflow.com", 0, true, false, null, null, "Durand", "$2a$11$nLJUJcrV97J3hAJBNhMg8OrEAFuautiqLpOvQwXIBKfjx1Ow5lARK", "Marie", "Directeur", null, null, "directeur" },
                    { new Guid("4805e2e0-a55b-4042-9257-f377c5afc201"), null, null, null, new DateTime(2025, 12, 18, 12, 49, 30, 207, DateTimeKind.Utc).AddTicks(7042), "admin@schoolflow.com", 0, true, false, null, null, "Administrateur", "$2a$11$kGyw90OXwsLJ9og5wnSjLucOu4C.43zhE8dLUsPQGcnhp9pvhOotG", "Système", "Admin", null, null, "admin" },
                    { new Guid("87cae480-fb8c-4d41-98ea-eb8df3c9ba7c"), null, null, null, new DateTime(2025, 12, 18, 12, 49, 30, 757, DateTimeKind.Utc).AddTicks(7285), "comptable@schoolflow.com", 0, true, false, null, null, "Martin", "$2a$11$GNbf6QSQ.9cB4awrYnKi7ufUJlnXTj8zmSN8W8FPfqB4hZC/wI1TW", "Sophie", "Comptable", null, null, "comptable" },
                    { new Guid("d080b4b5-1314-472f-9cbc-3ef83430ce56"), null, null, null, new DateTime(2025, 12, 18, 12, 49, 31, 30, DateTimeKind.Utc).AddTicks(7434), "secretaire@schoolflow.com", 0, true, false, null, null, "Leroy", "$2a$11$gpJiQcIQuJ60pIA5rx/.f.0ySSoWYNOXtqDSFt5DyW1.MLnKF1mTW", "Paul", "Secretaire", null, null, "secretaire" }
                });

            migrationBuilder.InsertData(
                table: "Classes",
                columns: new[] { "Id", "AnneeScolaireId", "ArchiveReason", "ArchivedAt", "ArchivedBy", "CapaciteMax", "Code", "CreatedAt", "IsArchived", "Niveau", "Nom", "Section", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("22234eb4-a686-4e41-b90f-f479ffafbd53"), new Guid("49ba4273-ba81-46e2-a044-47b46554cdcd"), null, null, null, 60, "CINQUIEME", new DateTime(2025, 12, 18, 12, 49, 31, 30, DateTimeKind.Utc).AddTicks(8293), false, "Cinquieme", "5ème", null, null },
                    { new Guid("3468793f-ea19-4058-bf64-431d8ec0bb0d"), new Guid("49ba4273-ba81-46e2-a044-47b46554cdcd"), null, null, null, 60, "SECONDE", new DateTime(2025, 12, 18, 12, 49, 31, 30, DateTimeKind.Utc).AddTicks(8342), false, "Seconde", "2nde", null, null },
                    { new Guid("396f3e27-e879-4e77-ac5a-d82eaeb2c47f"), new Guid("49ba4273-ba81-46e2-a044-47b46554cdcd"), null, null, null, 60, "CP", new DateTime(2025, 12, 18, 12, 49, 31, 30, DateTimeKind.Utc).AddTicks(8070), false, "CP", "CP", null, null },
                    { new Guid("3fb775e4-dbe3-4bee-a7bd-2cbf6690dc5f"), new Guid("49ba4273-ba81-46e2-a044-47b46554cdcd"), null, null, null, 60, "SIXIEME", new DateTime(2025, 12, 18, 12, 49, 31, 30, DateTimeKind.Utc).AddTicks(8231), false, "Sixieme", "6ème", null, null },
                    { new Guid("434526d5-d24c-47b5-825a-77f1ac49b80c"), new Guid("49ba4273-ba81-46e2-a044-47b46554cdcd"), null, null, null, 60, "CM2", new DateTime(2025, 12, 18, 12, 49, 31, 30, DateTimeKind.Utc).AddTicks(8175), false, "CM2", "CM2", null, null },
                    { new Guid("44de80f0-81bd-4a04-aecd-1d4fb23a6480"), new Guid("49ba4273-ba81-46e2-a044-47b46554cdcd"), null, null, null, 60, "CE1", new DateTime(2025, 12, 18, 12, 49, 31, 30, DateTimeKind.Utc).AddTicks(8100), false, "CE1", "CE1", null, null },
                    { new Guid("6b343a4e-da14-469d-8318-6bc9e1974648"), new Guid("49ba4273-ba81-46e2-a044-47b46554cdcd"), null, null, null, 60, "CM1", new DateTime(2025, 12, 18, 12, 49, 31, 30, DateTimeKind.Utc).AddTicks(8158), false, "CM1", "CM1", null, null },
                    { new Guid("7839b468-6cf3-419d-b6a9-30ac09127f96"), new Guid("49ba4273-ba81-46e2-a044-47b46554cdcd"), null, null, null, 60, "QUATRIEME", new DateTime(2025, 12, 18, 12, 49, 31, 30, DateTimeKind.Utc).AddTicks(8316), false, "Quatrieme", "4ème", null, null },
                    { new Guid("a098cf9c-5c90-4faa-b715-5c4805eeb8fc"), new Guid("49ba4273-ba81-46e2-a044-47b46554cdcd"), null, null, null, 60, "PREMIERE", new DateTime(2025, 12, 18, 12, 49, 31, 30, DateTimeKind.Utc).AddTicks(8355), false, "Premiere", "1ère", null, null },
                    { new Guid("a1597343-e4dd-4f34-ae06-9fc18980ce2d"), new Guid("49ba4273-ba81-46e2-a044-47b46554cdcd"), null, null, null, 60, "CE2", new DateTime(2025, 12, 18, 12, 49, 31, 30, DateTimeKind.Utc).AddTicks(8112), false, "CE2", "CE2", null, null },
                    { new Guid("c73fd978-80b0-4f0f-91c2-66c0ab2315be"), new Guid("49ba4273-ba81-46e2-a044-47b46554cdcd"), null, null, null, 60, "TERMINALE", new DateTime(2025, 12, 18, 12, 49, 31, 30, DateTimeKind.Utc).AddTicks(8371), false, "Terminale", "Tle", null, null },
                    { new Guid("ccdc6543-ca4d-4d41-9139-2105bc5ab7bb"), new Guid("49ba4273-ba81-46e2-a044-47b46554cdcd"), null, null, null, 60, "TROISIEME", new DateTime(2025, 12, 18, 12, 49, 31, 30, DateTimeKind.Utc).AddTicks(8327), false, "Troisieme", "3ème", null, null }
                });

            migrationBuilder.InsertData(
                table: "Periodes",
                columns: new[] { "Id", "AnneeScolaireId", "ArchiveReason", "ArchivedAt", "ArchivedBy", "CreatedAt", "DateDebut", "DateFin", "IsArchived", "Libelle", "Numero", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("79f89853-0522-4f0c-8a9d-2fab1ecf20b5"), new Guid("49ba4273-ba81-46e2-a044-47b46554cdcd"), null, null, null, new DateTime(2025, 12, 18, 12, 49, 29, 925, DateTimeKind.Utc).AddTicks(2507), new DateTime(2025, 4, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Trimestre 3", 3, "Trimestre", null },
                    { new Guid("c39b2291-1b35-4726-a853-23030a2955b1"), new Guid("49ba4273-ba81-46e2-a044-47b46554cdcd"), null, null, null, new DateTime(2025, 12, 18, 12, 49, 29, 925, DateTimeKind.Utc).AddTicks(2503), new DateTime(2025, 1, 7, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 3, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Trimestre 2", 2, "Trimestre", null },
                    { new Guid("e35c96b7-f8f8-4845-80c7-393e6582eedb"), new Guid("49ba4273-ba81-46e2-a044-47b46554cdcd"), null, null, null, new DateTime(2025, 12, 18, 12, 49, 29, 925, DateTimeKind.Utc).AddTicks(2495), new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2024, 12, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), false, "Trimestre 1", 1, "Trimestre", null }
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
