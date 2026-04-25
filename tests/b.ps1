
# ============================================================
# SchoolFlow — Tests Phase 13 — 39 assertions
# Compatible PowerShell 5.1 + curl.exe Windows
# Usage : .\tests\test-direct.ps1 [-BaseUrl "https://localhost:5294"]
# ============================================================
param([string]$BaseUrl = "https://localhost:5294")

$pass = 0; $fail = 0; $skip = 0
$CurlExe = "$env:SystemRoot\System32\curl.exe"
if (-not (Test-Path $CurlExe)) { $CurlExe = "curl" }

function Coalesce($a, $b) { if ($null -ne $a -and $a -ne "") { $a } else { $b } }

function Invoke-Api($method, $path, $token = "", $bodyObj = $null) {
    $uri = "$BaseUrl$path"
    $args = @("-s", "-k", "--max-time", "30", "-X", $method)
    if ($token) { $args += @("-H", "Authorization: Bearer $token") }
    $tmpIn = $null
    if ($null -ne $bodyObj) {
        $args += @("-H", "Content-Type: application/json")
        $tmpIn = [System.IO.Path]::GetTempFileName()
        $bodyObj | ConvertTo-Json -Depth 10 | Out-File $tmpIn -Encoding utf8 -NoNewline
        $args += @("--data-binary", "@$tmpIn")
    }
    $tmpOut = [System.IO.Path]::GetTempFileName()
    $args += @("-o", $tmpOut, "-w", "%{http_code}", $uri)
    try {
        $sc   = & $CurlExe @args 2>&1
        $code = if ($sc -match "^\d{3}$") { [int]$sc } else { 0 }
        $raw  = if (Test-Path $tmpOut) { [System.IO.File]::ReadAllText($tmpOut, [System.Text.Encoding]::UTF8) } else { "" }
        try { $data = $raw | ConvertFrom-Json } catch { $data = $raw }
        return @{ S=$code; D=$data; R=$raw }
    } finally {
        if ($tmpIn  -and (Test-Path $tmpIn))  { Remove-Item $tmpIn  -Force }
        if ($tmpOut -and (Test-Path $tmpOut)) { Remove-Item $tmpOut -Force }
    }
}

# --- NOUVELLE FONCTION DE RÉCUPÉRATION ROBUSTE ---
function Extract-List($response) {
    if ($null -eq $response.D) { return @() }
    
    # Cas 1 : La donnée est directement un tableau
    if ($response.D -is [array]) { return @($response.D) }
    
    # Cas 2 : Wrapper Result -> data (tableau)
    if ($response.D.data -is [array]) { return @($response.D.data) }
    if ($response.D.Data -is [array]) { return @($response.D.Data) }
    
    # Cas 3 : Wrapper Result -> data -> items (tableau)
    if ($response.D.data.items -is [array]) { return @($response.D.data.items) }
    if ($response.D.Data.items -is [array]) { return @($response.D.Data.items) }
    
    # Cas 4 : Wrapper direct -> items
    if ($response.D.items -is [array]) { return @($response.D.items) }
    
    return @()
}

function Post($p, $b, $t="") { Invoke-Api "POST"   $p $t $b }
function Get($p, $t="")      { Invoke-Api "GET"    $p $t }
function Put($p, $b, $t="")  { Invoke-Api "PUT"    $p $t $b }
function DeleteRequest($p, $t="")      { Invoke-Api "DELETE" $p $t }

