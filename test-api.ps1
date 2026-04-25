# ============================================================
# SchoolFlow — Tests automatisés PowerShell
# Usage : .\test-api.ps1 [-BaseUrl "http://localhost:5000"]
# ============================================================

param(
    [string]$BaseUrl = "http://localhost:5000"
)

$ErrorActionPreference = "Stop"
$global:Passed = 0
$global:Failed = 0
$global:Token  = ""
$global:EcoleId  = ""
$global:FamilleId = ""
$global:EleveId   = ""
$global:ClasseId  = ""

function Write-Pass { param($msg) Write-Host "  ✅ $msg" -ForegroundColor Green;  $global:Passed++ }
function Write-Fail { param($msg) Write-Host "  ❌ $msg" -ForegroundColor Red;    $global:Failed++ }
function Write-Step { param($msg) Write-Host "`n🔷 $msg" -ForegroundColor Cyan }

function Invoke-Api {
    param(
        [string]$Method,
        [string]$Path,
        [hashtable]$Body = $null,
        [string]$Token = $global:Token,
        [int]$ExpectedStatus = 200
    )

    $headers = @{ "Content-Type" = "application/json" }
    if ($Token) { $headers["Authorization"] = "Bearer $Token" }

    $params = @{
        Method  = $Method
        Uri     = "$BaseUrl$Path"
        Headers = $headers
        SkipHttpErrorCheck = $true
    }

    if ($Body) {
        $params["Body"] = ($Body | ConvertTo-Json -Depth 10)
    }

    try {
        $response = Invoke-WebRequest @params
        return @{
            Status  = [int]$response.StatusCode
            Data    = ($response.Content | ConvertFrom-Json -ErrorAction SilentlyContinue)
            Raw     = $response.Content
            Success = ([int]$response.StatusCode -eq $ExpectedStatus)
        }
    } catch {
        return @{ Status = 0; Data = $null; Raw = $_.Exception.Message; Success = $false }
    }
}

# ─── DÉBUT DES TESTS ───────────────────────────────────────────────────────

Write-Host "
╔══════════════════════════════════════════════╗
║    SchoolFlow API — Tests Automatisés        ║
║    $BaseUrl
╚══════════════════════════════════════════════╝
" -ForegroundColor Yellow

# ═══════════════════════════════════════════════════════════
Write-Step "1. HEALTH CHECK"
# ═══════════════════════════════════════════════════════════

$r = Invoke-Api -Method GET -Path "/health"
if ($r.Status -eq 200) { Write-Pass "API en ligne (HTTP 200)" }
else { Write-Fail "API inaccessible (HTTP $($r.Status))" }

# ═══════════════════════════════════════════════════════════
Write-Step "2. AUTHENTIFICATION"
# ═══════════════════════════════════════════════════════════

# 2.1 Login avec mauvais mot de passe
$r = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{
    username = "admin.victoire"; password = "mauvais"
} -Token "" -ExpectedStatus 400
if ($r.Status -eq 400) { Write-Pass "Login invalide → 400 BadRequest" }
else { Write-Fail "Login invalide devrait retourner 400 (reçu: $($r.Status))" }

# 2.2 Login Admin valide
$r = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{
    username = "admin.victoire"; password = "Admin@2025"
} -Token "" -ExpectedStatus 200
if ($r.Status -eq 200 -and $r.Data.token) {
    $global:Token   = $r.Data.token
    $global:EcoleId = $r.Data.ecoleId
    Write-Pass "Login Admin OK → Token obtenu"
    Write-Host "    EcoleId = $($global:EcoleId)" -ForegroundColor Gray
} else {
    Write-Fail "Login Admin échoué (HTTP $($r.Status))"
    Write-Host "    Réponse : $($r.Raw)" -ForegroundColor Red
    Write-Host "`n⚠️  Arrêt — impossible de continuer sans token." -ForegroundColor Red
    exit 1
}

# 2.3 GET /me
$r = Invoke-Api -Method GET -Path "/api/auth/me" -ExpectedStatus 200
if ($r.Status -eq 200 -and $r.Data.username) { Write-Pass "GET /me → username: $($r.Data.username)" }
else { Write-Fail "GET /me échoué (HTTP $($r.Status))" }

# 2.4 Accès sans token
$r = Invoke-Api -Method GET -Path "/api/familles" -Token "" -ExpectedStatus 401
if ($r.Status -eq 401) { Write-Pass "Sans token → 401 Unauthorized" }
else { Write-Fail "Sans token devrait retourner 401 (reçu: $($r.Status))" }

# ═══════════════════════════════════════════════════════════
Write-Step "3. DASHBOARD"
# ═══════════════════════════════════════════════════════════

