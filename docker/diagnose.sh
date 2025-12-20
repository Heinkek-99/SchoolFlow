#!/bin/bash

echo "🔍 DIAGNOSTIC DOCKER SCHOOLFLOW"
echo "================================"

echo ""
echo "1️⃣ État des conteneurs"
docker-compose ps

echo ""
echo "2️⃣ Healthcheck SQL Server"
docker inspect schoolflow-sqlserver --format='{{json .State.Health}}' | jq .

echo ""
echo "3️⃣ Test connexion SQL Server"
docker exec schoolflow-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P 'Azerty@12' \
  -Q "SELECT 'Connected!' AS Status, @@VERSION AS Version" 2>&1 | head -n 10

echo ""
echo "4️⃣ Bases de données existantes"
docker exec schoolflow-sqlserver /opt/mssql-tools/bin/sqlcmd \
  -S localhost \
  -U sa \
  -P 'Azerty@12' \
  -Q "SELECT name, database_id FROM sys.databases" 2>&1

echo ""
echo "5️⃣ Logs SQL Server (50 dernières lignes)"
docker logs schoolflow-sqlserver --tail 50 2>&1 | grep -E "error|fail|ready|healthy" -i

echo ""
echo "6️⃣ Logs API (50 dernières lignes)"
docker logs schoolflow-api --tail 50 2>&1 || echo "API non démarrée"

echo ""
echo "7️⃣ Test depuis l'hôte"
sqlcmd -S localhost,11433 -U sa -P 'Azerty@12' -Q "SELECT 1" 2>&1 || echo "Connexion échouée"

echo ""
echo "✅ Diagnostic terminé"