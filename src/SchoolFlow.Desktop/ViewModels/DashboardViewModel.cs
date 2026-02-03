using SchoolFlow.Desktop.Core;
using SchoolFlow.Desktop.Services.Interfaces;
using SchoolFlow.Shared.Dtos;
using System.Windows.Input;

namespace SchoolFlow.Desktop.ViewModels;

/// <summary>
/// ViewModel pour le tableau de bord.
/// </summary>
public class DashboardViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly ICacheService _cacheService;
    private readonly INavigationService _navigationService;

    private DashboardStats? _stats;
    private int _totalFamilles;
    private int _totalEleves;
    private decimal _totalRecettes;
    private decimal _totalImpayes;
    private int _totalClasses;

    public DashboardViewModel(
        IApiService apiService, 
        ICacheService cacheService,
        INavigationService navigationService)
    {
        _apiService = apiService;
        _cacheService = cacheService;
        _navigationService = navigationService;

        RefreshCommand = new AsyncRelayCommand(_ => LoadStatsAsync(forceRefresh: true));
        NavigateToFamillesCommand = new AsyncRelayCommand(_ => _navigationService.NavigateToAsync<FamillesViewModel>());
        NavigateToElevesCommand = new AsyncRelayCommand(_ => _navigationService.NavigateToAsync<ElevesViewModel>());
        NavigateToPaiementsCommand = new AsyncRelayCommand(_ => _navigationService.NavigateToAsync<PaiementsViewModel>());
    }

    #region Propriétés

    public DashboardStats? Stats
    {
        get => _stats;
        set => SetProperty(ref _stats, value);
    }

    public int TotalFamilles
    {
        get => _totalFamilles;
        set => SetProperty(ref _totalFamilles, value);
    }

    public int TotalEleves
    {
        get => _totalEleves;
        set => SetProperty(ref _totalEleves, value);
    }

    public decimal TotalRecettes
    {
        get => _totalRecettes;
        set => SetProperty(ref _totalRecettes, value);
    }

    public decimal TotalImpayes
    {
        get => _totalImpayes;
        set => SetProperty(ref _totalImpayes, value);
    }

    public int TotalClasses
    {
        get => _totalClasses;
        set => SetProperty(ref _totalClasses, value);
    }

    // Propriétés calculées pour l'affichage
    public string TotalRecettesFormatted => $"{TotalRecettes:N0} FCFA";
    public string TotalImpayesFormatted => $"{TotalImpayes:N0} FCFA";
    public double TauxRecouvrement => TotalRecettes + TotalImpayes > 0 
        ? (double)(TotalRecettes / (TotalRecettes + TotalImpayes) * 100) 
        : 0;

    #endregion

    #region Commandes

    public ICommand RefreshCommand { get; }
    public ICommand NavigateToFamillesCommand { get; }
    public ICommand NavigateToElevesCommand { get; }
    public ICommand NavigateToPaiementsCommand { get; }

    #endregion

    #region Méthodes

    public override async Task OnNavigatedToAsync()
    {
        await LoadStatsAsync();
    }

    private async Task LoadStatsAsync(bool forceRefresh = false)
    {
        try
        {
            IsLoading = true;
            ClearError();

            if (forceRefresh)
            {
                _cacheService.InvalidateByPrefix("dashboard");
            }

            var stats = await _cacheService.GetOrAddAsync(
                "dashboard_stats",
                () => _apiService.GetAsync<DashboardStats>("api/Dashboard/stats"),
                TimeSpan.FromMinutes(2)
            );

            if (stats != null)
            {
                Stats = stats;
                TotalFamilles = stats.TotalFamilles;
                TotalEleves = stats.TotalEleves;
                TotalRecettes = stats.TotalRecettes;
                TotalImpayes = stats.TotalImpayes;
                TotalClasses = stats.TotalClasses;

                // Notification des propriétés calculées
                OnPropertyChanged(nameof(TotalRecettesFormatted));
                OnPropertyChanged(nameof(TotalImpayesFormatted));
                OnPropertyChanged(nameof(TauxRecouvrement));
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du chargement des statistiques : {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion
}
