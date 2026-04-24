namespace SchoolFlow.Domain.Entities;

/// <summary>
/// Agrégat Paiement avec logique FIFO dans le Domain.
/// </summary>
public class Paiement : TenantEntity
{
    public string NumeroPaiement { get; set; } = string.Empty;
    public Guid FamilleId { get; set; }
    public decimal MontantTotal { get; set; }
    public DateTime DatePaiement { get; set; } = DateTime.UtcNow;
    public ModePaiement ModePaiement { get; set; }
    public string? Reference { get; set; }
    public string? Commentaire { get; set; }
    public Guid EnregistrePar { get; set; }

    public bool IsVentilationComplete =>
        Ventilations.Any() &&
        Math.Abs(Ventilations.Sum(v => v.MontantVentile) - MontantTotal) < 0.01m;

    public int NombreElevesBeneficiaires =>
        Ventilations.Select(v => v.EleveId).Distinct().Count();

    // Navigation
    public Famille Famille { get; private set; } = null!;
    public Utilisateur EnregistreParUtilisateur { get; private set; } = null!;
    public ICollection<VentilationPaiement> Ventilations { get; private set; } = new List<VentilationPaiement>();

    // ─── FACTORY METHOD ──────────────────────────────────────────────────────

    public static Paiement Creer(
        Guid ecoleId,
        string numeroPaiement,
        Guid familleId,
        decimal montantTotal,
        DateTime datePaiement,
        ModePaiement modePaiement,
        Guid enregistrePar,
        string? reference = null,
        string? commentaire = null)
    {
        if (montantTotal <= 0)
            throw new ArgumentException("Le montant doit être positif.", nameof(montantTotal));

        return new Paiement
        {
            EcoleId = ecoleId,
            NumeroPaiement = numeroPaiement,
            FamilleId = familleId,
            MontantTotal = montantTotal,
            DatePaiement = datePaiement,
            ModePaiement = modePaiement,
            EnregistrePar = enregistrePar,
            Reference = reference,
            Commentaire = commentaire
        };
    }

    // ─── MÉTHODES MÉTIER ─────────────────────────────────────────────────────

    /// <summary>
    /// Applique une ventilation FIFO sur les frais d'un élève.
    /// Distribue le montant sur les frais impayés, du plus ancien au plus récent.
    /// </summary>
    public void AppliquerVentilation(
        Guid eleveId,
        decimal montant,
        IEnumerable<Frais> fraisOrdonnes)
    {
        if (montant <= 0)
            throw new ArgumentException("Le montant de ventilation doit être positif.", nameof(montant));

        var reste = montant;

        foreach (var frais in fraisOrdonnes)
        {
            if (reste <= 0.01m) break;
            if (frais.EstSolde) continue;

            var aImputer = Math.Min(reste, frais.Solde);
            frais.ImputerMontant(aImputer);

            Ventilations.Add(new VentilationPaiement
            {
                EcoleId = EcoleId,
                PaiementId = Id,
                EleveId = eleveId,
                FraisId = frais.Id,
                MontantVentile = aImputer
            });

            reste -= aImputer;
        }

        if (reste > 0.01m)
            throw new InvalidOperationException(
                $"Montant {reste:N0} FCFA non ventilé — frais insuffisants pour cet élève.");

        RaiseDomainEvent(new PaiementEnregistreEvent(
            PaiementId: Id,
            NumeroPaiement: NumeroPaiement,
            FamilleId: FamilleId,
            EcoleId: EcoleId,
            MontantTotal: MontantTotal,
            DatePaiement: DatePaiement,
            NombreVentilations: Ventilations.Count
        ));
    }
}

public enum ModePaiement
{
    Especes = 1,
    Virement = 2,
    MobileMoney = 3,
    Cheque = 4,
    Carte = 5
}
