using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V4_ModulesAcademiques : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Eleves_EleveId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Matieres_MatiereId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Periodes_PeriodeId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Paiements_Utilisateurs_EnregistreParUtilisateurId",
                table: "Paiements");

            migrationBuilder.DropForeignKey(
                name: "FK_VentilationsPaiement_Eleves_EleveId",
                table: "VentilationsPaiement");

            migrationBuilder.DropIndex(
                name: "IX_Paiements_EnregistreParUtilisateurId",
                table: "Paiements");

            migrationBuilder.DropColumn(
                name: "EnregistreParUtilisateurId",
                table: "Paiements");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Notes",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "PeriodeId",
                table: "Notes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MatiereId",
                table: "Notes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "EleveId",
                table: "Notes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EcoleId",
                table: "Notes",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "EstPubliee",
                table: "Notes",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "EvaluationId",
                table: "Notes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Coefficient",
                table: "Matieres",
                type: "numeric(4,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Matieres",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "EcoleId",
                table: "Matieres",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Matieres",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SousSysteme",
                table: "Matieres",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Bulletins",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EleveId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClasseId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnneeScolaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    MoyenneGenerale = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    RangClasse = table.Column<int>(type: "integer", nullable: false),
                    EffectifClasse = table.Column<int>(type: "integer", nullable: false),
                    AppreciationProfesseur = table.Column<string>(type: "text", nullable: true),
                    AppreciationDirecteur = table.Column<string>(type: "text", nullable: true),
                    EstPublie = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true),
                    EcoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bulletins", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bulletins_Classes_ClasseId",
                        column: x => x.ClasseId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bulletins_Eleves_EleveId",
                        column: x => x.EleveId,
                        principalTable: "Eleves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bulletins_Periodes_PeriodeId",
                        column: x => x.PeriodeId,
                        principalTable: "Periodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Disciplines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EleveId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnneeScolaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    SignalePar = table.Column<Guid>(type: "uuid", nullable: true),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Motif = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DateDiscipline = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Mesure = table.Column<string>(type: "text", nullable: true),
                    NotifieParent = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true),
                    EcoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Disciplines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Disciplines_AnneeScolaires_AnneeScolaireId",
                        column: x => x.AnneeScolaireId,
                        principalTable: "AnneeScolaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Disciplines_Eleves_EleveId",
                        column: x => x.EleveId,
                        principalTable: "Eleves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Disciplines_Utilisateurs_SignalePar",
                        column: x => x.SignalePar,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Enseignants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UtilisateurId = table.Column<Guid>(type: "uuid", nullable: false),
                    Telephone = table.Column<string>(type: "text", nullable: false),
                    Specialite = table.Column<string>(type: "text", nullable: true),
                    Grade = table.Column<string>(type: "text", nullable: true),
                    AnneesExperience = table.Column<int>(type: "integer", nullable: true),
                    Bio = table.Column<string>(type: "text", nullable: true),
                    PhotoPath = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true),
                    EcoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enseignants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enseignants_Utilisateurs_UtilisateurId",
                        column: x => x.UtilisateurId,
                        principalTable: "Utilisateurs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Evaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClasseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatiereId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnneeScolaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    DateEvaluation = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    NoteSur = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    EstPubliee = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true),
                    EcoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Evaluations_Classes_ClasseId",
                        column: x => x.ClasseId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Evaluations_Matieres_MatiereId",
                        column: x => x.MatiereId,
                        principalTable: "Matieres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Evaluations_Periodes_PeriodeId",
                        column: x => x.PeriodeId,
                        principalTable: "Periodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Examens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AnneeScolaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nom = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    DateDebut = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    DateFin = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true),
                    EcoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Examens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Examens_AnneeScolaires_AnneeScolaireId",
                        column: x => x.AnneeScolaireId,
                        principalTable: "AnneeScolaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LignesBulletin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BulletinId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatiereId = table.Column<Guid>(type: "uuid", nullable: false),
                    MoyenneMatiere = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Coefficient = table.Column<decimal>(type: "numeric(4,2)", nullable: false),
                    MoyennePonderee = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    AppreciationEnseignant = table.Column<string>(type: "text", nullable: true),
                    MoyenneClasse = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true),
                    EcoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesBulletin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LignesBulletin_Bulletins_BulletinId",
                        column: x => x.BulletinId,
                        principalTable: "Bulletins",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LignesBulletin_Matieres_MatiereId",
                        column: x => x.MatiereId,
                        principalTable: "Matieres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CreneauxHoraires",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClasseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatiereId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnneeScolaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    EnseignantId = table.Column<Guid>(type: "uuid", nullable: true),
                    Jour = table.Column<string>(type: "text", nullable: false),
                    HeureDebut = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    HeureFin = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    Salle = table.Column<string>(type: "text", nullable: true),
                    Remarque = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true),
                    EcoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreneauxHoraires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreneauxHoraires_Classes_ClasseId",
                        column: x => x.ClasseId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CreneauxHoraires_Enseignants_EnseignantId",
                        column: x => x.EnseignantId,
                        principalTable: "Enseignants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CreneauxHoraires_Matieres_MatiereId",
                        column: x => x.MatiereId,
                        principalTable: "Matieres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MatiereEnseignants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EnseignantId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatiereId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClasseId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnneeScolaireId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true),
                    EcoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatiereEnseignants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatiereEnseignants_Classes_ClasseId",
                        column: x => x.ClasseId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MatiereEnseignants_Enseignants_EnseignantId",
                        column: x => x.EnseignantId,
                        principalTable: "Enseignants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatiereEnseignants_Matieres_MatiereId",
                        column: x => x.MatiereId,
                        principalTable: "Matieres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InscriptionsExamen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamenId = table.Column<Guid>(type: "uuid", nullable: false),
                    EleveId = table.Column<Guid>(type: "uuid", nullable: false),
                    NumeroCandidat = table.Column<string>(type: "text", nullable: true),
                    Admis = table.Column<bool>(type: "boolean", nullable: false),
                    MoyenneExamen = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    Mention = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ArchivedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchiveReason = table.Column<string>(type: "text", nullable: true),
                    EcoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InscriptionsExamen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InscriptionsExamen_Eleves_EleveId",
                        column: x => x.EleveId,
                        principalTable: "Eleves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InscriptionsExamen_Examens_ExamenId",
                        column: x => x.ExamenId,
                        principalTable: "Examens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_EnregistrePar",
                table: "Paiements",
                column: "EnregistrePar");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_EvaluationId",
                table: "Notes",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_Bulletins_ClasseId",
                table: "Bulletins",
                column: "ClasseId");

            migrationBuilder.CreateIndex(
                name: "IX_Bulletins_EleveId_PeriodeId_AnneeScolaireId",
                table: "Bulletins",
                columns: new[] { "EleveId", "PeriodeId", "AnneeScolaireId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bulletins_PeriodeId",
                table: "Bulletins",
                column: "PeriodeId");

            migrationBuilder.CreateIndex(
                name: "IX_CreneauxHoraires_ClasseId",
                table: "CreneauxHoraires",
                column: "ClasseId");

            migrationBuilder.CreateIndex(
                name: "IX_CreneauxHoraires_EnseignantId",
                table: "CreneauxHoraires",
                column: "EnseignantId");

            migrationBuilder.CreateIndex(
                name: "IX_CreneauxHoraires_MatiereId",
                table: "CreneauxHoraires",
                column: "MatiereId");

            migrationBuilder.CreateIndex(
                name: "IX_Disciplines_AnneeScolaireId",
                table: "Disciplines",
                column: "AnneeScolaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Disciplines_EleveId",
                table: "Disciplines",
                column: "EleveId");

            migrationBuilder.CreateIndex(
                name: "IX_Disciplines_SignalePar",
                table: "Disciplines",
                column: "SignalePar");

            migrationBuilder.CreateIndex(
                name: "IX_Enseignants_UtilisateurId",
                table: "Enseignants",
                column: "UtilisateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_ClasseId",
                table: "Evaluations",
                column: "ClasseId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_MatiereId",
                table: "Evaluations",
                column: "MatiereId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_PeriodeId",
                table: "Evaluations",
                column: "PeriodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Examens_AnneeScolaireId",
                table: "Examens",
                column: "AnneeScolaireId");

            migrationBuilder.CreateIndex(
                name: "IX_InscriptionsExamen_EleveId",
                table: "InscriptionsExamen",
                column: "EleveId");

            migrationBuilder.CreateIndex(
                name: "IX_InscriptionsExamen_ExamenId_EleveId",
                table: "InscriptionsExamen",
                columns: new[] { "ExamenId", "EleveId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LignesBulletin_BulletinId",
                table: "LignesBulletin",
                column: "BulletinId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesBulletin_MatiereId",
                table: "LignesBulletin",
                column: "MatiereId");

            migrationBuilder.CreateIndex(
                name: "IX_MatiereEnseignants_ClasseId",
                table: "MatiereEnseignants",
                column: "ClasseId");

            migrationBuilder.CreateIndex(
                name: "IX_MatiereEnseignants_EnseignantId_MatiereId_ClasseId_AnneeSco~",
                table: "MatiereEnseignants",
                columns: new[] { "EnseignantId", "MatiereId", "ClasseId", "AnneeScolaireId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatiereEnseignants_MatiereId",
                table: "MatiereEnseignants",
                column: "MatiereId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Eleves_EleveId",
                table: "Notes",
                column: "EleveId",
                principalTable: "Eleves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Evaluations_EvaluationId",
                table: "Notes",
                column: "EvaluationId",
                principalTable: "Evaluations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Matieres_MatiereId",
                table: "Notes",
                column: "MatiereId",
                principalTable: "Matieres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Periodes_PeriodeId",
                table: "Notes",
                column: "PeriodeId",
                principalTable: "Periodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Paiements_Utilisateurs_EnregistrePar",
                table: "Paiements",
                column: "EnregistrePar",
                principalTable: "Utilisateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VentilationsPaiement_Eleves_EleveId",
                table: "VentilationsPaiement",
                column: "EleveId",
                principalTable: "Eleves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Eleves_EleveId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Evaluations_EvaluationId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Matieres_MatiereId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Notes_Periodes_PeriodeId",
                table: "Notes");

            migrationBuilder.DropForeignKey(
                name: "FK_Paiements_Utilisateurs_EnregistrePar",
                table: "Paiements");

            migrationBuilder.DropForeignKey(
                name: "FK_VentilationsPaiement_Eleves_EleveId",
                table: "VentilationsPaiement");

            migrationBuilder.DropTable(
                name: "CreneauxHoraires");

            migrationBuilder.DropTable(
                name: "Disciplines");

            migrationBuilder.DropTable(
                name: "Evaluations");

            migrationBuilder.DropTable(
                name: "InscriptionsExamen");

            migrationBuilder.DropTable(
                name: "LignesBulletin");

            migrationBuilder.DropTable(
                name: "MatiereEnseignants");

            migrationBuilder.DropTable(
                name: "Examens");

            migrationBuilder.DropTable(
                name: "Bulletins");

            migrationBuilder.DropTable(
                name: "Enseignants");

            migrationBuilder.DropIndex(
                name: "IX_Paiements_EnregistrePar",
                table: "Paiements");

            migrationBuilder.DropIndex(
                name: "IX_Notes_EvaluationId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "EcoleId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "EstPubliee",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "EvaluationId",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Matieres");

            migrationBuilder.DropColumn(
                name: "EcoleId",
                table: "Matieres");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Matieres");

            migrationBuilder.DropColumn(
                name: "SousSysteme",
                table: "Matieres");

            migrationBuilder.AddColumn<Guid>(
                name: "EnregistreParUtilisateurId",
                table: "Paiements",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Notes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "PeriodeId",
                table: "Notes",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "MatiereId",
                table: "Notes",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "EleveId",
                table: "Notes",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Coefficient",
                table: "Matieres",
                type: "integer",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(4,2)");

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_EnregistreParUtilisateurId",
                table: "Paiements",
                column: "EnregistreParUtilisateurId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Eleves_EleveId",
                table: "Notes",
                column: "EleveId",
                principalTable: "Eleves",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Matieres_MatiereId",
                table: "Notes",
                column: "MatiereId",
                principalTable: "Matieres",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Notes_Periodes_PeriodeId",
                table: "Notes",
                column: "PeriodeId",
                principalTable: "Periodes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Paiements_Utilisateurs_EnregistreParUtilisateurId",
                table: "Paiements",
                column: "EnregistreParUtilisateurId",
                principalTable: "Utilisateurs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_VentilationsPaiement_Eleves_EleveId",
                table: "VentilationsPaiement",
                column: "EleveId",
                principalTable: "Eleves",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
