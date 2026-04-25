# ============================================================
# SchoolFlow — Test direct Phase 7+
# Utilise curl.exe pour éviter les problèmes TLS de PS 5.1
# ============================================================

param(
    [string]$BaseUrl = "https://localhost:5294"
)

$pass = 0; $fail = 0
$CurlExe = "$env:SystemRoot\System32\curl.exe"
if (-not (Test-Path $CurlExe)) { $CurlExe = "curl" }

function Invoke-CurlRequest($method, $path, $token = "", $bodyObj = $null) {
    $uri = "$BaseUrl$path"
    $curlArgs = @("-s", "-k", "--max-time", "15", "-X", $method)
    if ($token) { $curlArgs += @("-H", "Authorization: Bearer $token") }

    $tmpIn = $null
    if ($bodyObj -ne $null) {
        $curlArgs += @("-H", "Content-Type: application/json")
        $tmpIn = [System.IO.Path]::GetTempFileName()
        $bodyObj | ConvertTo-Json -Depth 5 | Out-File -FilePath $tmpIn -Encoding utf8 -NoNewline
        $curlArgs += @("--data-binary", "@$tmpIn")
    }

    $tmpOut = [System.IO.Path]::GetTempFileName()
    $curlArgs += @("-o", $tmpOut, "-w", "%{http_code}", $uri)

    try {
        $statusStr = & $CurlExe @curlArgs 2>&1
        $statusCode = if ($statusStr -match "^\d{3}") { [int]$Matches[0] } else { 0 }
        $content = ""
        if (Test-Path $tmpOut) { $content = [System.IO.File]::ReadAllText($tmpOut, [System.Text.Encoding]::UTF8) }
        try { $data = $content | ConvertFrom-Json } catch { $data = $content }
        return @{ Status = $statusCode; Data = $data }
    } finally {
        if ($tmpIn)  { Remove-Item $tmpIn  -ErrorAction SilentlyContinue }
        if ($tmpOut) { Remove-Item $tmpOut -ErrorAction SilentlyContinue }
    }
}

function Post($path, $body, $token = "") { return Invoke-CurlRequest "POST" $path $token $body }
function Get($path, $token = "")         { return Invoke-CurlRequest "GET"  $path $token }

function OK($label, $cond, $info = "") {
    $fullMsg = if ($info) { "$label - $info" } else { $label }
    if ($cond) {
        Write-Host "  [PASS] $fullMsg" -ForegroundColor Green
        $script:pass++
    } else {
        Write-Host "  [FAIL] $fullMsg" -ForegroundColor Red
        $script:fail++
    }
}

function Section($title) { Write-Host "`n--- $title ---" -ForegroundColor Cyan }

# ============================================================
Section "TEST 1 - LOGIN ADMIN"
# ============================================================

$login = Post "/api/auth/login" @{ username = "admin.victoire"; password = "Admin@2025" }
OK "Login admin.victoire -> 200" ($login.Status -eq 200) "Status=$($login.Status)"

if ($login.Status -ne 200) {
    Write-Host "  Login echoue - impossible de continuer." -ForegroundColor Red
    Write-Host "  Reponse : $($login.Data | ConvertTo-Json)" -ForegroundColor DarkRed
    exit 1
}

$token   = $login.Data.token
$ecoleId = $login.Data.ecoleId
OK "EcoleId non vide" ($ecoleId -and $ecoleId -ne "00000000-0000-0000-0000-000000000000") "ecoleId=$ecoleId"

# ============================================================
Section "TEST 2 - CREER ANNEE SCOLAIRE"
# ============================================================

$anneeResult = Post "/api/annees-scolaires" @{
    libelle              = "2025-2026-test-$(Get-Random -Max 9999)"
    dateDebut            = "2025-09-01T00:00:00Z"
    dateFin              = "2026-07-31T00:00:00Z"
    typePeriode          = "Trimestre"
    activerImmediatement = $false
} -token $token

OK "POST /annees-scolaires -> 200/201" ($anneeResult.Status -in @(200, 201)) "Status=$($anneeResult.Status)"
if ($anneeResult.Status -notin @(200, 201)) {
    Write-Host "  Reponse : $($anneeResult.Data | ConvertTo-Json -Depth 3)" -ForegroundColor DarkRed
}

