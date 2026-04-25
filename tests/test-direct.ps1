# ============================================================
# SchoolFlow — Test direct Phase 12 v3 (Modules Académiques)
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
    $curlArgs = @("-s", "-k", "--max-time", "30", "-X", $method)
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

function Post($path, $body, $token = "") { return Invoke-CurlRequest "POST"   $path $token $body }
function Get($path, $token = "")         { return Invoke-CurlRequest "GET"    $path $token }
function Put($path, $body, $token = "")  { return Invoke-CurlRequest "PUT"    $path $token $body }
function DeleteRequest($path, $token = "")         { return Invoke-CurlRequest "DELETE" $path $token }

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
Section "§1 — LOGIN"
# ============================================================

$loginAdmin = Post "/api/auth/login" @{ username = "admin.victoire"; password = "Admin@2025" }
OK "Login admin.victoire -> 200" ($loginAdmin.Status -eq 200) "Status=$($loginAdmin.Status)"

if ($loginAdmin.Status -ne 200) {
    Write-Host "  Login échoué — impossible de continuer." -ForegroundColor Red
    exit 1
}

$token   = $loginAdmin.Data.token
$ecoleId = $loginAdmin.Data.ecoleId
OK "EcoleId non vide" ($ecoleId -and $ecoleId -ne "00000000-0000-0000-0000-000000000000") "ecoleId=$ecoleId"

$loginComptable = Post "/api/auth/login" @{ username = "comptable.victoire"; password = "Compt@2025" }
OK "Login comptable.victoire -> 200" ($loginComptable.Status -eq 200)
$tokenComptable = $loginComptable.Data.token

# ============================================================
Section "§2 — ANNEES SCOLAIRES"
# ============================================================

$annees = Get "/api/annees-scolaires" $token
OK "GET annees-scolaires -> 200" ($annees.Status -eq 200) "Status=$($annees.Status)"

$anneeActive = $annees.Data | Where-Object { $_.isActive -eq $true } | Select-Object -First 1
OK "Année active existe" ($anneeActive -ne $null) "Libelle=$($anneeActive.libelle)"
$anneeId = $anneeActive.id

# ============================================================
Section "§3 — PERIODES"
# ============================================================

$periodes = Get "/api/annees-scolaires/$anneeId/periodes" $token
OK "GET periodes -> 200" ($periodes.Status -eq 200) "Status=$($periodes.Status)"
$periode = $periodes.Data | Select-Object -First 1
OK "Au moins une période existe" ($periode -ne $null) "Libelle=$($periode.libelle)"
$periodeId = $periode.id

# ============================================================
Section "§4 — CLASSES"
# ============================================================

$classes = Get "/api/classes" $token
OK "GET classes -> 200" ($classes.Status -eq 200) "Status=$($classes.Status)"
$classe = $classes.Data | Select-Object -First 1
OK "Au moins une classe" ($classe -ne $null) "Classe=$($classe.nomComplet)"
$classeId = $classe.id

# ============================================================
Section "§5 — FAMILLES & ELEVES"
# ============================================================

$familles = Get "/api/familles" $token
OK "GET familles -> 200" ($familles.Status -eq 200) "Status=$($familles.Status)"
$famille = $familles.Data | Select-Object -First 1
OK "Au moins une famille" ($famille -ne $null) "Famille=$($famille.nomPere)"
$familleId = $famille.id

$elevesClasse = Get "/api/eleves/classe/$classeId" $token
OK "GET eleves/classe/{id} -> 200" ($elevesClasse.Status -eq 200) "Status=$($elevesClasse.Status)"
$eleve = $elevesClasse.Data | Select-Object -First 1
OK "Au moins un élève dans la classe" ($eleve -ne $null) "Eleve=$($eleve.nom)"
$eleveId = $eleve.id

# ============================================================
Section "§6 — MATIERES"
# ============================================================

$matieres = Get "/api/matieres" $token
OK "GET matieres -> 200" ($matieres.Status -eq 200) "Status=$($matieres.Status)"
OK "Matières seedées (>= 1)" ($matieres.Data.Count -ge 1) "Count=$($matieres.Data.Count)"
$matiere = $matieres.Data | Select-Object -First 1
$matiereId = $matiere.id

$newMatiere = Post "/api/matieres" @{
    code = "TZZ1"; libelle = "Matiere Test Temporaire"
    coefficient = 1; sousSysteme = "Francophone"
} $token
OK "POST matiere -> 200" ($newMatiere.Status -eq 200) "Status=$($newMatiere.Status)"

if ($newMatiere.Status -eq 200 -and $newMatiere.Data.id) {
    DeleteRequest "/api/matieres/$($newMatiere.Data.id)" $token | Out-Null
}

# ============================================================
Section "§7 — DASHBOARD"
# ============================================================

$stats = Get "/api/dashboard/stats" $token
OK "GET dashboard/stats -> 200" ($stats.Status -eq 200) "Status=$($stats.Status)"
OK "NombreElevesActifs >= 0" ($stats.Data.totalEleves -ge 0) "N=$($stats.Data.Eleves)"

