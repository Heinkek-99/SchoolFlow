# ============================================================
# SchoolFlow -- Tests Phase 13 -- 18 sections
# Compatible PowerShell 5.1 + curl.exe Windows
# Usage : .\tests\test-direct.ps1 [-BaseUrl "https://localhost:5294"]
# ============================================================
param([string]$BaseUrl = "https://localhost:5294")

$pass = 0; $fail = 0; $skip = 0
$CurlExe = "$env:SystemRoot\System32\curl.exe"
if (-not (Test-Path $CurlExe)) { $CurlExe = "curl" }

function Invoke-Api($method, $path, $token = "", $bodyObj = $null) {
    $uri = "$BaseUrl$path"
    $curlArgs = @("-s", "-k", "--max-time", "30", "-X", $method)
    if ($token) { $curlArgs += @("-H", "Authorization: Bearer $token") }
    $tmpIn = $null
    if ($null -ne $bodyObj) {
        $curlArgs += @("-H", "Content-Type: application/json")
        $tmpIn = [System.IO.Path]::GetTempFileName()
        $bodyObj | ConvertTo-Json -Depth 10 | Out-File $tmpIn -Encoding utf8 -NoNewline
        $curlArgs += @("--data-binary", "@$tmpIn")
    }
    $tmpOut = [System.IO.Path]::GetTempFileName()
    $curlArgs += @("-o", $tmpOut, "-w", "%{http_code}", $uri)
    try {
        $sc   = & $CurlExe @curlArgs 2>&1
        $code = if ($sc -match "^\d{3}$") { [int]$sc } else { 0 }
        $raw  = if (Test-Path $tmpOut) { [System.IO.File]::ReadAllText($tmpOut, [System.Text.Encoding]::UTF8) } else { "" }
        try { $data = $raw | ConvertFrom-Json } catch { $data = $raw }
        return @{ S=$code; D=$data; R=$raw }
    } finally {
        if ($tmpIn  -and (Test-Path $tmpIn))  { Remove-Item $tmpIn  -Force }
        if ($tmpOut -and (Test-Path $tmpOut)) { Remove-Item $tmpOut -Force }
    }
}

function Post($p, $b, $t="") { Invoke-Api "POST"   $p $t $b }
function Get($p, $t="")      { Invoke-Api "GET"    $p $t }
function Put($p, $b, $t="")  { Invoke-Api "PUT"    $p $t $b }
function ApiDel($p, $t="")   { Invoke-Api "DELETE" $p $t }

function OK($label, $cond, $info="") {
    if ($cond) {
        $msg = if ($info) { "  [PASS] $label - $info" } else { "  [PASS] $label" }
        Write-Host $msg -ForegroundColor Green
        $script:pass++
    } else {
        $msg = if ($info) { "  [FAIL] $label - $info" } else { "  [FAIL] $label" }
        Write-Host $msg -ForegroundColor Red
        $script:fail++
    }
}
function SKIP($label) { Write-Host "  [SKIP] $label" -ForegroundColor Yellow; $script:skip++ }
function Section($t)  { Write-Host "`n--- $t ---" -ForegroundColor Cyan }

# -- Variables globales --
$token=""; $tokenC=""; $tokenSA=""
$ecoleId=""; $anneeId=""; $periodeId=""
$classeId=""; $eleveId=""; $eleveClasseId=""
$familleId=""; $matiereId=""
$evalId=$null; $examenId=$null

# ============================================================
Section "1 -- AUTHENTIFICATION"
# ============================================================

$r = Post "/api/auth/login" @{ username="admin.victoire"; password="Admin@2025" }
OK "Login admin.victoire -> 200" ($r.S -eq 200) "Status=$($r.S)"
if ($r.S -ne 200) { Write-Host "LOGIN ECHOUE." -ForegroundColor Red; exit 1 }
$token   = $r.D.token
$ecoleId = $r.D.ecoleId
OK "EcoleId valide" ($ecoleId -and $ecoleId -ne "00000000-0000-0000-0000-000000000000")

