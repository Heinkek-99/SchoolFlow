using SchoolFlow.Desktop.Core;
using SchoolFlow.Desktop.Services.Interfaces;
using SchoolFlow.Shared.Dtos;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SchoolFlow.Desktop.ViewModels;

/// <summary>
/// ViewModel pour la gestion des paiements avec ventilation.
/// </summary>
public class PaiementsViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly ICacheService _cacheService;
    private readonly INavigationService _navigationService;

    private ObservableCollection<PaiementDto> _paiements = new();
    private ObservableCollection<FamilleDto> _familles = new();
    private PaiementDetailDto? _paiementDetail;
    private PaiementDto? _selectedPaiement;
    private FamilleDto? _selectedFamille;
    
    // Champs pour nouveau paiement
    private decimal _montantPaiement;
    private string _modePaiement = "Espèces";
    private string? _reference;
    private ObservableCollection<VentillationDto> _ventilations = new();
    private bool _isNouveauPaiementVisible;
    private bool _isDetailVisible;

    public PaiementsViewModel(
        IApiService apiService,
        ICacheService cacheService,
        INavigationService navigationService)
    {
        _apiService = apiService;
        _cacheService = cacheService;
        _navigationService = navigationService;

        // Commandes
        LoadPaiementsCommand = new AsyncRelayCommand(_ => LoadPaiementsAsync());
        LoadFamillesCommand = new AsyncRelayCommand(_ => LoadFamillesAsync());
        NouveauPaiementCommand = new RelayCommand(_ => ShowNouveauPaiement());
        EnregistrerPaiementCommand = new AsyncRelayCommand(_ => EnregistrerPaiementAsync(), _ => CanEnregistrerPaiement());
        AnnulerPaiementCommand = new RelayCommand(_ => AnnulerNouveauPaiement());
        ViewDetailCommand = new AsyncRelayCommand(_ => ViewDetailAsync(), _ => SelectedPaiement != null);
        CloseDetailCommand = new RelayCommand(_ => CloseDetail());
        ImprimerRecuCommand = new RelayCommand(_ => ImprimerRecu(), _ => SelectedPaiement != null);
        VentilationAutomatiqueCommand = new AsyncRelayCommand(_ => VentilationAutomatiqueAsync(), _ => SelectedFamille != null && MontantPaiement > 0);
    }

    #region Propriétés

    public ObservableCollection<PaiementDto> Paiements
    {
        get => _paiements;
        set => SetProperty(ref _paiements, value);
    }

    public ObservableCollection<FamilleDto> Familles
    {
        get => _familles;
        set => SetProperty(ref _familles, value);
    }

    public ObservableCollection<VentillationDto> Ventilations
    {
        get => _ventilations;
        set => SetProperty(ref _ventilations, value);
    }

    public PaiementDto? SelectedPaiement
    {
        get => _selectedPaiement;
        set
        {
            SetProperty(ref _selectedPaiement, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public PaiementDetailDto? PaiementDetail
    {
        get => _paiementDetail;
        set => SetProperty(ref _paiementDetail, value);
    }

    public FamilleDto? SelectedFamille
    {
        get => _selectedFamille;
        set
        {
            SetProperty(ref _selectedFamille, value);
            if (value != null)
            {
                _ = LoadEnfantsFamilleAsync(value.Id);
            }
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public decimal MontantPaiement
    {
        get => _montantPaiement;
        set
        {
            SetProperty(ref _montantPaiement, value);
            CommandManager.InvalidateRequerySuggested();
            OnPropertyChanged(nameof(MontantRestantAVentiler));
        }
    }

    public string ModePaiement
    {
        get => _modePaiement;
        set => SetProperty(ref _modePaiement, value);
    }

    public string? Reference
    {
        get => _reference;
        set => SetProperty(ref _reference, value);
    }

    public bool IsNouveauPaiementVisible
    {
        get => _isNouveauPaiementVisible;
        set => SetProperty(ref _isNouveauPaiementVisible, value);
    }

    public bool IsDetailVisible
    {
        get => _isDetailVisible;
        set => SetProperty(ref _isDetailVisible, value);
    }

    // Modes de paiement disponibles
    public List<string> ModesPaiement { get; } = new()
    {
        "Espèces",
        "Chèque",
        "Virement",
        "Mobile Money",
        "Orange Money",
        "MTN Money"
    };

    // Calcul du montant restant à ventiler
    public decimal MontantRestantAVentiler => MontantPaiement - Ventilations.Sum(v => v.Montant);
    public string MontantRestantFormatted => $"{MontantRestantAVentiler:N0} FCFA";
    public bool VentilationValide => MontantRestantAVentiler == 0 && Ventilations.Count > 0;

    #endregion

    #region Commandes

    public ICommand LoadPaiementsCommand { get; }
    public ICommand LoadFamillesCommand { get; }
    public ICommand NouveauPaiementCommand { get; }
    public ICommand EnregistrerPaiementCommand { get; }
    public ICommand AnnulerPaiementCommand { get; }
    public ICommand ViewDetailCommand { get; }
    public ICommand CloseDetailCommand { get; }
    public ICommand ImprimerRecuCommand { get; }
    public ICommand VentilationAutomatiqueCommand { get; }

    #endregion

    #region Méthodes

    public override async Task OnNavigatedToAsync()
    {
        await Task.WhenAll(LoadPaiementsAsync(), LoadFamillesAsync());
    }

    private async Task LoadPaiementsAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();

            var paiements = await _apiService.GetAsync<List<PaiementDto>>("api/Paiements");
            
            Paiements.Clear();
            if (paiements != null)
            {
                foreach (var paiement in paiements.OrderByDescending(p => p.DatePaiement))
                {
                    Paiements.Add(paiement);
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du chargement des paiements : {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadFamillesAsync()
    {
        try
        {
            var familles = await _cacheService.GetOrAddAsync(
                "familles_list",
                () => _apiService.GetAsync<List<FamilleDto>>("api/Familles"),
                TimeSpan.FromMinutes(5)
            );

            Familles.Clear();
            if (familles != null)
            {
                foreach (var famille in familles)
                {
                    Familles.Add(famille);
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du chargement des familles : {ex.Message}");
        }
    }

    private async Task LoadEnfantsFamilleAsync(Guid familleId)
    {
        try
        {
            var familleDetail = await _apiService.GetAsync<FamilleDetailDto>($"api/Familles/{familleId}");
            
            Ventilations.Clear();
            if (familleDetail?.Enfants != null)
            {
                foreach (var enfant in familleDetail.Enfants)
                {
                    Ventilations.Add(new VentillationDto
                    {
                        EleveId = enfant.Id,
                        NomEleve = $"{enfant.Prenom} {enfant.Nom}",
                        Montant = 0
                    });
                }
            }

            OnPropertyChanged(nameof(MontantRestantAVentiler));
            OnPropertyChanged(nameof(MontantRestantFormatted));
            OnPropertyChanged(nameof(VentilationValide));
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du chargement des enfants : {ex.Message}");
        }
    }

    private void ShowNouveauPaiement()
    {
        ResetNouveauPaiement();
        IsNouveauPaiementVisible = true;
    }

    private void AnnulerNouveauPaiement()
    {
        ResetNouveauPaiement();
        IsNouveauPaiementVisible = false;
    }

    private void ResetNouveauPaiement()
    {
        SelectedFamille = null;
        MontantPaiement = 0;
        ModePaiement = "Espèces";
        Reference = null;
        Ventilations.Clear();
    }

    private bool CanEnregistrerPaiement()
    {
        return SelectedFamille != null && 
               MontantPaiement > 0 && 
               VentilationValide && 
               !IsLoading;
    }

    private async Task EnregistrerPaiementAsync()
    {
        if (!CanEnregistrerPaiement()) return;

        try
        {
            IsLoading = true;
            ClearError();

            var command = new
            {
                FamilleId = SelectedFamille!.Id,
                Montant = MontantPaiement,
                ModePaiement = ModePaiement,
                Reference = Reference,
                Ventilations = Ventilations.Where(v => v.Montant > 0).Select(v => new
                {
                    v.EleveId,
                    v.Montant
                }).ToList()
            };

            var result = await _apiService.PostAsync<object, PaiementDto>("api/Paiements", command);

            if (result != null)
            {
                Paiements.Insert(0, result);
                AnnulerNouveauPaiement();
                ShowSuccess("Paiement enregistré avec succès.");
                
                // Invalider les caches
                _cacheService.InvalidateByPrefix("famille");
                _cacheService.InvalidateByPrefix("dashboard");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors de l'enregistrement : {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task VentilationAutomatiqueAsync()
    {
        if (SelectedFamille == null || MontantPaiement <= 0 || Ventilations.Count == 0)
            return;

        // Répartition équitable entre les enfants
        var montantParEnfant = Math.Floor(MontantPaiement / Ventilations.Count);
        var reste = MontantPaiement - (montantParEnfant * Ventilations.Count);

        for (int i = 0; i < Ventilations.Count; i++)
        {
            Ventilations[i].Montant = montantParEnfant + (i == 0 ? reste : 0);
        }

        // Forcer la mise à jour de l'UI
        OnPropertyChanged(nameof(MontantRestantAVentiler));
        OnPropertyChanged(nameof(MontantRestantFormatted));
        OnPropertyChanged(nameof(VentilationValide));
        CommandManager.InvalidateRequerySuggested();

        await Task.CompletedTask;
    }

    private async Task ViewDetailAsync()
    {
        if (SelectedPaiement == null) return;

        try
        {
            IsLoading = true;
            PaiementDetail = await _apiService.GetAsync<PaiementDetailDto>(
                $"api/Paiements/{SelectedPaiement.Id}");
            IsDetailVisible = true;
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du chargement du détail : {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CloseDetail()
    {
        IsDetailVisible = false;
        PaiementDetail = null;
    }

    private void ImprimerRecu()
    {
        if (SelectedPaiement == null) return;
        ShowSuccess($"Impression du reçu #{SelectedPaiement.NumeroRecu} à implémenter.");
    }

    #endregion
}
