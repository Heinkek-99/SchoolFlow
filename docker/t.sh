#!/bin/bash

# ============================================
# Script de test API SchoolFlow
# ============================================

API_URL="http://localhost:5294"
API_URL_DOCKER="http://localhost:8080"  # Si test depuis le conteneur

# Couleurs
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo "=========================================="
echo "🧪 TESTS API SCHOOLFLOW"
echo "=========================================="

# ============================================
# Test 1 : Health Check
# ============================================
echo -e "\n${YELLOW}[TEST 1]${NC} Health Check..."
HEALTH=$(curl -s -o /dev/null -w "%{http_code}" $API_URL/health)
if [ "$HEALTH" -eq 200 ]; then
    echo -e "${GREEN}✅ API is healthy (200)${NC}"
else
    echo -e "${RED}❌ API unhealthy (Code: $HEALTH)${NC}"
    exit 1
fi

# ============================================
# Test 2 : Login Admin
# ============================================
echo -e "\n${YELLOW}[TEST 2]${NC} Login Admin..."
LOGIN_RESPONSE=$(curl -s -X POST "$API_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin@2025"
  }')

TOKEN=$(echo $LOGIN_RESPONSE | jq -r '.data.token // empty')

if [ -z "$TOKEN" ] || [ "$TOKEN" == "null" ]; then
    echo -e "${RED}❌ Login failed${NC}"
    echo "Response: $LOGIN_RESPONSE"
    exit 1
else
    echo -e "${GREEN}✅ Login successful${NC}"
    echo "Token: ${TOKEN:0:50}..."
fi

# ============================================
# Test 3 : Créer une Famille
# ============================================
echo -e "\n${YELLOW}[TEST 3]${NC} Créer une famille..."
CREATE_FAMILLE=$(curl -s -X POST "$API_URL/api/familles" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "nomPere": "TRAORE",
    "prenomPere": "Amadou",
    "telephonePere": "+225 07 12 34 56 78",
    "emailPere": "amadou.traore@example.com",
    "nomMere": "KONE",
    "prenomMere": "Mariam",
    "telephoneMere": "+225 07 98 76 54 32",
    "telephonePrincipal": "+225 07 12 34 56 78",
    "adresse": "Cocody Angré 8ème Tranche",
    "ville": "Abidjan",
    "quartierCommune": "Cocody"
  }')

FAMILLE_ID=$(echo $CREATE_FAMILLE | jq -r '.data.id // empty')

if [ -z "$FAMILLE_ID" ] || [ "$FAMILLE_ID" == "null" ]; then
    echo -e "${RED}❌ Failed to create famille${NC}"
    echo "Response: $CREATE_FAMILLE"
else
    echo -e "${GREEN}✅ Famille created (ID: $FAMILLE_ID)${NC}"
fi

# ============================================
# Test 4 : Créer une Classe
# ============================================
echo -e "\n${YELLOW}[TEST 4]${NC} Créer une classe..."
CREATE_CLASSE=$(curl -s -X POST "$API_URL/api/classes" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "nom": "6ème A",
    "niveau": "Collège",
    "capaciteMax": 40,
    "description": "Classe de 6ème section A"
  }')

CLASSE_ID=$(echo $CREATE_CLASSE | jq -r '.data.id // empty')

if [ -z "$CLASSE_ID" ] || [ "$CLASSE_ID" == "null" ]; then
    echo -e "${RED}❌ Failed to create classe${NC}"
    echo "Response: $CREATE_CLASSE"
else
    echo -e "${GREEN}✅ Classe created (ID: $CLASSE_ID)${NC}"
fi

# ============================================
# Test 5 : Créer une Année Scolaire
# ============================================
echo -e "\n${YELLOW}[TEST 5]${NC} Créer année scolaire..."
CREATE_ANNEE=$(curl -s -X POST "$API_URL/api/anneesscolaires" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "libelle": "2024-2025",
    "dateDebut": "2024-09-01",
    "dateFin": "2025-06-30",
    "estActive": true
  }')

ANNEE_ID=$(echo $CREATE_ANNEE | jq -r '.data.id // empty')

if [ -z "$ANNEE_ID" ] || [ "$ANNEE_ID" == "null" ]; then
    echo -e "${RED}❌ Failed to create année scolaire${NC}"
    echo "Response: $CREATE_ANNEE"
else
    echo -e "${GREEN}✅ Année scolaire created (ID: $ANNEE_ID)${NC}"
fi

