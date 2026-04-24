#!/bin/bash
# ============================================================
# SCRIPT : apply-refactoring.sh
# USAGE  : bash apply-refactoring.sh
# RÔLE   : Installe les packages manquants + crée la migration EF
# ============================================================

set -e

echo "🔧 SchoolFlow — Intégration Refactoring Phase 1"
echo "================================================"

PROJECT_ROOT="$(pwd)"
INFRA="src/SchoolFlow.Infrastructure/SchoolFlow.Infrastructure.csproj"
APP="src/SchoolFlow.Application/SchoolFlow.Application.csproj"
API="src/SchoolFlow.Api/SchoolFlow.Api.csproj"
STARTUP="src/SchoolFlow.Api"

# ── ÉTAPE 1 : Packages manquants ───────────────────────────────────────────

echo ""
echo "📦 Installation des packages..."

# Dapper pour les queries de lecture
dotnet add "$INFRA" package Dapper --version 2.1.28

# Npgsql pour la connexion native PostgreSQL avec Dapper
dotnet add "$INFRA" package Npgsql --version 8.0.3

# (Déjà présent normalement) MediatR pour IPublisher dans DbContext
# dotnet add "$INFRA" package MediatR --version 12.2.0

echo "✅ Packages installés."

# ── ÉTAPE 2 : Vérification de la build ─────────────────────────────────────

echo ""
echo "🔨 Vérification de la build..."
dotnet build "$PROJECT_ROOT" --no-restore -v quiet

echo "✅ Build OK."

# ── ÉTAPE 3 : Créer la migration EF Core ──────────────────────────────────

echo ""
echo "🗄️  Création de la migration EF Core..."

dotnet ef migrations add "AddEcoleMultiTenantOutbox" \
  --project "$INFRA" \
  --startup-project "$STARTUP" \
  --output-dir "Migrations"

echo "✅ Migration créée."

# ── ÉTAPE 4 : Appliquer la migration ───────────────────────────────────────

echo ""
read -p "⚠️  Appliquer la migration sur la base de données ? (o/N) " -n 1 -r
echo
if [[ $REPLY =~ ^[Oo]$ ]]; then
    dotnet ef database update \
      --project "$INFRA" \
      --startup-project "$STARTUP"
    echo "✅ Migration appliquée."
else
    echo "⏭️  Migration non appliquée — lance manuellement avec :"
    echo "   dotnet ef database update --project $INFRA --startup-project $STARTUP"
fi

echo ""
echo "🎉 Refactoring Phase 1 intégré avec succès !"
echo ""
echo "📋 Prochaines étapes :"
echo "  1. Copier les fichiers de /tmp/GMS_Refactor/ vers ton projet"
echo "  2. Remplacer les entités héritant BaseEntity → TenantEntity"
echo "  3. Ajouter 'app.UseMiddleware<JwtClaimsMiddleware>()' dans Program.cs"
echo "  4. Tester : POST /api/ecoles/inscription"
echo "  5. Tester : POST /api/auth/login (vérifie ecoleId dans la réponse)"