# ============================================================
Section "§8 — TYPES FRAIS"
# ============================================================

$typesFrais = Get "/api/types-frais" $token
OK "GET types-frais -> 200" ($typesFrais.Status -eq 200) "Status=$($typesFrais.Status)"
OK "Au moins un TypeFrais" ($typesFrais.Data.Count -ge 1) "Count=$($typesFrais.Data.Count)"

# ============================================================
Section "§9 — TENANT ISOLATION"
# ============================================================

$loginSA = Post "/api/auth/login" @{ username = "superadmin"; password = "SuperAdmin@2025" }
OK "Login superadmin -> 200" ($loginSA.Status -eq 200)
$tokenSA = $loginSA.Data.token

$saFamilles = Get "/api/familles" $tokenSA
OK "SuperAdmin: GET /familles -> 403" ($saFamilles.Status -eq 403) "Status=$($saFamilles.Status)"

# ============================================================
Section "§10 — EVALUATIONS"
# ============================================================

$evalId = $null
if ($classeId -and $matiereId -and $periodeId -and $anneeId) {
    $dateEval = (Get-Date -Format "yyyy-MM-ddT00:00:00")
    $createEval = Post "/api/evaluations" @{
        classeId = $classeId; matiereId = $matiereId
        periodeId = $periodeId; anneeScolaireId = $anneeId
        type = "Devoir"; dateEvaluation = $dateEval
        description = "Test Phase 12"; noteSur = 20
    } $token
    OK "POST evaluation -> 200" ($createEval.Status -eq 200) "Status=$($createEval.Status)"
    $evalId = $createEval.Data.id

    $getEvals = Get "/api/evaluations?classeId=$classeId" $token
    OK "GET evaluations -> 200" ($getEvals.Status -eq 200) "Count=$($getEvals.Data.Count)"
} else {
    Write-Host "  [SKIP] Evaluation — données manquantes" -ForegroundColor Yellow
}

# ============================================================
Section "§11 — NOTES"
# ============================================================

$notesOk = $false
if ($evalId -and $eleveId) {
    $saisirNotes = Post "/api/evaluations/$evalId/notes" @{
        notes = @(
            @{ eleveId = $eleveId; valeur = 15.5; commentaire = "Très bien" }
        )
    } $token
    OK "POST saisir notes -> 200" ($saisirNotes.Status -eq 200) "notesSaisies=$($saisirNotes.Data.notesSaisies)"

    $publierEval = Put "/api/evaluations/$evalId/publier" $null $token
    OK "PUT evaluation publier -> 204" ($publierEval.Status -eq 204) "Status=$($publierEval.Status)"

    $notesViaController = Get "/api/notes/eleve/$eleveId" $token
    OK "GET /api/notes/eleve/{id} -> 200" ($notesViaController.Status -eq 200) "Status=$($notesViaController.Status)"

    $notesViaEleves = Get "/api/eleves/$eleveId/notes" $token
    OK "GET /api/eleves/{id}/notes -> 200" ($notesViaEleves.Status -eq 200) "Status=$($notesViaEleves.Status)"

    $notesOk = ($saisirNotes.Status -eq 200 -and $publierEval.Status -eq 204)
} else {
    Write-Host "  [SKIP] Notes — evalId ou eleveId manquant" -ForegroundColor Yellow
}

# ============================================================
Section "§12 — BULLETINS"
# ============================================================

if ($classeId -and $periodeId -and $anneeId) {
    $generer = Post "/api/bulletins/generer" @{
        classeId = $classeId; periodeId = $periodeId; anneeScolaireId = $anneeId
    } $token
    OK "POST bulletins/generer -> 200" ($generer.Status -eq 200) "bulletinsGeneres=$($generer.Data.bulletinsGeneres)"

    $bulletinsClasse = Get "/api/bulletins/classe/$($classeId)?periodeId=$periodeId&anneeScolaireId=$anneeId" $token
    OK "GET bulletins/classe -> 200" ($bulletinsClasse.Status -eq 200) "Status=$($bulletinsClasse.Status)"
} else {
    Write-Host "  [SKIP] Bulletins — données manquantes" -ForegroundColor Yellow
}

# ============================================================
Section "§13 — EMPLOI DU TEMPS"
# ============================================================

$creneauId = $null
if ($classeId -and $matiereId -and $anneeId) {
    $createCreneau = Post "/api/emploi-du-temps" @{
        classeId = $classeId; matiereId = $matiereId; anneeScolaireId = $anneeId
        jour = "Lundi"; heureDebut = "08:00:00"; heureFin = "10:00:00"
        salle = "Salle A101"
    } $token
    OK "POST emploi-du-temps -> 200" ($createCreneau.Status -eq 200) "Status=$($createCreneau.Status)"
    $creneauId = $createCreneau.Data.id

    $getEmploi = Get "/api/emploi-du-temps/classe/$($classeId)?anneeScolaireId=$anneeId" $token
    OK "GET emploi-du-temps/classe -> 200" ($getEmploi.Status -eq 200) "Status=$($getEmploi.Status)"

    if ($creneauId) {
        $del = DeleteRequest "/api/emploi-du-temps/$creneauId" $token
        OK "DELETE creneau -> 204" ($del.Status -eq 204) "Status=$($del.Status)"
    }
} else {
    Write-Host "  [SKIP] EmploiDuTemps — données manquantes" -ForegroundColor Yellow
}

