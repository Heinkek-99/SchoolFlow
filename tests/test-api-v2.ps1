# ============================================================
# SchoolFlow API — Test Script v2
# Usage : .\tests\test-api-v2.ps1
# Usage (prod) : .\tests\test-api-v2.ps1 -BaseUrl "https://schoolflow-8e86.onrender.com"
# ============================================================

param(
    [string]$BaseUrl = "http://localhost:5294"
)

$ErrorActionPreference = "Stop"

# ─── Helpers ─────────────────────────────────────────────────────────────────

$pass = 0; $fail = 0; $skip = 0

function Check([string]$label, [int]$got, [int]$expected, $body = $null){
    if ($got -eq $expected) {
        Write-Host "  [PASS] $label" -ForegroundColor Green
        $script:pass++
    } else {
        Write-Host "  [FAIL] $label — attendu $expected, reçu $got" -ForegroundColor Red
        if ($body) { Write-Host "         $($body | ConvertTo-Json -Compress)" -ForegroundColor DarkRed }
        $script:fail++
    }
}

function Invoke-Api([string]$method, [string]$path, $body = $null, [string]$token = "") {
    $headers = @{ "Content-Type" = "application/json" }
    if ($token) { $headers["Authorization"] = "Bearer $token" }
    $uri = "$BaseUrl$path"
    $params = @{ Method = $method; Uri = $uri; Headers = $headers; SkipHttpErrorCheck = $true }
    if ($body) { $params["Body"] = ($body | ConvertTo-Json -Depth 10) }
    return Invoke-WebRequest @params
}

function Login([string]$username, [string]$password) {
    $r = Invoke-Api "POST" "/api/auth/login" @{ username = $username; password = $password }
    if ($r.StatusCode -ne 200) {
        Write-Host "  [SKIP] Login $username échoué ($($r.StatusCode))" -ForegroundColor Yellow
        $script:skip++
        return $null
    }
    return ($r.Content | ConvertFrom-Json).token
}

# ─── Section Header ──────────────────────────────────────────────────────────

function Section([string]$title) {
    Write-Host ""
    Write-Host "══ $title ══" -ForegroundColor Cyan
}

# ============================================================
# 1. AUTH
# ============================================================
Section "1. AUTH — LOGIN"

$r = Invoke-Api "POST" "/api/auth/login" @{ username = "admin.victoire"; password = "Admin@2025" }
Check "Login admin.victoire → 200" $r.StatusCode 200 ($r.Content | ConvertFrom-Json)
$tokenAdmin = if ($r.StatusCode -eq 200) { ($r.Content | ConvertFrom-Json).token } else { $null }

$r = Invoke-Api "POST" "/api/auth/login" @{ username = "superadmin"; password = "SuperAdmin@2025" }
Check "Login superadmin → 200" $r.StatusCode 200
$tokenSA = if ($r.StatusCode -eq 200) { ($r.Content | ConvertFrom-Json).token } else { $null }

$r = Invoke-Api "POST" "/api/auth/login" @{ username = "comptable.victoire"; password = "Compt@2025" }
Check "Login comptable.victoire → 200" $r.StatusCode 200
$tokenComptable = if ($r.StatusCode -eq 200) { ($r.Content | ConvertFrom-Json).token } else { $null }

$r = Invoke-Api "POST" "/api/auth/login" @{ username = "bad"; password = "wrong" }
Check "Login mauvais credentials → 400/401" $r.StatusCode 401

# ============================================================
# 2. SUPERADMIN — ACCÈS REFUSÉ SUR ENDPOINTS MÉTIER (TenantAccess)
# ============================================================
Section "2. SUPERADMIN — TENANT ISOLATION"

