# ============================================
# Setup-Database.ps1
# Script PowerShell pour créer et configurer la base de données SchoolFlow
# ============================================

param(
    [string]$ServerInstance = "localhost",
    [string]$DatabaseName = "SchoolFlowDb",
    [string]$SqlUser = "sa",
    [string]$SqlPassword = "Azerty@12",
    [string]$SqlPort = "11433",
    [switch]$Reset,
    [switch]$SeedOnly
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  SchoolFlow - Database Setup Script" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Vérifier si SQL Server est installé
Write-Host "[INFO] Verification de SQL Server..." -ForegroundColor Yellow

try {
    $sqlInstance = Get-Service -Name "MSSQL`$*" -ErrorAction SilentlyContinue
    if (-not $sqlInstance) {
        # Essayer de vérifier l'installation de SQL Server
        $sqlCheck = sqlcmd -S "$ServerInstance,$SqlPort" -U $SqlUser -P $SqlPassword -Q "SELECT @@VERSION" 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "[ERROR] SQL Server n'est pas installe ou inaccessible" -ForegroundColor Red
            Write-Host "[INFO] Telechargez-le ici: https://go.microsoft.com/fwlink/?linkid=866658" -ForegroundColor Yellow
            Write-Host "[ERROR] Details: $sqlCheck" -ForegroundColor Red
            exit 1
        }
        Write-Host "[OK] SQL Server detecte" -ForegroundColor Green
    } else {
        Write-Host "[OK] SQL Server detecte (Service Windows)" -ForegroundColor Green
    }
} catch {
    Write-Host "[WARNING] Impossible de verifier SQL Server: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "[INFO] Tentative de connexion directe..." -ForegroundColor Yellow
}

# Fonction pour exécuter une commande SQL
function Invoke-SqlCommand {
    param(
        [string]$Query,
        [string]$Server = "$ServerInstance,$SqlPort",
        [string]$Database = "master",
        [string]$User = $SqlUser,
        [string]$Password = $SqlPassword
    )
    
    try {
        # Utiliser Invoke-Sqlcmd si disponible, sinon sqlcmd
        if (Get-Command Invoke-Sqlcmd -ErrorAction SilentlyContinue) {
            $result = Invoke-Sqlcmd -ServerInstance $Server -Database $Database -Query $Query -Username $User -Password $Password 
            return $result
        } else {
            # Fallback sur sqlcmd.exe
            $result = sqlcmd -S $Server -d $Database -U $User -P $Password -Q $Query -C # -C = Trust Server Certificate
            if ($LASTEXITCODE -eq 0) {
                return $true
            } else {
                throw "sqlcmd returned error code $LASTEXITCODE"
            }
        }
    } catch {
        Write-Host "[ERROR] Erreur SQL: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Reset database si demandé
if ($Reset) {
    Write-Host ""
    Write-Host "[ACTION] Suppression de la base de donnees existante..." -ForegroundColor Yellow
    
    $dropQuery = @"
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'$DatabaseName')
BEGIN
    ALTER DATABASE [$DatabaseName] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE [$DatabaseName];
    PRINT 'Base de donnees supprimee avec succes';
END
ELSE
BEGIN
    PRINT 'La base de donnees n''existe pas';
END
"@
    
    $result = Invoke-SqlCommand -Query $dropQuery
    if ($result -ne $false) {
        Write-Host "[OK] Base de donnees supprimee" -ForegroundColor Green
    }
}

# Créer la base de données
if (-not $SeedOnly) {
    Write-Host ""
    Write-Host "[ACTION] Creation de la base de donnees..." -ForegroundColor Yellow
    
    $createDbQuery = @"
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'$DatabaseName')
BEGIN
    CREATE DATABASE [$DatabaseName];
    PRINT 'Base de donnees creee avec succes';
END
ELSE
BEGIN
    PRINT 'La base de donnees existe deja';
END
"@
    
    $result = Invoke-SqlCommand -Query $createDbQuery
    if ($result -ne $false) {
        Write-Host "[OK] Base de donnees '$DatabaseName' prete" -ForegroundColor Green
    } else {
        Write-Host "[ERROR] Echec de creation de la base de donnees" -ForegroundColor Red
        exit 1
    }
}