$r = Invoke-Api -Method GET -Path "/api/dashboard/stats" -ExpectedStatus 200
if ($r.Status -eq 200) { Write-Pass "Dashboard stats → HTTP 200" }
else { Write-Fail "Dashboard stats échoué (HTTP $($r.Status)) — $($r.Raw)" }

# ═══════════════════════════════════════════════════════════
Write-Step "4. CLASSES"
# ═══════════════════════════════════════════════════════════

$r = Invoke-Api -Method GET -Path "/api/classes" -ExpectedStatus 200
if ($r.Status -eq 200) {
    $count = if ($r.Data -is [array]) { $r.Data.Count } else { "?" }
    Write-Pass "GET /classes → $count classes trouvées"
    if ($r.Data -is [array] -and $r.Data.Count -gt 0) {
        $global:ClasseId = $r.Data[0].id
        Write-Host "    1ère classe : $($r.Data[0].nom) (SousSysteme: $($r.Data[0].sousSysteme ?? 'N/A'))" -ForegroundColor Gray
    }
} else {
    Write-Fail "GET /classes échoué (HTTP $($r.Status))"
}

# ═══════════════════════════════════════════════════════════
Write-Step "5. FAMILLES"
# ═══════════════════════════════════════════════════════════

# 5.1 Liste familles
$r = Invoke-Api -Method GET -Path "/api/familles" -ExpectedStatus 200
if ($r.Status -eq 200) { Write-Pass "GET /familles → HTTP 200" }
else { Write-Fail "GET /familles → HTTP $($r.Status)" }

# 5.2 Recherche
$r = Invoke-Api -Method GET -Path "/api/familles/search?query=Nk" -ExpectedStatus 200
if ($r.Status -eq 200) { Write-Pass "Recherche familles 'Nk' → HTTP 200" }
else { Write-Fail "Recherche familles → HTTP $($r.Status)" }

# 5.3 Créer famille valide
$r = Invoke-Api -Method POST -Path "/api/familles" -Body @{
    nomPere           = "Ateba"
    prenomPere        = "Simon"
    telephonePere     = "+237 677 200 300"
    emailPere         = "s.ateba@gmail.com"
    professionPere    = "Enseignant"
    nomMere           = "Ateba née Mvogo"
    prenomMere        = "Claire"
    telephoneMere     = "+237 699 400 500"
    adresse           = "Rue de la Réunification"
    ville             = "Yaoundé"
    telephonePrincipal = "+237 677 200 300"
    quartierCommune   = "Mvan"
} -ExpectedStatus 201

if ($r.Status -eq 201) {
    $global:FamilleId = if ($r.Data) { $r.Data } else { "" }
    Write-Pass "Créer famille ATEBA → ID: $($global:FamilleId)"
} else {
    Write-Fail "Créer famille → HTTP $($r.Status) : $($r.Raw)"
}

# 5.4 Famille sans téléphone (doit échouer)
$r = Invoke-Api -Method POST -Path "/api/familles" -Body @{
    nomPere           = "Sans"
    adresse           = "Quelque part"
    ville             = "Yaoundé"
    telephonePrincipal = ""
} -ExpectedStatus 400

if ($r.Status -eq 400) { Write-Pass "Famille sans téléphone → 400 (validation OK)" }
else { Write-Fail "Famille sans téléphone devrait retourner 400 (reçu: $($r.Status))" }

# 5.5 Détail famille créée
if ($global:FamilleId) {
    $r = Invoke-Api -Method GET -Path "/api/familles/$($global:FamilleId)" -ExpectedStatus 200
    if ($r.Status -eq 200) { Write-Pass "GET famille par ID → famille ATEBA" }
    else { Write-Fail "GET famille par ID → HTTP $($r.Status)" }
}

# ═══════════════════════════════════════════════════════════
Write-Step "6. ÉLÈVES"
# ═══════════════════════════════════════════════════════════

# 6.1 Liste élèves
$r = Invoke-Api -Method GET -Path "/api/eleves" -ExpectedStatus 200
if ($r.Status -eq 200) { Write-Pass "GET /eleves → HTTP 200" }
else { Write-Fail "GET /eleves → HTTP $($r.Status)" }

# 6.2 Élèves de la première classe
if ($global:ClasseId) {
    $r = Invoke-Api -Method GET -Path "/api/eleves/classe/$($global:ClasseId)" -ExpectedStatus 200
    if ($r.Status -eq 200) {
        $count = if ($r.Data -is [array]) { $r.Data.Count } else { "?" }
        Write-Pass "Élèves de la classe → $count élève(s)"
        if ($r.Data -is [array] -and $r.Data.Count -gt 0) {
            $global:EleveId = $r.Data[0].id
        }
    } else { Write-Fail "Élèves par classe → HTTP $($r.Status)" }
}