# ============================================
# Test 6 : Inscrire un Élève
# ============================================
if [ ! -z "$FAMILLE_ID" ] && [ ! -z "$CLASSE_ID" ] && [ ! -z "$ANNEE_ID" ]; then
    echo -e "\n${YELLOW}[TEST 6]${NC} Inscrire un élève..."
    CREATE_ELEVE=$(curl -s -X POST "$API_URL/api/eleves" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $TOKEN" \
      -d "{
        \"nom\": \"TRAORE\",
        \"prenom\": \"Sekou\",
        \"dateNaissance\": \"2012-05-15\",
        \"lieuNaissance\": \"Abidjan\",
        \"sexe\": \"Masculin\",
        \"nationalite\": \"Ivoirienne\",
        \"familleId\": \"$FAMILLE_ID\",
        \"classeId\": \"$CLASSE_ID\",
        \"anneeScolaireId\": \"$ANNEE_ID\",
        \"statut\": \"Actif\"
      }")

    ELEVE_ID=$(echo $CREATE_ELEVE | jq -r '.data.id // empty')

    if [ -z "$ELEVE_ID" ] || [ "$ELEVE_ID" == "null" ]; then
        echo -e "${RED}❌ Failed to create élève${NC}"
        echo "Response: $CREATE_ELEVE"
    else
        echo -e "${GREEN}✅ Élève created (ID: $ELEVE_ID)${NC}"
        echo "Matricule: $(echo $CREATE_ELEVE | jq -r '.data.matricule // empty')"
    fi
fi

# ============================================
# Test 7 : Créer un Type de Frais
# ============================================
echo -e "\n${YELLOW}[TEST 7]${NC} Créer type de frais..."
CREATE_TYPE_FRAIS=$(curl -s -X POST "$API_URL/api/typefrais" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "nom": "Scolarité",
    "description": "Frais de scolarité trimestrielle",
    "montantParDefaut": 150000,
    "estRecurrent": true
  }')

TYPE_FRAIS_ID=$(echo $CREATE_TYPE_FRAIS | jq -r '.data.id // empty')

if [ -z "$TYPE_FRAIS_ID" ] || [ "$TYPE_FRAIS_ID" == "null" ]; then
    echo -e "${RED}❌ Failed to create type frais${NC}"
else
    echo -e "${GREEN}✅ Type frais created (ID: $TYPE_FRAIS_ID)${NC}"
fi

# ============================================
# Test 8 : Ajouter des Frais à l'Élève
# ============================================
if [ ! -z "$ELEVE_ID" ] && [ ! -z "$TYPE_FRAIS_ID" ]; then
    echo -e "\n${YELLOW}[TEST 8]${NC} Ajouter frais élève..."
    CREATE_FRAIS=$(curl -s -X POST "$API_URL/api/frais" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $TOKEN" \
      -d "{
        \"eleveId\": \"$ELEVE_ID\",
        \"typeFraisId\": \"$TYPE_FRAIS_ID\",
        \"montant\": 150000,
        \"dateEcheance\": \"2025-01-31\",
        \"commentaire\": \"Frais de scolarité T1\"
      }")

    FRAIS_ID=$(echo $CREATE_FRAIS | jq -r '.data.id // empty')

    if [ -z "$FRAIS_ID" ] || [ "$FRAIS_ID" == "null" ]; then
        echo -e "${RED}❌ Failed to create frais${NC}"
    else
        echo -e "${GREEN}✅ Frais created (ID: $FRAIS_ID)${NC}"
    fi
fi

# ============================================
# Test 9 : Enregistrer un Paiement
# ============================================
if [ ! -z "$FAMILLE_ID" ] && [ ! -z "$FRAIS_ID" ]; then
    echo -e "\n${YELLOW}[TEST 9]${NC} Enregistrer paiement..."
    CREATE_PAIEMENT=$(curl -s -X POST "$API_URL/api/paiements" \
      -H "Content-Type: application/json" \
      -H "Authorization: Bearer $TOKEN" \
      -d "{
        \"familleId\": \"$FAMILLE_ID\",
        \"montant\": 50000,
        \"datePaiement\": \"2025-01-15\",
        \"modePaiement\": \"Especes\",
        \"reference\": \"PAY-001\",
        \"ventilations\": [
          {
            \"fraisId\": \"$FRAIS_ID\",
            \"montant\": 50000
          }
        ]
      }")

    PAIEMENT_ID=$(echo $CREATE_PAIEMENT | jq -r '.data.id // empty')

    if [ -z "$PAIEMENT_ID" ] || [ "$PAIEMENT_ID" == "null" ]; then
        echo -e "${RED}❌ Failed to create paiement${NC}"
        echo "Response: $CREATE_PAIEMENT"
    else
        echo -e "${GREEN}✅ Paiement created (ID: $PAIEMENT_ID)${NC}"
    fi
fi

# ============================================
# Test 10 : Dashboard Stats
# ============================================
echo -e "\n${YELLOW}[TEST 10]${NC} Dashboard stats..."
DASHBOARD=$(curl -s -X GET "$API_URL/api/dashboard/stats" \
  -H "Authorization: Bearer $TOKEN")

TOTAL_ELEVES=$(echo $DASHBOARD | jq -r '.data.totalEleves // 0')
echo -e "${GREEN}✅ Total élèves: $TOTAL_ELEVES${NC}"

# ============================================
# RÉSUMÉ
# ============================================
echo -e "\n=========================================="
echo -e "${GREEN}✅ TOUS LES TESTS TERMINÉS${NC}"
echo "=========================================="
echo "Famille ID: $FAMILLE_ID"
echo "Classe ID: $CLASSE_ID"
echo "Année ID: $ANNEE_ID"
echo "Élève ID: $ELEVE_ID"
echo "Type Frais ID: $TYPE_FRAIS_ID"
echo "Frais ID: $FRAIS_ID"
echo "Paiement ID: $PAIEMENT_ID"
echo "=========================================="