$r = Post "/api/auth/login" @{ username="comptable.victoire"; password="Compt@2025" }
OK "Login comptable -> 200" ($r.S -eq 200)
$tokenC = $r.D.token

$r = Post "/api/auth/login" @{ username="admin.victoire"; password="MAUVAIS" }
OK "Mauvais mdp -> 400/401" ($r.S -in @(400,401)) "Status=$($r.S)"

# ============================================================
Section "2 -- ANNEES SCOLAIRES"
# ============================================================

$r = Get "/api/annees-scolaires" $token
OK "GET annees-scolaires -> 200" ($r.S -eq 200)
# API returns array directly
$anneeActive = $r.D | Where-Object { $_.isActive -eq $true } | Select-Object -First 1
OK "Annee active existe" ($null -ne $anneeActive) "Libelle=$($anneeActive.libelle)"
$anneeId = $anneeActive.id

# ============================================================
Section "3 -- PERIODES"
# ============================================================

$r = Get "/api/annees-scolaires/$anneeId/periodes" $token
OK "GET periodes -> 200" ($r.S -eq 200)
$periode1 = $r.D | Select-Object -First 1
OK "Au moins 1 periode" ($null -ne $periode1) "Libelle=$($periode1.libelle)"
$periodeId = $periode1.id

# ============================================================
Section "4 -- CLASSES"
# ============================================================

$r = Get "/api/classes" $token
OK "GET classes -> 200" ($r.S -eq 200)
$classe1 = $r.D | Select-Object -First 1
OK "Au moins 1 classe" ($null -ne $classe1) "Classe=$($classe1.nomComplet)"
$classeId = $classe1.id

# ============================================================
Section "5 -- FAMILLES"
# ============================================================

$r = Get "/api/familles" $token
OK "GET familles -> 200" ($r.S -eq 200)
# Handler returns flat array (not paginated)
$famille1 = $r.D | Select-Object -First 1
OK "Au moins 1 famille" ($null -ne $famille1) "Famille=$($famille1.nomPere)"
$familleId = $famille1.id

if ($familleId) {
    $r = Get "/api/familles/$familleId" $token
    OK "GET familles/{id} -> 200" ($r.S -eq 200)
}

# ============================================================
Section "6 -- ELEVES"
# ============================================================

$r = Get "/api/eleves" $token
OK "GET eleves -> 200" ($r.S -eq 200)
$eleve1 = $r.D | Select-Object -First 1
OK "Au moins 1 eleve" ($null -ne $eleve1) "Eleve=$($eleve1.nom) $($eleve1.prenom)"
$eleveId = $eleve1.id

if ($eleveId) {
    $r2 = Get "/api/eleves/$eleveId" $token
    OK "GET eleves/{id} dossier -> 200" ($r2.S -eq 200)
    $dossier = $r2.D
    $eleveClasseId = if ($dossier.classeId) { $dossier.classeId } else { $classeId }
    Write-Host "         Dossier: $($dossier.nom) $($dossier.prenom) | ClasseId=$eleveClasseId" -ForegroundColor DarkGray
}

$r = Get "/api/eleves/classe/$classeId" $token
OK "GET eleves/classe/{id} -> 200" ($r.S -eq 200)

# ============================================================
Section "7 -- DASHBOARD"
# ============================================================

$r = Get "/api/dashboard/stats" $token
OK "GET dashboard/stats -> 200" ($r.S -eq 200)
OK "Eleves >= 0" ($r.D.totalEleves -ge 0 -or $r.D.nombreElevesActifs -ge 0)

# ============================================================
Section "8 -- TYPES FRAIS"
# ============================================================

$r = Get "/api/types-frais" $token
OK "GET types-frais -> 200" ($r.S -eq 200)
OK "Au moins 1 TypeFrais" (($r.D | Measure-Object).Count -ge 1) "Count=$(($r.D | Measure-Object).Count)"

