using SchoolFlow.Desktop.Core;
using SchoolFlow.Desktop.Services.Interfaces;
using SchoolFlow.Shared.Dtos;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SchoolFlow.Desktop.ViewModels;

/// <summary>
/// ViewModel pour la gestion des classes.
/// </summary>
public class ClassesViewModel : BaseViewModel
{
    private readonly IApiService _apiService;
    private readonly ICacheService _cacheService;

    private ObservableCollection<ClasseDto> _classes = new();
    private ClasseDetailDto? _classeDetail;
    private ClasseDto? _selectedClasse;
    private bool _isDetailVisible;

    public ClassesViewModel(
        IApiService apiService,
        ICacheService cacheService)
    {
        _apiService = apiService;
        _cacheService = cacheService;

        LoadClassesCommand = new AsyncRelayCommand(_ => LoadClassesAsync());
        ViewDetailCommand = new AsyncRelayCommand(_ => ViewDetailAsync(), _ => SelectedClasse != null);
        CloseDetailCommand = new RelayCommand(_ => CloseDetail());
        AddClasseCommand = new RelayCommand(_ => AddClasse());
        EditClasseCommand = new RelayCommand(_ => EditClasse(), _ => SelectedClasse != null);
    }

    #region Propriétés

    public ObservableCollection<ClasseDto> Classes
    {
        get => _classes;
        set => SetProperty(ref _classes, value);
    }

    public ClasseDto? SelectedClasse
    {
        get => _selectedClasse;
        set
        {
            SetProperty(ref _selectedClasse, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public ClasseDetailDto? ClasseDetail
    {
        get => _classeDetail;
        set => SetProperty(ref _classeDetail, value);
    }

    public bool IsDetailVisible
    {
        get => _isDetailVisible;
        set => SetProperty(ref _isDetailVisible, value);
    }

    // Statistiques
    public int NombreEleves => ClasseDetail?.Eleves?.Count ?? 0;

    #endregion

    #region Commandes

    public ICommand LoadClassesCommand { get; }
    public ICommand ViewDetailCommand { get; }
    public ICommand CloseDetailCommand { get; }
    public ICommand AddClasseCommand { get; }
    public ICommand EditClasseCommand { get; }

    #endregion

    #region Méthodes

    public override async Task OnNavigatedToAsync()
    {
        await LoadClassesAsync();
    }

    private async Task LoadClassesAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();

            var classes = await _apiService.GetAsync<List<ClasseDto>>("api/Classes");
            
            Classes.Clear();
            if (classes != null)
            {
                foreach (var classe in classes.OrderBy(c => c.Niveau).ThenBy(c => c.Nom))
                {
                    Classes.Add(classe);
                }
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du chargement des classes : {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ViewDetailAsync()
    {
        if (SelectedClasse == null) return;

        try
        {
            IsLoading = true;
            ClasseDetail = await _apiService.GetAsync<ClasseDetailDto>($"api/Classes/{SelectedClasse.Id}");
            OnPropertyChanged(nameof(NombreEleves));
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
        ClasseDetail = null;
    }

    private void AddClasse()
    {
        ShowSuccess("Fonctionnalité de création de classe à implémenter.");
    }

    private void EditClasse()
    {
        if (SelectedClasse == null) return;
        ShowSuccess($"Modification de la classe {SelectedClasse.Nom} à implémenter.");
    }

    #endregion
}
