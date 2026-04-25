using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolFlow.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class V5_PerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Notes_EleveId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_EvaluationId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Frais_EleveId",
                table: "Frais");

            migrationBuilder.DropIndex(
                name: "IX_Evaluations_ClasseId",
                table: "Evaluations");

            migrationBuilder.DropIndex(
                name: "IX_Eleves_FamilleId",
                table: "Eleves");

            migrationBuilder.DropIndex(
                name: "IX_CreneauxHoraires_ClasseId",
                table: "CreneauxHoraires");

            migrationBuilder.DropIndex(
                name: "IX_AnneeScolaires_EcoleId",
                table: "AnneeScolaires");

            migrationBuilder.CreateIndex(
                name: "IX_Utilisateurs_Username_IsArchived",
                table: "Utilisateurs",
                columns: new[] { "Username", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_EcoleId_DatePaiement_IsArchived",
                table: "Paiements",
                columns: new[] { "EcoleId", "DatePaiement", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedAt_RetryCount",
                table: "OutboxMessages",
                columns: new[] { "ProcessedAt", "RetryCount" });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_EleveId_PeriodeId_EcoleId",
                table: "Notes",
                columns: new[] { "EleveId", "PeriodeId", "EcoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_Notes_EvaluationId_EleveId",
                table: "Notes",
                columns: new[] { "EvaluationId", "EleveId" });

            migrationBuilder.CreateIndex(
                name: "IX_Frais_EcoleId_IsArchived",
                table: "Frais",
                columns: new[] { "EcoleId", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_Frais_EleveId_IsArchived",
                table: "Frais",
                columns: new[] { "EleveId", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_Familles_EcoleId_IsArchived",
                table: "Familles",
                columns: new[] { "EcoleId", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_Familles_NomPere",
                table: "Familles",
                column: "NomPere");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_ClasseId_PeriodeId",
                table: "Evaluations",
                columns: new[] { "ClasseId", "PeriodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_Eleves_EcoleId_Statut_IsArchived",
                table: "Eleves",
                columns: new[] { "EcoleId", "Statut", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_Eleves_FamilleId_IsArchived",
                table: "Eleves",
                columns: new[] { "FamilleId", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_CreneauxHoraires_ClasseId_AnneeScolaireId",
                table: "CreneauxHoraires",
                columns: new[] { "ClasseId", "AnneeScolaireId" });

            migrationBuilder.CreateIndex(
                name: "IX_Classes_EcoleId_AnneeScolaireId_IsArchived",
                table: "Classes",
                columns: new[] { "EcoleId", "AnneeScolaireId", "IsArchived" });

            migrationBuilder.CreateIndex(
                name: "IX_AnneeScolaires_EcoleId_IsActive",
                table: "AnneeScolaires",
                columns: new[] { "EcoleId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Utilisateurs_Username_IsArchived",
                table: "Utilisateurs");

            migrationBuilder.DropIndex(
                name: "IX_Paiements_EcoleId_DatePaiement_IsArchived",
                table: "Paiements");

            migrationBuilder.DropIndex(
                name: "IX_OutboxMessages_ProcessedAt_RetryCount",
                table: "OutboxMessages");

            migrationBuilder.DropIndex(
                name: "IX_Notes_EleveId_PeriodeId_EcoleId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Notes_EvaluationId_EleveId",
                table: "Notes");

            migrationBuilder.DropIndex(
                name: "IX_Frais_EcoleId_IsArchived",
                table: "Frais");

            migrationBuilder.DropIndex(
                name: "IX_Frais_EleveId_IsArchived",
                table: "Frais");

            migrationBuilder.DropIndex(
                name: "IX_Familles_EcoleId_IsArchived",
                table: "Familles");

            migrationBuilder.DropIndex(
                name: "IX_Familles_NomPere",
                table: "Familles");

            migrationBuilder.DropIndex(
                name: "IX_Evaluations_ClasseId_PeriodeId",
                table: "Evaluations");

            migrationBuilder.DropIndex(
                name: "IX_Eleves_EcoleId_Statut_IsArchived",
                table: "Eleves");

            migrationBuilder.DropIndex(
                name: "IX_Eleves_FamilleId_IsArchived",
                table: "Eleves");

            migrationBuilder.DropIndex(
                name: "IX_CreneauxHoraires_ClasseId_AnneeScolaireId",
                table: "CreneauxHoraires");

            migrationBuilder.DropIndex(
                name: "IX_Classes_EcoleId_AnneeScolaireId_IsArchived",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_AnneeScolaires_EcoleId_IsActive",
                table: "AnneeScolaires");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_EleveId",
                table: "Notes",
                column: "EleveId");

            migrationBuilder.CreateIndex(
                name: "IX_Notes_EvaluationId",
                table: "Notes",
                column: "EvaluationId");

            migrationBuilder.CreateIndex(
                name: "IX_Frais_EleveId",
                table: "Frais",
                column: "EleveId");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_ClasseId",
                table: "Evaluations",
                column: "ClasseId");

            migrationBuilder.CreateIndex(
                name: "IX_Eleves_FamilleId",
                table: "Eleves",
                column: "FamilleId");

            migrationBuilder.CreateIndex(
                name: "IX_CreneauxHoraires_ClasseId",
                table: "CreneauxHoraires",
                column: "ClasseId");

            migrationBuilder.CreateIndex(
                name: "IX_AnneeScolaires_EcoleId",
                table: "AnneeScolaires",
                column: "EcoleId");
        }
    }
}