# ============================================================
Section "9 -- MATIERES"
# ============================================================

$r = Get "/api/matieres" $token
OK "GET matieres -> 200" ($r.S -eq 200)
OK "Matieres seedees >= 1" (($r.D | Measure-Object).Count -ge 1) "Count=$(($r.D | Measure-Object).Count)"
$matiereId = ($r.D | Select-Object -First 1).id

$testCode = "TZ" + (Get-Random -Min 10000 -Max 99999)
$newMat = Post "/api/matieres" @{
    code=$testCode; libelle="Matiere Test Temporaire"
    coefficient=1; sousSysteme="Francophone"
} $token
OK "POST matiere -> 200" ($newMat.S -in @(200,201)) "Status=$($newMat.S)"
if ($newMat.S -in @(200,201) -and $newMat.D.id) {
    ApiDel "/api/matieres/$($newMat.D.id)" $token | Out-Null
}

# ============================================================
Section "10 -- ISOLATION TENANT"
# ============================================================

$r = Post "/api/auth/login" @{ username="superadmin"; password="SuperAdmin@2025" }
OK "Login superadmin -> 200" ($r.S -eq 200)
$tokenSA = $r.D.token

$r = Get "/api/familles" $tokenSA
OK "SuperAdmin: GET /familles -> 403" ($r.S -eq 403) "Status=$($r.S)"

$r = Get "/api/ecoles" $tokenSA
OK "SuperAdmin: GET /ecoles -> 200" ($r.S -eq 200) "Status=$($r.S)"

# ============================================================
Section "11 -- EVALUATIONS"
# ============================================================

if ($classeId -and $matiereId -and $periodeId -and $anneeId) {
    $dateEval = (Get-Date).AddDays(-1).ToString("yyyy-MM-ddT00:00:00Z")
    $r = Post "/api/evaluations" @{
        classeId=$classeId; matiereId=$matiereId; periodeId=$periodeId
        anneeScolaireId=$anneeId; type="Devoir"
        dateEvaluation=$dateEval; description="Test Phase 13"; noteSur=20
    } $token
    OK "POST evaluations -> 200" ($r.S -in @(200,201)) "Status=$($r.S)"
    $evalId = $r.D.id

    $r = Get "/api/evaluations?classeId=$classeId" $token
    OK "GET evaluations?classeId -> 200" ($r.S -eq 200) "Count=$($r.D.Count)"
} else { SKIP "Evaluations - donnees manquantes" }

# ============================================================
Section "12 -- NOTES"
# ============================================================

if ($evalId -and $eleveId) {
    $r = Post "/api/evaluations/$evalId/notes" @{
        notes = @(@{ eleveId=$eleveId; valeur=16.0; commentaire="Excellent" })
    } $token
    OK "POST saisir notes -> 200" ($r.S -in @(200,201)) "notesSaisies=$($r.D.notesSaisies)"

    $r = Put "/api/evaluations/$evalId/publier" $null $token
    OK "PUT evaluation publier -> 204" ($r.S -in @(200,204)) "Status=$($r.S)"

    $r = Get "/api/notes/eleve/$eleveId" $token
    OK "GET /api/notes/eleve/{id} -> 200" ($r.S -eq 200) "Status=$($r.S)"

    $r = Get "/api/eleves/$eleveId/notes" $token
    OK "GET /api/eleves/{id}/notes -> 200" ($r.S -eq 200) "Status=$($r.S)"
} else { SKIP "Notes - evalId ou eleveId manquant" }

# ============================================================
Section "13 -- BULLETINS"
# ============================================================

$cIdBulletin = if ($eleveClasseId) { $eleveClasseId } else { $classeId }

