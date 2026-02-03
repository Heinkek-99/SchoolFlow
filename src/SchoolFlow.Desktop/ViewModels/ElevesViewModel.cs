using SchoolFlow.Desktop.Core;
using SchoolFlow.Desktop.Services;
using SchoolFlow.Desktop.Services.Interfaces;
using SchoolFlow.Shared.Dtos;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SchoolFlow.Desktop.ViewModels;

/// <summary>
/// ViewModel pour la gestion des élèves.
/// </summary>
public class ElevesViewModel : BaseViewModel, IParameterReceiver
{
    private readonly IApiService _apiService;
    private readonly ICacheService _cacheService;
    private readonly INavigationService _navigationService;

    private ObservableCollection<EleveDto> _eleves = new();
    private ObservableCollection<ClasseDto> _classes = new();
    private EleveDto? _selectedEleve;
    private EleveDossierDto? _eleveDossier;
    private ClasseDto? _selectedClasse;
    private string _searchText = string.Empty;
    private bool _isDetailVisible;

    public ElevesViewModel(
        IApiService apiService,
        ICacheService cacheService,
        INavigationService navigationService)
    {
        _apiService = apiService;
        _cacheService = cacheService;
        _navigationService = navigationService;

        // Commandes
        LoadElevesCommand = new AsyncRelayCommand(_ => LoadElevesAsync());
        SearchCommand = new AsyncRelayCommand(_ => SearchElevesAsync());
        FilterByClasseCommand = new AsyncRelayCommand(_ => FilterByClasseAsync());
        AddEleveCommand = new RelayCommand(_ => AddEleve());
        EditEleveCommand = new RelayCommand(_ => EditEleve(), _ => SelectedEleve != null);
        DeleteEleveCommand = new AsyncRelayCommand(_ => DeleteEleveAsync(), _ => SelectedEleve != null);
        ViewDetailCommand = new AsyncRelayCommand(_ => ViewDetailAsync(), _ => SelectedEleve != null);
        ViewFamilleCommand = new AsyncRelayCommand(_ => ViewFamilleAsync(), _ => SelectedEleve != null);
        CloseDetailCommand = new RelayCommand(_ => CloseDetail());
    }

    #region Propriétés

    public ObservableCollection<EleveDto> Eleves
    {
        get => _eleves;
        set => SetProperty(ref _eleves, value);
    }

    public ObservableCollection<ClasseDto> Classes
    {
        get => _classes;
        set => SetProperty(ref _classes, value);
    }

    public EleveDto? SelectedEleve
    {
        get => _selectedEleve;
        set
        {
            SetProperty(ref _selectedEleve, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public EleveDossierDto? EleveDossier
    {
        get => _eleveDossier;
        set => SetProperty(ref _eleveDossier, value);
    }

    public ClasseDto? SelectedClasse
    {
        get => _selectedClasse;
        set => SetProperty(ref _selectedClasse, value);
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

    // Propriétés calculées
    public string SoldeEleveFormatted => EleveDossier != null ? $"{EleveDossier.Solde:N0} FCFA" : "0 FCFA";

    #endregion

    #region Commandes

    public ICommand LoadElevesCommand { get; }
    public ICommand SearchCommand { get; }
    public ICommand FilterByClasseCommand { get; }
    public ICommand AddEleveCommand { get; }
    public ICommand EditEleveCommand { get; }
    public ICommand DeleteEleveCommand { get; }
    public ICommand ViewDetailCommand { get; }
    public ICommand ViewFamilleCommand { get; }
    public ICommand CloseDetailCommand { get; }

    #endregion

    #region Méthodes

    public void ReceiveParameter(object parameter)
    {
        if (parameter is Guid eleveId)
        {
            _ = LoadEleveDossierAsync(eleveId);
        }
    }

    public override async Task OnNavigatedToAsync()
    {
        await Task.WhenAll(LoadElevesAsync(), LoadClassesAsync());
    }

    private async Task LoadElevesAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();

            var eleves = await _apiService.GetAsync<List<EleveDto>>("api/Eleves");
            
            Eleves.Clear();
            if (eleves != null)
            {
                foreach (var eleve in eleves)
                {
                    Eleves.Add(eleve);
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du chargement des élèves : {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadClassesAsync()
    {
        try
        {
            var classes = await _cacheService.GetOrAddAsync(
                "classes_list",
                () => _apiService.GetAsync<List<ClasseDto>>("api/Classes"),
                TimeSpan.FromMinutes(10)
            );

            Classes.Clear();
            Classes.Add(new ClasseDto { Id = Guid.Empty, Nom = "Toutes les classes" });
            
            if (classes != null)
            {
                foreach (var classe in classes)
                {
                    Classes.Add(classe);
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du chargement des classes : {ex.Message}");
        }
    }

    private async Task SearchElevesAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadElevesAsync();
            return;
        }

        try
        {
            IsLoading = true;
            var eleves = await _apiService.GetAsync<List<EleveDto>>(
                $"api/Eleves/search?terme={Uri.EscapeDataString(SearchText)}");
            
            Eleves.Clear();
            if (eleves != null)
            {
                foreach (var eleve in eleves)
                {
                    Eleves.Add(eleve);
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

    private async Task FilterByClasseAsync()
    {
        if (SelectedClasse == null || SelectedClasse.Id == Guid.Empty)
        {
            await LoadElevesAsync();
            return;
        }

        try
        {
            IsLoading = true;
            var eleves = await _apiService.GetAsync<List<EleveDto>>(
                $"api/Eleves/classe/{SelectedClasse.Id}");
            
            Eleves.Clear();
            if (eleves != null)
            {
                foreach (var eleve in eleves)
                {
                    Eleves.Add(eleve);
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du filtrage : {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ViewDetailAsync()
    {
        if (SelectedEleve == null) return;

        await LoadEleveDossierAsync(SelectedEleve.Id);
        IsDetailVisible = true;
    }

    private async Task LoadEleveDossierAsync(Guid eleveId)
    {
        try
        {
            IsLoading = true;
            EleveDossier = await _apiService.GetAsync<EleveDossierDto>($"api/Eleves/{eleveId}/dossier");
            OnPropertyChanged(nameof(SoldeEleveFormatted));
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du chargement du dossier : {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ViewFamilleAsync()
    {
        if (SelectedEleve?.FamilleId == null) return;
        await _navigationService.NavigateToAsync<FamillesViewModel>(SelectedEleve.FamilleId.Value);
    }

    private void CloseDetail()
    {
        IsDetailVisible = false;
        EleveDossier = null;
    }

    private void AddEleve()
    {
        _ = _navigationService.NavigateToAsync<EleveFormViewModel>();
    }

    private void EditEleve()
    {
        if (SelectedEleve == null) return;
        _ = _navigationService.NavigateToAsync<EleveFormViewModel>(SelectedEleve.Id);
    }

    private async Task DeleteEleveAsync()
    {
        if (SelectedEleve == null) return;

        if (!Confirm($"Êtes-vous sûr de vouloir supprimer l'élève {SelectedEleve.Prenom} {SelectedEleve.Nom} ?"))
            return;

        try
        {
            IsLoading = true;
            var success = await _apiService.DeleteAsync($"api/Eleves/{SelectedEleve.Id}");
            
            if (success)
            {
                Eleves.Remove(SelectedEleve);
                SelectedEleve = null;
                CloseDetail();
                ShowSuccess("Élève supprimé avec succès.");
                _cacheService.InvalidateByPrefix("eleve");
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

    #endregion
}
