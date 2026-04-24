using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolFlow.Infrastructure.Migrations
{
    public partial class V3_SuperAdminNullableEcoleId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rendre EcoleId nullable pour permettre le SuperAdmin (sans école)
            migrationBuilder.DropForeignKey(
                name: "FK_Utilisateurs_Ecoles_EcoleId",
                table: "Utilisateurs");

            migrationBuilder.AlterColumn<Guid>(
                name: "EcoleId",
                table: "Utilisateurs",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Utilisateurs_Ecoles_EcoleId",
                table: "Utilisateurs",
                column: "EcoleId",
                principalTable: "Ecoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Utilisateurs_Ecoles_EcoleId",
                table: "Utilisateurs");

            migrationBuilder.AlterColumn<Guid>(
                name: "EcoleId",
                table: "Utilisateurs",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Utilisateurs_Ecoles_EcoleId",
                table: "Utilisateurs",
                column: "EcoleId",
                principalTable: "Ecoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