if ($cIdBulletin -and $periodeId -and $anneeId) {
    $r = Post "/api/bulletins/generer" @{
        classeId=$cIdBulletin; periodeId=$periodeId; anneeScolaireId=$anneeId
    } $token
    OK "POST bulletins/generer -> 200" ($r.S -in @(200,201)) "bulletinsGeneres=$($r.D.bulletinsGeneres)"

    $bulletinUrl = "/api/bulletins/classe/$($cIdBulletin)?periodeId=$periodeId&anneeScolaireId=$anneeId"
    $r = Get $bulletinUrl $token
    OK "GET bulletins/classe/{id} -> 200" ($r.S -eq 200) "Status=$($r.S)"

    if ($eleveId) {
        $r = Get "/api/bulletins/eleve/$eleveId" $token
        OK "GET bulletins/eleve/{id} -> 200" ($r.S -eq 200) "Status=$($r.S)"
    }
} else { SKIP "Bulletins - donnees manquantes" }

# ============================================================
Section "14 -- EMPLOI DU TEMPS"
# ============================================================

if ($classeId -and $matiereId -and $anneeId) {
    $r = Post "/api/emploi-du-temps" @{
        classeId=$classeId; matiereId=$matiereId; anneeScolaireId=$anneeId
        jour="Mardi"; heureDebut="10:00:00"; heureFin="12:00:00"; salle="Salle B201"
    } $token
    OK "POST emploi-du-temps -> 200" ($r.S -in @(200,201)) "Status=$($r.S)"
    $creneauId = $r.D.id

    $emploiUrl = "/api/emploi-du-temps/classe/$($classeId)?anneeScolaireId=$anneeId"
    $r = Get $emploiUrl $token
    OK "GET emploi-du-temps/classe/{id} -> 200" ($r.S -eq 200) "Status=$($r.S)"

    if ($creneauId) {
        $r = ApiDel "/api/emploi-du-temps/$creneauId" $token
        OK "DELETE creneau -> 204" ($r.S -in @(200,204)) "Status=$($r.S)"
    }
} else { SKIP "EmploiDuTemps - donnees manquantes" }

# ============================================================
Section "15 -- DISCIPLINES"
# ============================================================

if ($eleveId -and $anneeId) {
    $dateDisc = (Get-Date).ToString("yyyy-MM-ddT00:00:00Z")
    $r = Post "/api/disciplines" @{
        eleveId=$eleveId; anneeScolaireId=$anneeId; type="Avertissement"
        motif="Retard en cours"; dateDiscipline=$dateDisc; notifieParent=$true
    } $token
    OK "POST discipline -> 200" ($r.S -in @(200,201)) "Status=$($r.S)"

    $r = Get "/api/disciplines/eleve/$eleveId" $token
    OK "GET disciplines/eleve/{id} -> 200" ($r.S -eq 200) "Count=$($r.D.Count)"

    $cutoff2000 = Get-Date "2000-01-01"
    $discValides = if ($r.D -is [array]) {
        ($r.D | Where-Object { $_.date -and [datetime]$_.date -gt $cutoff2000 }).Count
    } else { 0 }
    OK "Disciplines avec date valide" ($discValides -ge 1) "Valides=$discValides"
} else { SKIP "Disciplines - donnees manquantes" }

# ============================================================
Section "16 -- EXAMENS"
# ============================================================

if ($anneeId) {
    $r = Post "/api/examens" @{
        nom="BEPC 2025 Session Test"; type="BEPC"; anneeScolaireId=$anneeId
        dateDebut="2025-06-01T08:00:00Z"; dateFin="2025-06-07T17:00:00Z"
    } $token
    OK "POST examen -> 200" ($r.S -in @(200,201)) "Status=$($r.S)"
    $examenId = $r.D.id

    $r = Get "/api/examens?anneeScolaireId=$anneeId" $token
    OK "GET examens -> 200" ($r.S -eq 200) "Count=$($r.D.Count)"

    if ($examenId -and $eleveId) {
        $r = Post "/api/examens/$examenId/inscrire" @{ eleveIds=@($eleveId) } $token
        OK "POST examens/{id}/inscrire -> 200" ($r.S -in @(200,201)) "inscrits=$($r.D.inscrits)"

        $r = Get "/api/examens/$examenId/resultats" $token
        OK "GET examens/{id}/resultats -> 200" ($r.S -eq 200) "Status=$($r.S)"
    }
} else { SKIP "Examens - anneeId manquant" }