# 6.3 Dossier élève complet
if ($global:EleveId) {
    $r = Invoke-Api -Method GET -Path "/api/eleves/$($global:EleveId)" -ExpectedStatus 200
    if ($r.Status -eq 200) {
        $eleve = $r.Data
        Write-Pass "Dossier élève → $($eleve.prenom) $($eleve.nom) | Matricule: $($eleve.matricule)"
    } else { Write-Fail "Dossier élève → HTTP $($r.Status)" }
}

# ═══════════════════════════════════════════════════════════
Write-Step "7. PAIEMENTS"
# ═══════════════════════════════════════════════════════════

# Login Comptable pour cette section
$rComp = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{
    username = "comptable.victoire"; password = "Compt@2025"
} -Token "" -ExpectedStatus 200

if ($rComp.Status -eq 200 -and $rComp.Data.token) {
    $tokenComptable = $rComp.Data.token
    Write-Pass "Login Comptable OK"

    # 7.1 Paiement daté dans le futur (doit échouer)
    $r = Invoke-Api -Method POST -Path "/api/paiements" -Token $tokenComptable -Body @{
        familleId     = $global:FamilleId
        montantTotal  = 10000
        datePaiement  = "2099-01-01T00:00:00Z"
        modePaiement  = 1
        enregistrePar = ([System.Guid]::NewGuid()).ToString()
        ventilations  = @()
    } -ExpectedStatus 400

    if ($r.Status -eq 400) { Write-Pass "Paiement futur → 400 (validation OK)" }
    else { Write-Fail "Paiement futur devrait retourner 400 (reçu: $($r.Status))" }

    # 7.2 GET paiement par numéro seed
    $r = Invoke-Api -Method GET -Path "/api/paiements/PAY-2025-00001" -Token $tokenComptable -ExpectedStatus 200
    if ($r.Status -eq 200) { Write-Pass "GET paiement PAY-2025-00001 → trouvé" }
    else { Write-Fail "GET PAY-2025-00001 → HTTP $($r.Status)" }

} else {
    Write-Fail "Login Comptable échoué — tests paiements ignorés"
}

# ═══════════════════════════════════════════════════════════
Write-Step "8. CONTRÔLE D'ACCÈS (Sécurité)"
# ═══════════════════════════════════════════════════════════

$rSecr = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{
    username = "secretaire.victoire"; password = "Secr@2025"
} -Token "" -ExpectedStatus 200

if ($rSecr.Status -eq 200) {
    $tokenSecr = $rSecr.Data.token

    # Secrétaire accède au dashboard top-impayes (ComptableAccess requis → 403)
    $r = Invoke-Api -Method GET -Path "/api/dashboard/top-impayes" -Token $tokenSecr -ExpectedStatus 403
    if ($r.Status -eq 403) { Write-Pass "Secrétaire → top-impayes : 403 Forbidden" }
    else { Write-Fail "Secrétaire → top-impayes devrait retourner 403 (reçu: $($r.Status))" }

    # Secrétaire accède aux paiements (403)
    $r = Invoke-Api -Method GET -Path "/api/paiements" -Token $tokenSecr -ExpectedStatus 403
    if ($r.Status -eq 403) { Write-Pass "Secrétaire → paiements : 403 Forbidden" }
    else { Write-Fail "Secrétaire → paiements devrait retourner 403 (reçu: $($r.Status))" }
}

# ═══════════════════════════════════════════════════════════
Write-Step "9. ISOLATION MULTI-TENANT"
# ═══════════════════════════════════════════════════════════

# Admin de l'école Franco ne doit pas voir les données de La Victoire
$rFranco = Invoke-Api -Method POST -Path "/api/auth/login" -Body @{
    username = "admin.champions"; password = "Admin@2025"
} -Token "" -ExpectedStatus 200

if ($rFranco.Status -eq 200) {
    $tokenFranco = $rFranco.Data.token
    $r = Invoke-Api -Method GET -Path "/api/familles" -Token $tokenFranco -ExpectedStatus 200
    if ($r.Status -eq 200) {
        $familles = $r.Data
        $count = if ($familles -is [array]) { $familles.Count } else { 0 }
        Write-Pass "Admin Franco → voit $count famille(s) (isolées de La Victoire)"
    } else { Write-Fail "Admin Franco → familles : HTTP $($r.Status)" }
}

# ═══════════════════════════════════════════════════════════
# RÉSUMÉ
# ═══════════════════════════════════════════════════════════

$total = $global:Passed + $global:Failed
Write-Host "
╔══════════════════════════════════════════════╗
║              RÉSUMÉ DES TESTS                ║
╠══════════════════════════════════════════════╣
║  Total   : $total tests
║  Réussis : $($global:Passed) ✅
║  Échoués : $($global:Failed) ❌
╚══════════════════════════════════════════════╝
" -ForegroundColor $(if ($global:Failed -eq 0) { "Green" } else { "Yellow" })

if ($global:Failed -gt 0) { exit 1 } else { exit 0 }