if ($tokenSA) {
    $r = Invoke-Api "GET" "/api/familles" -token $tokenSA
    Check "SuperAdmin GET /familles → 403" $r.StatusCode 403

    $r = Invoke-Api "GET" "/api/eleves" -token $tokenSA
    Check "SuperAdmin GET /eleves → 403" $r.StatusCode 403

    $r = Invoke-Api "GET" "/api/classes" -token $tokenSA
    Check "SuperAdmin GET /classes → 403" $r.StatusCode 403

    $r = Invoke-Api "GET" "/api/types-frais" -token $tokenSA
    Check "SuperAdmin GET /types-frais → 403" $r.StatusCode 403

    $r = Invoke-Api "GET" "/api/dashboard/stats" -token $tokenSA
    Check "SuperAdmin GET /dashboard/stats → 403" $r.StatusCode 403

    # SuperAdmin peut lister les écoles
    $r = Invoke-Api "GET" "/api/ecoles" -token $tokenSA
    Check "SuperAdmin GET /ecoles → 200" $r.StatusCode 200

    $r = Invoke-Api "GET" "/api/ecoles/pending" -token $tokenSA
    Check "SuperAdmin GET /ecoles/pending → 200" $r.StatusCode 200
} else {
    Write-Host "  [SKIP] Token SuperAdmin non disponible" -ForegroundColor Yellow
    $script:skip += 7
}

# ============================================================
# 3. DASHBOARD
# ============================================================
Section "3. DASHBOARD"

if ($tokenAdmin) {
    $r = Invoke-Api "GET" "/api/dashboard/stats" -token $tokenAdmin
    Check "Admin GET /dashboard/stats → 200" $r.StatusCode 200
    if ($r.StatusCode -eq 200) {
        $stats = $r.Content | ConvertFrom-Json
        Write-Host "       Elèves actifs : $($stats.totalEleves)" -ForegroundColor DarkGray
        Write-Host "       Familles      : $($stats.totalFamilles)" -ForegroundColor DarkGray
    }

    $r = Invoke-Api "GET" "/api/dashboard/impayes" -token $tokenAdmin
    Check "Admin GET /dashboard/impayes → 200" $r.StatusCode 200
} else {
    $script:skip += 2
}

# ============================================================
# 4. CLASSES
# ============================================================
Section "4. CLASSES"

if ($tokenAdmin) {
    $r = Invoke-Api "GET" "/api/classes" -token $tokenAdmin
    Check "Admin GET /classes → 200" $r.StatusCode 200
    if ($r.StatusCode -eq 200) {
        $classes = $r.Content | ConvertFrom-Json
        Write-Host "       Classes trouvées : $($classes.Count)" -ForegroundColor DarkGray
    }

    # Créer une classe de test
    $anneeId = $null
    $rAnnees = Invoke-Api "GET" "/api/annees-scolaires" -token $tokenAdmin
    if ($rAnnees.StatusCode -eq 200) {
        $annees = $rAnnees.Content | ConvertFrom-Json
        $anneeId = if ($annees.Count -gt 0) { $annees[0].id } else { $null }
    }

    if ($anneeId) {
        $newClasse = @{
            code = "TEST-PS1-$(Get-Random -Max 999)"
            nom = "Classe Test Script"
            niveau = "CP1"
            sousSysteme = "Francophone"
            section = "T"
            capaciteMax = 30
            anneeScolaireId = $anneeId
        }
        $r = Invoke-Api "POST" "/api/classes" $newClasse -token $tokenAdmin
        Check "Admin POST /classes → 201" $r.StatusCode 201
        $classeId = if ($r.StatusCode -eq 201) { ($r.Content | ConvertFrom-Json) } else { $null }

        if ($classeId) {
            $r = Invoke-Api "DELETE" "/api/classes/$classeId" -token $tokenAdmin
            Check "Admin DELETE /classes/$classeId → 200" $r.StatusCode 200
        }
    } else {
        Write-Host "  [SKIP] Aucune année scolaire pour test Classe" -ForegroundColor Yellow
        $script:skip += 2
    }
} else {
    $script:skip += 4
}

# ============================================================
# 5. TYPES DE FRAIS
# ============================================================
Section "5. TYPES DE FRAIS"

if ($tokenAdmin) {
    $r = Invoke-Api "GET" "/api/types-frais" -token $tokenAdmin
    Check "Admin GET /types-frais → 200" $r.StatusCode 200
    if ($r.StatusCode -eq 200) {
        $tf = $r.Content | ConvertFrom-Json
        Write-Host "       Types de frais trouvés : $($tf.Count)" -ForegroundColor DarkGray
    }
} else {
    $script:skip++
}

# ============================================================
# 6. FAMILLES
# ============================================================
Section "6. FAMILLES"