# ============================================================
Section "§14 — DISCIPLINES"
# ============================================================

if ($eleveId -and $anneeId) {
    $dateDisc = (Get-Date -Format "yyyy-MM-ddT00:00:00")
    $createDisc = Post "/api/disciplines" @{
        eleveId = $eleveId; anneeScolaireId = $anneeId
        type = "Retenue"; motif = "Retard répété au cours de mathématiques"
        date = $dateDisc; notifieParent = $true
    } $token
    OK "POST discipline -> 200" ($createDisc.Status -eq 200) "Status=$($createDisc.Status)"

    $getDisciplines = Get "/api/disciplines/eleve/$eleveId" $token
    OK "GET disciplines/eleve -> 200" ($getDisciplines.Status -eq 200) "Count=$($getDisciplines.Data.Count)"
} else {
    Write-Host "  [SKIP] Disciplines — données manquantes" -ForegroundColor Yellow
}

# ============================================================
Section "§15 — EXAMENS"
# ============================================================

$examenId = $null
if ($anneeId) {
    $createExamen = Post "/api/examens" @{
        nom = "BEPC 2025 Session Principale"; type = "BEPC"
        anneeScolaireId = $anneeId
        dateDebut = "2025-06-10T08:00:00"; dateFin = "2025-06-14T17:00:00"
        centre = "Lycée Général Leclerc"
    } $token
    OK "POST examen -> 200" ($createExamen.Status -eq 200) "Status=$($createExamen.Status)"
    $examenId = $createExamen.Data.id

    $getExamens = Get "/api/examens?anneeScolaireId=$anneeId" $token
    OK "GET examens -> 200" ($getExamens.Status -eq 200) "Count=$($getExamens.Data.Count)"

    if ($examenId -and $eleveId) {
        $inscrire = Post "/api/examens/$examenId/inscrire" @{
            eleveIds = @($eleveId)
        } $token
        OK "POST examens/{id}/inscrire -> 200" ($inscrire.Status -eq 200) "inscrits=$($inscrire.Data.inscrits)"

        $resultats = Get "/api/examens/$examenId/resultats" $token
        OK "GET examens/{id}/resultats -> 200" ($resultats.Status -eq 200) "Status=$($resultats.Status)"
    }
} else {
    Write-Host "  [SKIP] Examens — anneeId manquant" -ForegroundColor Yellow
}

# ============================================================
Section "§16 — PAIEMENT FIFO"
# ============================================================

if ($familleId -and $eleveId -and $tokenComptable) {
    $ventilation = Post "/api/paiements/ventilation/proposer" @{
        familleId = $familleId; montant = 10000
    } $tokenComptable
    OK "POST ventilation/proposer -> 200" ($ventilation.Status -eq 200) "Status=$($ventilation.Status)"

    if ($ventilation.Status -eq 200 -and $ventilation.Data) {
        $vents = $ventilation.Data | Select-Object -First 2
        $ventList = @()
        foreach ($v in $vents) {
            $ventList += @{ eleveId = $v.eleveId; montant = $v.montant }
        }
        $totalVent = ($vents | Measure-Object -Property montant -Sum).Sum

        if ($ventList.Count -gt 0 -and $totalVent -gt 0) {
            $datePay = (Get-Date -Format "yyyy-MM-ddT00:00:00")
            $paiement = Post "/api/paiements" @{
                familleId = $familleId; montantTotal = $totalVent
                datePaiement = $datePay; modePaiement = "MobileMoney"
                ventilations = $ventList
            } $tokenComptable
            OK "POST paiement FIFO -> 201" ($paiement.Status -eq 201) "Status=$($paiement.Status)"
        } else {
            Write-Host "  [SKIP] Paiement — aucune ventilation disponible (tout déjà payé)" -ForegroundColor Yellow
        }
    }
} else {
    Write-Host "  [SKIP] Paiement — données ou token manquants" -ForegroundColor Yellow
}

# ============================================================
Section "§ SYNTHESE"
# ============================================================

$total = $pass + $fail
Write-Host ""
Write-Host "============================================" -ForegroundColor White
$color = if ($fail -eq 0) { "Green" } else { "Yellow" }
Write-Host "  RESULTATS : $pass PASS / $total TESTS" -ForegroundColor $color
Write-Host "============================================" -ForegroundColor White
if ($fail -gt 0) {
    Write-Host "  $fail test(s) ECHOUE(S)" -ForegroundColor Red
    exit 1
}