# Le controller retourne {"id": "guid"} — on extrait directement .id
$anneeId = $anneeResult.Data.id

# ============================================================
Section "TEST 3 - LISTER ANNEES"
# ============================================================

$annees = Get "/api/annees-scolaires" -token $token
OK "GET /annees-scolaires -> 200" ($annees.Status -eq 200) "Status=$($annees.Status)"
$dataAnnees = $annees.Data
$count = if ($dataAnnees -is [array]) { $dataAnnees.Count } else { 0 }
OK "Au moins 1 annee retournee" ($count -gt 0) "Count=$count"

# ============================================================
Section "TEST 4 - CREER CLASSE"
# ============================================================

if ($anneeId) {
    $classe = Post "/api/classes" @{
        code            = "CM2-TEST-$(Get-Random -Max 999)"
        nom             = "CM2 Test"
        niveau          = "CM2"
        sousSysteme     = "Francophone"
        section         = "T"
        capaciteMax     = 40
        anneeScolaireId = $anneeId
    } -token $token

    OK "POST /classes -> 200/201" ($classe.Status -in @(200, 201)) "Status=$($classe.Status)"
    if ($classe.Status -notin @(200, 201)) {
        Write-Host "  Reponse : $($classe.Data | ConvertTo-Json -Depth 3)" -ForegroundColor DarkRed
    }
} else {
    Write-Host "  [SKIP] Pas d'anneeId disponible" -ForegroundColor Yellow
}

# ============================================================
Section "TEST 5 - LISTER FAMILLES"
# ============================================================

$familles = Get "/api/familles" -token $token
OK "GET /familles -> 200" ($familles.Status -eq 200) "Status=$($familles.Status)"
if ($familles.Status -eq 200) {
    $dataFam = $familles.Data
    $count = if ($dataFam -is [array]) { $dataFam.Count } else { 0 }
    Write-Host "       Familles retournees : $count" -ForegroundColor DarkGray
}

# ============================================================
Section "TEST 6 - DASHBOARD"
# ============================================================

$dash = Get "/api/dashboard/stats" -token $token
OK "GET /dashboard/stats -> 200" ($dash.Status -eq 200) "Status=$($dash.Status)"
if ($dash.Status -eq 200) {
    $d = $dash.Data
    Write-Host "       TotalEleves=$($d.totalEleves) TotalFamilles=$($d.totalFamilles)" -ForegroundColor DarkGray
}

# ============================================================
Section "TEST 7 - ISOLATION TENANT (SuperAdmin)"
# ============================================================

$loginSA = Post "/api/auth/login" @{ username = "superadmin"; password = "SuperAdmin@2025" }
if ($loginSA.Status -eq 200) {
    $tokenSA = $loginSA.Data.token

    $r = Get "/api/familles" -token $tokenSA
    OK "SuperAdmin GET /familles -> 403" ($r.Status -eq 403) "Status=$($r.Status)"

    $r = Get "/api/dashboard/stats" -token $tokenSA
    OK "SuperAdmin GET /dashboard/stats -> 403" ($r.Status -eq 403) "Status=$($r.Status)"

    $r = Get "/api/ecoles" -token $tokenSA
    OK "SuperAdmin GET /ecoles -> 200" ($r.Status -eq 200) "Status=$($r.Status)"
} else {
    Write-Host "  [SKIP] Login SuperAdmin echoue" -ForegroundColor Yellow
}

# ============================================================
Section "TEST 8 - TYPES DE FRAIS"
# ============================================================

$tf = Get "/api/types-frais" -token $token
OK "GET /types-frais -> 200" ($tf.Status -eq 200) "Status=$($tf.Status)"
if ($tf.Status -eq 200) {
    $dataTf = $tf.Data
    $count = if ($dataTf -is [array]) { $dataTf.Count } else { 0 }
    Write-Host "       Types de frais : $count" -ForegroundColor DarkGray
}

# ============================================================
Write-Host "`n===============================================" -ForegroundColor Cyan
$color = if ($fail -gt 0) { "Red" } else { "Green" }
Write-Host "  RESULTAT : $pass PASS | $fail FAIL" -ForegroundColor $color
Write-Host "===============================================" -ForegroundColor Cyan

if ($fail -gt 0) { exit 1 }