if ($tokenAdmin) {
    $r = Invoke-Api "GET" "/api/familles" -token $tokenAdmin
    Check "Admin GET /familles → 200" $r.StatusCode 200

    # Créer une famille de test
    $newFamille = @{
        nomPere = "TestPS1"
        prenomPere = "Famille"
        telephonePere = "+237 699 999 001"
        telephonePrincipal = "+237 699 999 001"
        adresse = "Test Adresse"
        ville = "Yaoundé"
    }
    $r = Invoke-Api "POST" "/api/familles" $newFamille -token $tokenAdmin
    Check "Admin POST /familles → 201" $r.StatusCode 201
    $familleId = if ($r.StatusCode -eq 201) { ($r.Content | ConvertFrom-Json).familleId } else { $null }
} else {
    $script:skip += 2
}

# ============================================================
# 7. UTILISATEURS
# ============================================================
Section "7. UTILISATEURS"

if ($tokenAdmin) {
    $r = Invoke-Api "GET" "/api/utilisateurs" -token $tokenAdmin
    Check "Admin GET /utilisateurs → 200" $r.StatusCode 200
    if ($r.StatusCode -eq 200) {
        $users = $r.Content | ConvertFrom-Json
        Write-Host "       Utilisateurs trouvés : $($users.Count)" -ForegroundColor DarkGray
    }
} else {
    $script:skip++
}

# ============================================================
# 8. ANNÉES SCOLAIRES
# ============================================================
Section "8. ANNÉES SCOLAIRES"

if ($tokenAdmin) {
    $r = Invoke-Api "GET" "/api/annees-scolaires" -token $tokenAdmin
    Check "Admin GET /annees-scolaires → 200" $r.StatusCode 200
} else {
    $script:skip++
}

# ============================================================
# 9. PAIEMENTS
# ============================================================
Section "9. PAIEMENTS"

if ($tokenComptable) {
    $r = Invoke-Api "GET" "/api/paiements" -token $tokenComptable
    Check "Comptable GET /paiements → 200" $r.StatusCode 200
} else {
    $script:skip++
}

# ============================================================
# 10. ECOLES (SuperAdmin)
# ============================================================
Section "10. ÉCOLES — WORKFLOW SUPERADMIN"

if ($tokenSA) {
    # Créer une école de test
    $newEcole = @{
        nom = "École Test Script PS1"
        type = "Primaire"
        typeSecteur = "PriveLaic"
        sousSysteme = "Francophone"
        adresse = "123 Rue Test"
        ville = "Douala"
        pays = "Cameroun"
        telephonePrincipal = "+237 699 111 222"
        nomDirecteur = "Directeur"
        prenomDirecteur = "Test"
        nomAdmin = "Admin"
        prenomAdmin = "Test"
        usernameAdmin = "admin.testps1.$(Get-Random -Max 9999)"
        passwordAdmin = "TestAdmin@2025"
    }
    $r = Invoke-Api "POST" "/api/ecoles/inscription" $newEcole
    Check "POST /ecoles/inscription (public) → 201" $r.StatusCode 201
    $ecoleId = if ($r.StatusCode -eq 201) { ($r.Content | ConvertFrom-Json).ecoleId } else { $null }

    if ($ecoleId) {
        $r = Invoke-Api "PUT" "/api/ecoles/$ecoleId/valider" -token $tokenSA
        Check "SuperAdmin PUT /ecoles/{id}/valider → 200" $r.StatusCode 200

        $r = Invoke-Api "PUT" "/api/ecoles/$ecoleId/suspendre" @{ raison = "Test suspension script" } -token $tokenSA
        Check "SuperAdmin PUT /ecoles/{id}/suspendre → 200" $r.StatusCode 200
    } else {
        $script:skip += 2
    }

    $r = Invoke-Api "GET" "/api/ecoles" -token $tokenSA
    Check "SuperAdmin GET /ecoles → 200" $r.StatusCode 200
} else {
    $script:skip += 4
}

# ============================================================
# RÉSUMÉ
# ============================================================
Write-Host ""
Write-Host "══════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  RÉSULTATS : $pass PASS | $fail FAIL | $skip SKIP" -ForegroundColor $(if ($fail -gt 0) { "Red" } else { "Green" })
Write-Host "══════════════════════════════════════════" -ForegroundColor Cyan

if ($fail -gt 0) { exit 1 }