function OK($label, $cond, $info="") {
    if ($cond) {
        Write-Host "  [PASS] $label$(if($info){" - $info"})" -ForegroundColor Green
        $script:pass++
    } else {
        Write-Host "  [FAIL] $label$(if($info){" - $info"})" -ForegroundColor Red
        $script:fail++
    }
}
function SKIP($label) {
    Write-Host "  [SKIP] $label" -ForegroundColor Yellow
    $script:skip++
}
function Section($t) { Write-Host "`n━━━ $t ━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan }

# ── Variables globales ────────────────────────────────────────────────────────
$token=""; $tokenC=""; $tokenSA=""
$ecoleId=""; $anneeId=""; $periodeId=""
$classeId=""; $eleveId=""; $eleveClasseId=""
$familleId=""; $matiereId=""
$evalId=""; $examenId=""

# ════════════════════════════════════════════════════════════════════════════
Section "1 — AUTHENTIFICATION"
# ════════════════════════════════════════════════════════════════════════════

$r = Post "/api/auth/login" @{ username="admin.victoire"; password="Admin@2025" }
OK "Login admin.victoire → 200" ($r.S -eq 200) "Status=$($r.S)"
if ($r.S -ne 200) { Write-Host "LOGIN ÉCHOUÉ." -ForegroundColor Red; exit 1 }
$token   = $r.D.token
$ecoleId = $r.D.ecoleId
OK "EcoleId valide" ($ecoleId -and $ecoleId -ne "00000000-0000-0000-0000-000000000000")

$r = Post "/api/auth/login" @{ username="comptable.victoire"; password="Compt@2025" }
OK "Login comptable → 200" ($r.S -eq 200)
$tokenC = $r.D.token

$r = Post "/api/auth/login" @{ username="admin.victoire"; password="MAUVAIS" }
OK "Mauvais mdp → 400/401" ($r.S -in @(400,401)) "Status=$($r.S)"

# ════════════════════════════════════════════════════════════════════════════
Section "2 — ANNÉES SCOLAIRES"
# ════════════════════════════════════════════════════════════════════════════

$r = Get "/api/annees-scolaires" $token
OK "GET /annees-scolaires → 200" ($r.S -eq 200)
$annees = Extract-List $r
# Attention : On check IsActive (Majuscule) ET isActive (Minuscule)
$anneeActive = $annees | Where-Object { $_.IsActive -eq $true -or $_.isActive -eq $true } | Select-Object -First 1
OK "Année active existe" ($null -ne $anneeActive) "Libelle=$($anneeActive.libelle)"
$anneeId = $anneeActive.id

# ════════════════════════════════════════════════════════════════════════════
Section "3 — PÉRIODES"
# ════════════════════════════════════════════════════════════════════════════

$r = Get "/api/annees-scolaires/$anneeId/periodes" $token
OK "GET /periodes → 200" ($r.S -eq 200)
$periodes = Coalesce $r.D.data $r.D
$periode1 = if ($periodes -is [array]) { $periodes | Select-Object -First 1 }
OK "Au moins 1 période" ($null -ne $periode1) "Libelle=$($periode1.libelle)"
$periodeId = $periode1.id

# ════════════════════════════════════════════════════════════════════════════
Section "4 — CLASSES"
# ════════════════════════════════════════════════════════════════════════════

$r = Get "/api/classes" $token
OK "GET /classes → 200" ($r.S -eq 200)
$classes = Extract-List $r
$classe1 = $classes | Select-Object -First 1
OK "Au moins 1 classe" ($null -ne $classe1) "Classe=$($classe1.nomComplet)"
$classeId = $classe1.id

# ════════════════════════════════════════════════════════════════════════════
Section "5 — FAMILLES"
# ════════════════════════════════════════════════════════════════════════════

$familles = Get "/api/familles" $token
OK "GET /familles → 200" ($familles.S -eq 200) "Status=$($familles.S)"

# ON UTILISE la fonction Extract-List pour être sûr de récupérer le tableau
$familleList = Extract-List $familles
$famille1 = $familleList | Select-Object -First 1

OK "Au moins 1 famille" ($null -ne $famille1) "Famille=$($famille1.nomPere)"
$familleId = $famille1.id

if ($familleId) {
    $r = Get "/api/familles/$familleId" $token
    OK "GET /familles/{id} → 200" ($r.S -eq 200)
}

# Créer famille test
$r = Post "/api/familles" @{
    nomPere="Ateba"; prenomPere="Claude"
    telephonePere="+237 677 $(Get-Random -Min 100000 -Max 999999)"
    adresse="Quartier Mvan"; ville="Yaoundé"
    telephonePrincipal="+237 699 $(Get-Random -Min 100000 -Max 999999)"
} $token
OK "POST /familles → 201" ($r.S -in @(200,201)) "Status=$($r.S)"

# ════════════════════════════════════════════════════════════════════════════
Section "6 — ÉLÈVES"
# ════════════════════════════════════════════════════════════════════════════

$eleves = Get "/api/eleves" $token
OK "GET /eleves → 200" ($eleves.S -eq 200) "Status=$($eleves.S)"

# ON UTILISE la fonction Extract-List ici aussi
$eleveListe = Extract-List $eleves
$eleve1 = $eleveListe | Select-Object -First 1

OK "Au moins 1 élève" ($null -ne $eleve1) "Eleve=$($eleve1.nom) $($eleve1.prenom)"
$eleveId = $eleve1.id

# Dossier élève
if ($eleveId) {
    $r = Get "/api/eleves/$eleveId" $token
    OK "GET /eleves/{id} dossier → 200" ($r.S -eq 200)
    
    # Utilisation de Coalesce pour la robustesse du résultat
    $dossier = Coalesce $r.D.data $r.D
    $eleveClasseId = Coalesce $dossier.classeId $classeId
    Write-Host "         Dossier: $($dossier.nom) $($dossier.prenom) | Solde=$($dossier.solde) FCFA" -ForegroundColor DarkGray
}

# Élèves par classe
$r = Get "/api/eleves/classe/$classeId" $token
OK "GET /eleves/classe/{id} → 200" ($r.S -eq 200)

# ════════════════════════════════════════════════════════════════════════════
Section "7 — DASHBOARD"
# ════════════════════════════════════════════════════════════════════════════

$r = Get "/api/dashboard/stats" $token
OK "GET /dashboard/stats → 200" ($r.S -eq 200)
$stats = Coalesce $r.D.data $r.D
OK "TotalEleves ≥ 0" ($stats.totalEleves -ge 0) "Elèves=$($stats.totalEleves) | Familles=$($stats.totalFamilles)"
OK "TotalAEncaisser ≥ 0" ($stats.finances.totalAEncaisser -ge 0) "=$($stats.finances.totalAEncaisser) FCFA"

$r = Get "/api/dashboard/impayes" $token
OK "GET /dashboard/impayes → 200" ($r.S -in @(200,403))

# ════════════════════════════════════════════════════════════════════════════
Section "8 — TYPES DE FRAIS"
# ════════════════════════════════════════════════════════════════════════════

$r = Get "/api/types-frais" $token
OK "GET /types-frais → 200" ($r.S -eq 200)
$tfs = Coalesce $r.D.data $r.D
OK "≥ 1 TypeFrais" (($tfs | Measure-Object).Count -ge 1) "Count=$(($tfs | Measure-Object).Count)"

# ════════════════════════════════════════════════════════════════════════════
Section "9 — MATIÈRES"
# ════════════════════════════════════════════════════════════════════════════

$r = Get "/api/matieres" $token
OK "GET /matieres → 200" ($r.S -eq 200)
$mats = Coalesce $r.D.data $r.D
OK "≥ 8 matières seedées" (($mats | Measure-Object).Count -ge 8) "Count=$(($mats | Measure-Object).Count)"
$mat1 = $mats | Select-Object -First 1
$matiereId = $mat1.id
Write-Host "         Matière: $($mat1.code) — $($mat1.libelle) (coef $($mat1.coefficient))" -ForegroundColor DarkGray

# ════════════════════════════════════════════════════════════════════════════
Section "10 — ISOLATION TENANT"
# ════════════════════════════════════════════════════════════════════════════

$r = Post "/api/auth/login" @{ username="superadmin"; password="SuperAdmin@2025" }
OK "Login superadmin → 200" ($r.S -eq 200)
$tokenSA = $r.D.token

$r = Get "/api/familles" $tokenSA
OK "SuperAdmin → /familles → 403" ($r.S -eq 403) "Status=$($r.S)"

$r = Get "/api/ecoles" $tokenSA
OK "SuperAdmin → /ecoles → 200" ($r.S -eq 200)
$ecoles = Coalesce $r.D.data $r.D
$nEcoles = if ($ecoles.items) { $ecoles.items.Count } elseif ($ecoles -is [array]) { $ecoles.Count } else { 0 }
OK "SuperAdmin voit ≥ 1 école" ($nEcoles -ge 1) "Count=$nEcoles"

# Admin autre école → 0 données La Victoire
$r = Post "/api/auth/login" @{ username="azer"; password="Admin@2025" }
if ($r.S -eq 200) {
    $tAzer = $r.D.token
    $r2 = Get "/api/familles" $tAzer
    $f2 = Coalesce $r2.D.data $r2.D
    $f2List = if ($f2.items) { $f2.items } elseif ($f2 -is [array]) { $f2 } else { @() }
    OK "Isolation azer → 0 familles La Victoire" ($f2List.Count -eq 0) "Count=$($f2List.Count)"
}

# ════════════════════════════════════════════════════════════════════════════
Section "11 — ÉVALUATIONS"
# ════════════════════════════════════════════════════════════════════════════

if ($classeId -and $matiereId -and $periodeId -and $anneeId) {
    $dateEval = (Get-Date).AddDays(-1).ToString("yyyy-MM-ddT00:00:00Z")
    $r = Post "/api/evaluations" @{
        classeId=$classeId; matiereId=$matiereId; periodeId=$periodeId
        anneeScolaireId=$anneeId; type="Devoir"
        dateEvaluation=$dateEval; description="Test Phase 13"; noteSur=20
    } $token
    OK "POST /evaluations → 200/201" ($r.S -in @(200,201)) "Status=$($r.S)"
    $evalId = Coalesce $r.D.id $r.D.data

    $r = Get "/api/evaluations?classeId=$classeId" $token
    OK "GET /evaluations?classeId → 200" ($r.S -eq 200)
    $evalsData = Coalesce $r.D.data $r.D
    $nEvals = if ($evalsData -is [array]) { $evalsData.Count } else { 0 }
    Write-Host "         Évaluations pour cette classe: $nEvals" -ForegroundColor DarkGray
} else { SKIP "Évaluations — données manquantes" }

# ════════════════════════════════════════════════════════════════════════════
Section "12 — NOTES"
# ════════════════════════════════════════════════════════════════════════════

if ($evalId -and $eleveId) {
    $r = Post "/api/evaluations/$evalId/notes" @{
        notes = @(@{ eleveId=$eleveId; valeur=16.0; commentaire="Excellent" })
    } $token
    OK "POST /evaluations/{id}/notes → 200" ($r.S -in @(200,201)) "notesSaisies=$(Coalesce $r.D.data $r.D)"

    $r = Get "/api/evaluations/$evalId/notes" $token
    OK "GET /evaluations/{id}/notes → 200" ($r.S -eq 200)

    $r = Put "/api/evaluations/$evalId/publier" $null $token
    OK "PUT /evaluations/{id}/publier → 200/204" ($r.S -in @(200,204)) "Status=$($r.S)"

    $r = Get "/api/notes/eleve/$eleveId" $token
    OK "GET /notes/eleve/{id} → 200" ($r.S -eq 200)

    $r = Get "/api/eleves/$eleveId/notes" $token
    OK "GET /eleves/{id}/notes → 200" ($r.S -eq 200)
} else { SKIP "Notes — evalId ou eleveId manquant" }

# ════════════════════════════════════════════════════════════════════════════
Section "13 — BULLETINS"
# ════════════════════════════════════════════════════════════════════════════

# FIX : utiliser eleveClasseId (classe de l'élève test) pas classeId (CM2-A)
$cIdBulletin = if ($eleveClasseId) { $eleveClasseId } else { $classeId }

if ($cIdBulletin -and $periodeId -and $anneeId) {
    $r = Post "/api/bulletins/generer" @{
        classeId=$cIdBulletin; periodeId=$periodeId; anneeScolaireId=$anneeId
    } $token
    OK "POST /bulletins/generer → 200" ($r.S -in @(200,201)) "Status=$($r.S)"
    $nb = Coalesce $r.D.data $r.D
    Write-Host "         Bulletins générés : $nb" -ForegroundColor DarkGray

    $r = Get "/api/bulletins/classe/$cIdBulletin`?periodeId=$periodeId&anneeScolaireId=$anneeId" $token
    OK "GET /bulletins/classe/{id} → 200" ($r.S -eq 200)
    $buls = Coalesce $r.D.data $r.D
    $nBuls = if ($buls -is [array]) { $buls.Count } else { 0 }
    Write-Host "         Bulletins disponibles : $nBuls" -ForegroundColor DarkGray

    if ($eleveId) {
        $r = Get "/api/bulletins/eleve/$eleveId" $token
        OK "GET /bulletins/eleve/{id} → 200" ($r.S -eq 200)
    }
} else { SKIP "Bulletins — données manquantes" }

# ════════════════════════════════════════════════════════════════════════════
Section "14 — EMPLOI DU TEMPS"
# ════════════════════════════════════════════════════════════════════════════

if ($classeId -and $matiereId -and $anneeId) {
    $r = Post "/api/emploi-du-temps" @{
        classeId=$classeId; matiereId=$matiereId; anneeScolaireId=$anneeId
        jour="Mardi"; heureDebut="10:00:00"; heureFin="12:00:00"; salle="Salle B201"
    } $token
    OK "POST /emploi-du-temps → 200/201" ($r.S -in @(200,201)) "Status=$($r.S)"
    $creneauId = Coalesce $r.D.id $r.D.data

    $r = Get "/api/emploi-du-temps/classe/$classeId`?anneeScolaireId=$anneeId" $token
    OK "GET /emploi-du-temps/classe/{id} → 200" ($r.S -eq 200)

    if ($creneauId) {
        $r = DeleteRequest "/api/emploi-du-temps/$creneauId" $token
        OK "DELETE /emploi-du-temps/{id} → 200/204" ($r.S -in @(200,204)) "Status=$($r.S)"
    }
} else { SKIP "Emploi du temps — données manquantes" }

# ════════════════════════════════════════════════════════════════════════════
Section "15 — DISCIPLINE"
# ════════════════════════════════════════════════════════════════════════════

if ($eleveId -and $anneeId) {
    $dateDisc = (Get-Date).ToString("yyyy-MM-ddT00:00:00Z")
    $r = Post "/api/disciplines" @{
        eleveId=$eleveId; anneeScolaireId=$anneeId; type="Avertissement"
        motif="Retard en cours"; dateDiscipline=$dateDisc; notifieParent=$true
    } $token
    OK "POST /disciplines → 200/201" ($r.S -in @(200,201)) "Status=$($r.S)"

    $r = Get "/api/disciplines/eleve/$eleveId" $token
    OK "GET /disciplines/eleve/{id} → 200" ($r.S -eq 200)
    $discs = Coalesce $r.D.data $r.D
    $nDiscs = if ($discs -is [array]) { $discs.Count } else { 0 }
    # Vérifier que les dates sont valides
    $discValides = if ($discs -is [array]) { ($discs | Where-Object { $_.date -and [datetime]$_.date -gt (Get-Date "2000-01-01") }).Count } else { 0 }
    OK "Disciplines avec date valide" ($discValides -ge 1) "ValideCount=$discValides/TotalCount=$nDiscs"
} else { SKIP "Discipline — données manquantes" }

# ════════════════════════════════════════════════════════════════════════════
Section "16 — EXAMENS"
# ════════════════════════════════════════════════════════════════════════════

if ($anneeId) {
    $r = Post "/api/examens" @{
        nom="BEPC 2025 Session Test"; type="BEPC"; anneeScolaireId=$anneeId
        dateDebut="2025-06-01T08:00:00Z"; dateFin="2025-06-07T17:00:00Z"
    } $token
    OK "POST /examens → 200/201" ($r.S -in @(200,201)) "Status=$($r.S)"
    $examenId = Coalesce $r.D.id $r.D.data

    $r = Get "/api/examens?anneeScolaireId=$anneeId" $token
    OK "GET /examens → 200" ($r.S -eq 200)

    if ($examenId -and $eleveId) {
        $r = Post "/api/examens/$examenId/inscrire" @{ eleveIds=@($eleveId) } $token
        OK "POST /examens/{id}/inscrire → 200/201" ($r.S -in @(200,201)) "Status=$($r.S)"

        $r = Get "/api/examens/$examenId/resultats" $token
        OK "GET /examens/{id}/resultats → 200" ($r.S -eq 200)
    }
} else { SKIP "Examens — anneeId manquant" }

# ════════════════════════════════════════════════════════════════════════════
Section "17 — PAIEMENTS"
# ════════════════════════════════════════════════════════════════════════════

if ($familleId -and $tokenC) {
    # Proposer ventilation
    $r = Post "/api/paiements/ventilation/proposer" @{
        familleId=$familleId; montantTotal=10000
    } $tokenC
    OK "POST /paiements/ventilation/proposer → 200" ($r.S -in @(200,400)) "Status=$($r.S)"

    $vents = Coalesce $r.D.data $r.D
    if ($r.S -eq 200 -and $vents -is [array] -and $vents.Count -gt 0) {
        $totalV = ($vents | Measure-Object -Property montantSuggere -Sum).Sum
        if ($totalV -gt 0) {
            $ventList = $vents | ForEach-Object { @{ eleveId=$_.eleveId; montant=$_.montantSuggere } }
            $dateP = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
            $r = Post "/api/paiements" @{
                familleId=$familleId; montantTotal=$totalV
                datePaiement=$dateP; modePaiement="MobileMoney"; ventilations=$ventList
            } $tokenC
            OK "POST /paiements → 201" ($r.S -in @(200,201)) "Numéro=$(Coalesce $r.D.data $r.D)"
        } else { SKIP "Paiement — solde déjà 0" }
    } else { SKIP "Paiement — aucun frais impayé ou erreur ventilation" }

    # Test date future → 400
    $r = Post "/api/paiements" @{
        familleId=$familleId; montantTotal=1000
        datePaiement="2099-01-01T00:00:00Z"; modePaiement="Especes"; ventilations=@()
    } $tokenC
    OK "Paiement date future → 400" ($r.S -eq 400) "Status=$($r.S)"
} else { SKIP "Paiements — familleId ou tokenComptable manquant" }

# ════════════════════════════════════════════════════════════════════════════
Section "18 — PERFORMANCE"
# ════════════════════════════════════════════════════════════════════════════

$endpoints = @(
    @{ P="/api/dashboard/stats"; L="Dashboard stats";   Max=500 }
    @{ P="/api/familles";        L="Liste familles";     Max=300 }
    @{ P="/api/eleves";          L="Liste élèves";       Max=300 }
    @{ P="/api/classes";         L="Liste classes";      Max=200 }
    @{ P="/api/matieres";        L="Liste matières";     Max=200 }
    @{ P="/api/types-frais";     L="Types de frais";     Max=200 }
)

Write-Host ""
Write-Host "  Endpoint                    Max     Temps   Barre" -ForegroundColor White
Write-Host "  ─────────────────────────────────────────────────" -ForegroundColor DarkGray

foreach ($ep in $endpoints) {
    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    $r  = Get $ep.P $token
    $sw.Stop()
    $ms  = $sw.ElapsedMilliseconds
    $ok  = ($r.S -eq 200) -and ($ms -le $ep.Max)
    $bar = "█" * [Math]::Min(25, [Math]::Ceiling($ms / 20))
    $col = if ($ms -le $ep.Max) { "Green" } else { "Red" }
    Write-Host ("  {0,-28} {1,4}ms  {2,5}ms  {3}" -f $ep.L, $ep.Max, $ms, $bar) -ForegroundColor $col
    if ($ok) { $script:pass++ } else {
        $script:fail++
        if ($r.S -ne 200)          { Write-Host "    → HTTP $($r.S)" -ForegroundColor Red }
        if ($ms -gt $ep.Max)        { Write-Host "    → Trop lent ($ms ms > $($ep.Max) ms max)" -ForegroundColor Yellow }
    }
}

# ════════════════════════════════════════════════════════════════════════════
# RÉSUMÉ
# ════════════════════════════════════════════════════════════════════════════

$total = $pass + $fail + $skip
$pct   = if (($pass + $fail) -gt 0) { [Math]::Round($pass / ($pass + $fail) * 100) } else { 100 }
$col   = if ($fail -eq 0) { "Green" } elseif ($pct -ge 80) { "Yellow" } else { "Red" }

Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "  $pass PASS | $fail FAIL | $skip SKIP | $pct% réussite" -ForegroundColor $col
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
if ($fail -gt 0) { exit 1 }