# ============================================================
Section "17 -- PAIEMENT FIFO"
# ============================================================

if ($familleId -and $tokenC) {
    $r = Post "/api/paiements/ventilation/proposer" @{
        familleId=$familleId; montant=10000
    } $tokenC
    OK "POST ventilation/proposer -> 200/204" ($r.S -in @(200,204)) "Status=$($r.S)"

    if ($r.S -eq 200 -and $r.D -is [array] -and $r.D.Count -gt 0) {
        $vents2 = $r.D | Select-Object -First 2
        $ventList = @()
        foreach ($v in $vents2) {
            $ventList += @{ eleveId=$v.eleveId; montant=$v.montant }
        }
        $totalVent = ($vents2 | Measure-Object -Property montant -Sum).Sum
        if ($ventList.Count -gt 0 -and $totalVent -gt 0) {
            $datePay = (Get-Date).ToString("yyyy-MM-ddT00:00:00Z")
            $r = Post "/api/paiements" @{
                familleId=$familleId; montantTotal=$totalVent
                datePaiement=$datePay; modePaiement="MobileMoney"
                ventilations=$ventList
            } $tokenC
            OK "POST paiement FIFO -> 201" ($r.S -in @(200,201)) "Status=$($r.S)"
        } else { SKIP "Paiement - solde deja 0" }
    } else { SKIP "Paiement - aucun frais impaye" }
} else { SKIP "Paiements - familleId ou tokenComptable manquant" }

# ============================================================
Section "18 -- PERFORMANCE"
# ============================================================

$endpoints = @(
    @{ P="/api/dashboard/stats"; L="Dashboard stats";  Max=2000 }
    @{ P="/api/familles";        L="Liste familles";    Max=2000 }
    @{ P="/api/eleves";          L="Liste eleves";      Max=2000 }
    @{ P="/api/classes";         L="Liste classes";     Max=1000 }
    @{ P="/api/matieres";        L="Liste matieres";    Max=1000 }
    @{ P="/api/types-frais";     L="Types de frais";    Max=1000 }
)

Write-Host ""
Write-Host "  Endpoint                    Max      Temps" -ForegroundColor White
Write-Host "  -------------------------------------------" -ForegroundColor DarkGray

foreach ($ep in $endpoints) {
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $r  = Get $ep.P $token
    $sw.Stop()
    $ms  = $sw.ElapsedMilliseconds
    $ok  = ($r.S -eq 200) -and ($ms -le $ep.Max)
    $col = if ($ok) { "Green" } else { "Red" }
    $line = "  {0,-28} {1,5}ms  {2,5}ms" -f $ep.L, $ep.Max, $ms
    Write-Host $line -ForegroundColor $col
    if ($ok) { $script:pass++ } else {
        $script:fail++
        if ($r.S -ne 200) { Write-Host "    -> HTTP $($r.S)" -ForegroundColor Red }
        if ($ms -gt $ep.Max) { Write-Host "    -> Trop lent ($($ms)ms > $($ep.Max)ms)" -ForegroundColor Yellow }
    }
}

# ============================================================
# SYNTHESE
# ============================================================

$total = $pass + $fail + $skip
$pct   = if (($pass + $fail) -gt 0) { [Math]::Round($pass / ($pass + $fail) * 100) } else { 100 }
$col   = if ($fail -eq 0) { "Green" } elseif ($pct -ge 80) { "Yellow" } else { "Red" }

Write-Host ""
Write-Host "============================================" -ForegroundColor White
Write-Host "  $pass PASS | $fail FAIL | $skip SKIP | $pct% reussite" -ForegroundColor $col
Write-Host "============================================" -ForegroundColor White
if ($fail -gt 0) { exit 1 }