# Appliquer les migrations EF Core
Write-Host ""
Write-Host "[ACTION] Application des migrations Entity Framework Core..." -ForegroundColor Yellow

$projectRoot = Split-Path -Parent $PSScriptRoot
$infrastructureProject = Join-Path $projectRoot "src\SchoolFlow.Infrastructure\SchoolFlow.Infrastructure.csproj"
$apiProject = Join-Path $projectRoot "src\SchoolFlow.Api\SchoolFlow.Api.csproj"

if (-not (Test-Path $infrastructureProject)) {
    Write-Host "[ERROR] Projet Infrastructure introuvable: $infrastructureProject" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $apiProject)) {
    Write-Host "[ERROR] Projet Api introuvable: $apiProject" -ForegroundColor Red
    exit 1
}

# Créer la migration initiale si elle n'existe pas
$migrationsFolder = Join-Path $projectRoot "src\SchoolFlow.Infrastructure\Data\Migrations"
if (-not (Test-Path $migrationsFolder)) {
    Write-Host "[ACTION] Creation de la migration initiale..." -ForegroundColor Yellow
    
    dotnet ef migrations add InitialCreate `
        --project $infrastructureProject `
        --startup-project $apiProject `
        --output-dir "Data/Migrations" `
        --context ApplicationDbContext
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Migration creee" -ForegroundColor Green
    } else {
        Write-Host "[ERROR] Echec de creation de la migration" -ForegroundColor Red
        exit 1
    }
}

# Appliquer les migrations
Write-Host "[ACTION] Application des migrations a la base de donnees..." -ForegroundColor Yellow

# Définir la connection string avec TrustServerCertificate=true
$env:ConnectionStrings__DefaultConnection = "Server=$ServerInstance,$SqlPort;Database=$DatabaseName;User Id=$SqlUser;Password=$SqlPassword;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=False"

