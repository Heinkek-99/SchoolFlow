using SchoolFlow.Desktop.Core;
using SchoolFlow.Desktop.Services;
using SchoolFlow.Desktop.Services.Interfaces;
using SchoolFlow.Shared.Dtos;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SchoolFlow.Desktop.ViewModels;

/// <summary>
/// ViewModel pour la gestion des familles.
/// </summary>
public class FamillesViewModel : BaseViewModel, IParameterReceiver
{
    private readonly IApiService _apiService;
    private readonly ICacheService _cacheService;
    private readonly INavigationService _navigationService;

    private ObservableCollection<FamilleDto> _familles = new();
    private FamilleDto? _selectedFamille;
    private FamilleDetailDto? _familleDetail;
    private string _searchText = string.Empty;
    private bool _isDetailVisible;

    public FamillesViewModel(
        IApiService apiService,
        ICacheService cacheService,
        INavigationService navigationService)
    {
        _apiService = apiService;
        _cacheService = cacheService;
        _navigationService = navigationService;

        // Commandes
        LoadFamillesCommand = new AsyncRelayCommand(_ => LoadFamillesAsync());
        SearchCommand = new AsyncRelayCommand(_ => SearchFamillesAsync());
        AddFamilleCommand = new RelayCommand(_ => AddFamille());
        EditFamilleCommand = new RelayCommand(_ => EditFamille(), _ => SelectedFamille != null);
        DeleteFamilleCommand = new AsyncRelayCommand(_ => DeleteFamilleAsync(), _ => SelectedFamille != null);
        ViewDetailCommand = new AsyncRelayCommand(_ => ViewDetailAsync(), _ => SelectedFamille != null);
        CloseDetailCommand = new RelayCommand(_ => CloseDetail());
        AddEnfantCommand = new RelayCommand(_ => AddEnfant(), _ => SelectedFamille != null);
    }

    #region Propriétés

    public ObservableCollection<FamilleDto> Familles
    {
        get => _familles;
        set => SetProperty(ref _familles, value);
    }

    public FamilleDto? SelectedFamille
    {
        get => _selectedFamille;
        set
        {
            SetProperty(ref _selectedFamille, value);
            CommandManager.InvalidateRequerySuggested();
            if (value != null && IsDetailVisible)
            {
                _ = ViewDetailAsync();
            }
        }
    }

    public FamilleDetailDto? FamilleDetail
    {
        get => _familleDetail;
        set => SetProperty(ref _familleDetail, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public bool IsDetailVisible
    {
        get => _isDetailVisible;
        set => SetProperty(ref _isDetailVisible, value);
    }

    // Statistiques de la famille sélectionnée
    public int NombreEnfants => FamilleDetail?.Enfants?.Count ?? 0;
    public string SoldeFormatted => FamilleDetail != null ? $"{FamilleDetail.SoldeGlobal:N0} FCFA" : "0 FCFA";
    public bool EstEnRegle => FamilleDetail?.SoldeGlobal <= 0;

    #endregion

    #region Commandes

    public ICommand LoadFamillesCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand AddFamilleCommand { get; }
    public ICommand EditFamilleCommand { get; }
    public ICommand DeleteFamilleCommand { get; }
    public ICommand ViewDetailCommand { get; }
    public ICommand CloseDetailCommand { get; }
    public ICommand AddEnfantCommand { get; }

    #endregion

    #region Méthodes

    public void ReceiveParameter(object parameter)
    {
        if (parameter is Guid familleId)
        {
            _ = LoadFamilleDetailAsync(familleId);
        }
    }

    public override async Task OnNavigatedToAsync()
    {
        await LoadFamillesAsync();
    }

    private async Task LoadFamillesAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();

            var familles = await _apiService.GetAsync<List<FamilleDto>>("api/Familles");
            
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
        finally
        {
            IsLoading = false;
        }
    }

    private async Task SearchFamillesAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadFamillesAsync();
            return;
        }

        try
        {
            IsLoading = true;
            var familles = await _apiService.GetAsync<List<FamilleDto>>(
                $"api/Familles/search?terme={Uri.EscapeDataString(SearchText)}");
            
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
            ShowError($"Erreur lors de la recherche : {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ViewDetailAsync()
    {
        if (SelectedFamille == null) return;

        await LoadFamilleDetailAsync(SelectedFamille.Id);
        IsDetailVisible = true;
    }

    private async Task LoadFamilleDetailAsync(Guid familleId)
    {
        try
        {
            IsLoading = true;
            FamilleDetail = await _apiService.GetAsync<FamilleDetailDto>($"api/Familles/{familleId}");
            
            OnPropertyChanged(nameof(NombreEnfants));
            OnPropertyChanged(nameof(SoldeFormatted));
            OnPropertyChanged(nameof(EstEnRegle));
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
        FamilleDetail = null;
    }

    private void AddFamille()
    {
        _ = _navigationService.NavigateToAsync<FamilleFormViewModel>();
    }

    private void EditFamille()
    {
        if (SelectedFamille == null) return;
        _ = _navigationService.NavigateToAsync<FamilleFormViewModel>(SelectedFamille.Id);
    }

    private async Task DeleteFamilleAsync()
    {
        if (SelectedFamille == null) return;

        if (!Confirm($"Êtes-vous sûr de vouloir supprimer la famille {SelectedFamille.NomPere} ?"))
            return;

        try
        {
            IsLoading = true;
            var success = await _apiService.DeleteAsync($"api/Familles/{SelectedFamille.Id}");
            
            if (success)
            {
                Familles.Remove(SelectedFamille);
                SelectedFamille = null;
                CloseDetail();
                ShowSuccess("Famille supprimée avec succès.");
                _cacheService.InvalidateByPrefix("famille");
                _cacheService.InvalidateByPrefix("dashboard");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors de la suppression : {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void AddEnfant()
    {
        if (SelectedFamille == null) return;
        _ = _navigationService.NavigateToAsync<EleveFormViewModel>(SelectedFamille);
    }

    #endregion
}