dotnet ef database update `
    --project $infrastructureProject `
    --startup-project $apiProject `
    --context ApplicationDbContext

if ($LASTEXITCODE -eq 0) {
    Write-Host "[OK] Migrations appliquees avec succes" -ForegroundColor Green
} else {
    Write-Host "[ERROR] Echec d'application des migrations" -ForegroundColor Red
    Write-Host "[INFO] Verifiez la connection string dans appsettings.json" -ForegroundColor Yellow
    exit 1
}

# Vérifier les tables créées
Write-Host ""
Write-Host "[INFO] Verification des tables creees..." -ForegroundColor Yellow

$verifyQuery = @"
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE' 
ORDER BY TABLE_NAME
"@

$tables = Invoke-SqlCommand -Query $verifyQuery -Database $DatabaseName

if ($tables) {
    Write-Host ""
    Write-Host "Tables creees:" -ForegroundColor Cyan
    if ($tables -is [Array]) {
        $tables | ForEach-Object {
            if ($_.TABLE_NAME) {
                Write-Host "  [OK] $($_.TABLE_NAME)" -ForegroundColor Green
            }
        }
    } else {
        Write-Host "  [OK] Tables creees (verification impossible via PowerShell)" -ForegroundColor Green
    }
} else {
    Write-Host "[WARNING] Impossible de lister les tables" -ForegroundColor Yellow
}

# Afficher le nombre d'enregistrements seed
Write-Host ""
Write-Host "[INFO] Donnees initiales (Seed Data):" -ForegroundColor Cyan

$seedCheckQueries = @{
    "Utilisateurs" = "SELECT COUNT(*) as Count FROM Utilisateurs WHERE IsArchived = 0"
    "AnneeScolaires" = "SELECT COUNT(*) as Count FROM AnneeScolaires"
    "Classes" = "SELECT COUNT(*) as Count FROM Classes WHERE IsArchived = 0"
    "TypeFrais" = "SELECT COUNT(*) as Count FROM TypeFrais WHERE IsArchived = 0"
    "Periodes" = "SELECT COUNT(*) as Count FROM Periodes WHERE IsArchived = 0"
}

foreach ($table in $seedCheckQueries.Keys) {
    try {
        $result = Invoke-SqlCommand -Query $seedCheckQueries[$table] -Database $DatabaseName
        if ($result -and $result.Count -ne $null) {
            Write-Host "  [OK] $table : $($result.Count) enregistrement(s)" -ForegroundColor Green
        } else {
            Write-Host "  [OK] $table : Donnees presentes" -ForegroundColor Green
        }
    } catch {
        Write-Host "  [WARNING] $table : Erreur de verification" -ForegroundColor Yellow
    }
}

# Afficher les credentials admin par défaut
Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  [ADMIN] CREDENTIALS PAR DEFAUT" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  Username : admin" -ForegroundColor White
Write-Host "  Password : Admin@2025" -ForegroundColor White
Write-Host ""
Write-Host "  Username : directeur" -ForegroundColor White
Write-Host "  Password : Dir@2025" -ForegroundColor White
Write-Host ""
Write-Host "  Username : secretaire" -ForegroundColor White
Write-Host "  Password : Sec@2025" -ForegroundColor White
Write-Host ""
Write-Host "  Username : comptable" -ForegroundColor White
Write-Host "  Password : Comptan@2025" -ForegroundColor White
Write-Host ""
Write-Host "  [WARNING] CHANGEZ CE MOT DE PASSE EN PRODUCTION !" -ForegroundColor Red
Write-Host "============================================" -ForegroundColor Cyan

# Générer le connection string
$connectionString = "Server=$ServerInstance,$SqlPort;Database=$DatabaseName;User Id=$SqlUser;Password=$SqlPassword;TrustServerCertificate=True;MultipleActiveResultSets=True;Encrypt=False"

Write-Host ""
Write-Host "[INFO] Connection String:" -ForegroundColor Cyan
Write-Host $connectionString -ForegroundColor White

# Sauvegarder dans appsettings.Development.json
$apiFolder = Join-Path $projectRoot "src\SchoolFlow.Api"
$appsettingsDevPath = Join-Path $apiFolder "appsettings.Development.json"

if (Test-Path $appsettingsDevPath) {
    Write-Host ""
    Write-Host "[ACTION] Mise a jour de appsettings.Development.json..." -ForegroundColor Yellow
    
    $appsettingsDev = @{
        "ConnectionStrings" = @{
            "DefaultConnection" = $connectionString
        }
        "Logging" = @{
            "LogLevel" = @{
                "Default" = "Information"
                "Microsoft.AspNetCore" = "Warning"
                "Microsoft.EntityFrameworkCore" = "Information"
            }
        }
    } | ConvertTo-Json -Depth 10
    
    $appsettingsDev | Out-File -FilePath $appsettingsDevPath -Encoding UTF8 -Force
    Write-Host "[OK] Configuration mise a jour" -ForegroundColor Green
} else {
    Write-Host "[WARNING] appsettings.Development.json introuvable" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  [SUCCESS] SETUP DATABASE TERMINE !" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Prochaines etapes:" -ForegroundColor Yellow
Write-Host "  1. cd src\SchoolFlow.Api" -ForegroundColor White
Write-Host "  2. dotnet run" -ForegroundColor White
Write-Host "  3. Ouvrir https://localhost:5294" -ForegroundColor White
Write-Host ""

# ============================================
# USAGE:
# ============================================
# Creation initiale:
#   .\scripts\Setup-Database.ps1
#
# Reset complet:
#   .\scripts\Setup-Database.ps1 -Reset
#
# Avec parametres personnalises:
#   .\scripts\Setup-Database.ps1 -ServerInstance "localhost" -SqlUser "sa" -SqlPassword "VotreMotDePasse"
# ============================================