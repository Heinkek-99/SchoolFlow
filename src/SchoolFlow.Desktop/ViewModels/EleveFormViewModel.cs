using FluentValidation.Results;
using SchoolFlow.Desktop.Core;
using SchoolFlow.Desktop.Services;
using SchoolFlow.Desktop.Services.Interfaces;
using SchoolFlow.Desktop.Validators;
using SchoolFlow.Shared.Dtos;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace SchoolFlow.Desktop.ViewModels;

/// <summary>
/// ViewModel pour la création et modification d'un élève.
/// </summary>
public class EleveFormViewModel : BaseViewModel, IParameterReceiver
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;
    private readonly ICacheService _cacheService;
    private readonly EleveValidator _validator;

    private EleveFormModel _formModel = new();
    private Dictionary<string, string> _validationErrors = new();
    private ObservableCollection<FamilleDto> _familles = new();
    private ObservableCollection<ClasseDto> _classes = new();
    private FamilleDto? _selectedFamille;
    private ClasseDto? _selectedClasse;
    private bool _isEditMode;
    private string _pageTitle = "Inscrire un Élève";

    public EleveFormViewModel(
        IApiService apiService,
        INavigationService navigationService,
        ICacheService cacheService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        _cacheService = cacheService;
        _validator = new EleveValidator();

        SaveCommand = new AsyncRelayCommand(_ => SaveAsync(), _ => CanSave());
        CancelCommand = new AsyncRelayCommand(_ => CancelAsync());
    }

    #region Propriétés

    public string PageTitle
    {
        get => _pageTitle;
        set => SetProperty(ref _pageTitle, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    // Listes déroulantes
    public ObservableCollection<FamilleDto> Familles
    {
        get => _familles;
        set => SetProperty(ref _familles, value);
    }

    public ObservableCollection<ClasseDto> Classes
    {
        get => _classes;
        set => SetProperty(ref _classes, value);
    }

    public FamilleDto? SelectedFamille
    {
        get => _selectedFamille;
        set
        {
            SetProperty(ref _selectedFamille, value);
            if (value != null)
            {
                _formModel.FamilleId = value.Id;
                ValidateProperty(nameof(FamilleId));
            }
        }
    }

    public ClasseDto? SelectedClasse
    {
        get => _selectedClasse;
        set
        {
            SetProperty(ref _selectedClasse, value);
            if (value != null)
            {
                _formModel.ClasseId = value.Id;
                ValidateProperty(nameof(ClasseId));
            }
        }
    }

    // Sexes disponibles
    public List<SexeOption> Sexes { get; } = new()
    {
        new SexeOption { Code = "M", Libelle = "Masculin" },
        new SexeOption { Code = "F", Libelle = "Féminin" }
    };

    private SexeOption? _selectedSexe;
    public SexeOption? SelectedSexe
    {
        get => _selectedSexe;
        set
        {
            SetProperty(ref _selectedSexe, value);
            if (value != null)
            {
                _formModel.Sexe = value.Code;
            }
        }
    }

    // Propriétés du formulaire
    public string? Matricule
    {
        get => _formModel.Matricule;
        set { _formModel.Matricule = value; OnPropertyChanged(); }
    }

    public string Nom
    {
        get => _formModel.Nom;
        set { _formModel.Nom = value; OnPropertyChanged(); ValidateProperty(nameof(Nom)); }
    }

    public string Prenom
    {
        get => _formModel.Prenom;
        set { _formModel.Prenom = value; OnPropertyChanged(); ValidateProperty(nameof(Prenom)); }
    }

    public DateTime? DateNaissance
    {
        get => _formModel.DateNaissance;
        set { _formModel.DateNaissance = value; OnPropertyChanged(); ValidateProperty(nameof(DateNaissance)); }
    }

    public string? LieuNaissance
    {
        get => _formModel.LieuNaissance;
        set { _formModel.LieuNaissance = value; OnPropertyChanged(); }
    }

    public Guid FamilleId
    {
        get => _formModel.FamilleId;
        set { _formModel.FamilleId = value; OnPropertyChanged(); }
    }

    public Guid ClasseId
    {
        get => _formModel.ClasseId;
        set { _formModel.ClasseId = value; OnPropertyChanged(); }
    }

    public string? Nationalite
    {
        get => _formModel.Nationalite;
        set { _formModel.Nationalite = value; OnPropertyChanged(); }
    }

    public string? Allergies
    {
        get => _formModel.Allergies;
        set { _formModel.Allergies = value; OnPropertyChanged(); }
    }

    public string? Observations
    {
        get => _formModel.Observations;
        set { _formModel.Observations = value; OnPropertyChanged(); }
    }

    public bool EstBoursier
    {
        get => _formModel.EstBoursier;
        set { _formModel.EstBoursier = value; OnPropertyChanged(); }
    }

    public bool EstRedoublant
    {
        get => _formModel.EstRedoublant;
        set { _formModel.EstRedoublant = value; OnPropertyChanged(); }
    }

    // Erreurs de validation
    public string? NomError => GetError(nameof(Nom));
    public string? PrenomError => GetError(nameof(Prenom));
    public string? DateNaissanceError => GetError(nameof(DateNaissance));
    public string? FamilleError => GetError(nameof(FamilleId));
    public string? ClasseError => GetError(nameof(ClasseId));
    public bool HasErrors => _validationErrors.Count > 0;

    #endregion

    #region Commandes

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    #endregion

    #region Méthodes

    public void ReceiveParameter(object parameter)
    {
        if (parameter is EleveDto eleve)
        {
            _ = LoadEleveAsync(eleve.Id);
        }
        else if (parameter is Guid eleveId)
        {
            _ = LoadEleveAsync(eleveId);
        }
        else if (parameter is FamilleDto famille)
        {
            // Pré-remplir la famille si on vient de FamillesView
            SelectedFamille = famille;
        }
    }

    public override async Task OnNavigatedToAsync()
    {
        await LoadDropdownDataAsync();
    }

    private async Task LoadDropdownDataAsync()
    {
        try
        {
            IsLoading = true;

            // Charger familles et classes en parallèle
            var famillesTask = _cacheService.GetOrAddAsync(
                "familles_list",
                () => _apiService.GetAsync<List<FamilleDto>>("api/Familles"),
                TimeSpan.FromMinutes(5)
            );

            var classesTask = _cacheService.GetOrAddAsync(
                "classes_list",
                () => _apiService.GetAsync<List<ClasseDto>>("api/Classes"),
                TimeSpan.FromMinutes(10)
            );

            await Task.WhenAll(famillesTask, classesTask);

            Familles.Clear();
            if (famillesTask.Result != null)
            {
                foreach (var f in famillesTask.Result)
                {
                    Familles.Add(f);
                }
            }

            Classes.Clear();
            if (classesTask.Result != null)
            {
                foreach (var c in classesTask.Result.OrderBy(c => c.Niveau).ThenBy(c => c.Nom))
                {
                    Classes.Add(c);
                }
            }

            // Sélectionner sexe par défaut
            SelectedSexe = Sexes.First();
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du chargement des données : {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadEleveAsync(Guid eleveId)
    {
        try
        {
            IsLoading = true;
            var eleve = await _apiService.GetAsync<EleveDossierDto>($"api/Eleves/{eleveId}/dossier");
            
            if (eleve != null)
            {
                _formModel = new EleveFormModel
                {
                    Id = eleve.Id,
                    Matricule = eleve.Matricule,
                    Nom = eleve.Nom ?? string.Empty,
                    Prenom = eleve.Prenom ?? string.Empty,
                    DateNaissance = eleve.DateNaissance,
                    LieuNaissance = eleve.LieuNaissance,
                    Sexe = eleve.Sexe ?? "M",
                    FamilleId = eleve.FamilleId,
                    ClasseId = eleve.ClasseId,
                    Nationalite = eleve.Nationalite,
                    Allergies = eleve.Allergies,
                    Observations = eleve.Observations,
                    EstBoursier = eleve.EstBoursier,
                    EstRedoublant = eleve.EstRedoublant
                };

                IsEditMode = true;
                PageTitle = $"Modifier - {eleve.Prenom} {eleve.Nom}";
                
                // Sélectionner dans les listes
                SelectedFamille = Familles.FirstOrDefault(f => f.Id == eleve.FamilleId);
                SelectedClasse = Classes.FirstOrDefault(c => c.Id == eleve.ClasseId);
                SelectedSexe = Sexes.FirstOrDefault(s => s.Code == eleve.Sexe);

                RefreshAllProperties();
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur lors du chargement : {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanSave()
    {
        return !IsLoading && 
               !HasErrors && 
               !string.IsNullOrWhiteSpace(Nom) && 
               !string.IsNullOrWhiteSpace(Prenom) &&
               SelectedFamille != null &&
               SelectedClasse != null;
    }

    private async Task SaveAsync()
    {
        // Validation complète
        var result = await _validator.ValidateAsync(_formModel);
        if (!result.IsValid)
        {
            UpdateValidationErrors(result);
            ShowError("Veuillez corriger les erreurs du formulaire.");
            return;
        }

        try
        {
            IsLoading = true;
            ClearError();

            if (IsEditMode)
            {
                // Mise à jour
                var updateCommand = new
                {
                    Id = _formModel.Id,
                    Nom,
                    Prenom,
                    DateNaissance,
                    LieuNaissance,
                    Sexe = SelectedSexe?.Code,
                    FamilleId = SelectedFamille?.Id,
                    ClasseId = SelectedClasse?.Id,
                    Nationalite,
                    Allergies,
                    Observations,
                    EstBoursier,
                    EstRedoublant
                };

                await _apiService.PutAsync<object, EleveDto>($"api/Eleves/{_formModel.Id}", updateCommand);
                ShowSuccess("Élève mis à jour avec succès.");
            }
            else
            {
                // Création (inscription)
                var createCommand = new
                {
                    Nom,
                    Prenom,
                    DateNaissance,
                    LieuNaissance,
                    Sexe = SelectedSexe?.Code,
                    FamilleId = SelectedFamille?.Id,
                    ClasseId = SelectedClasse?.Id,
                    Nationalite,
                    Allergies,
                    Observations,
                    EstBoursier,
                    EstRedoublant
                };

                var newEleve = await _apiService.PostAsync<object, EleveDto>("api/Eleves", createCommand);
                ShowSuccess($"Élève inscrit avec succès.\nMatricule: {newEleve?.Matricule}");
            }

            // Invalider les caches
            _cacheService.InvalidateByPrefix("eleve");
            _cacheService.InvalidateByPrefix("famille");
            _cacheService.InvalidateByPrefix("dashboard");
            await _navigationService.GoBackAsync();
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

    private async Task CancelAsync()
    {
        if (Confirm("Êtes-vous sûr de vouloir annuler ? Les modifications seront perdues."))
        {
            await _navigationService.GoBackAsync();
        }
    }

    private void ValidateProperty(string propertyName)
    {
        var result = _validator.Validate(_formModel);
        var error = result.Errors.FirstOrDefault(e => e.PropertyName == propertyName);
        
        if (error != null)
        {
            _validationErrors[propertyName] = error.ErrorMessage;
        }
        else
        {
            _validationErrors.Remove(propertyName);
        }

        OnPropertyChanged($"{propertyName}Error");
        OnPropertyChanged(nameof(HasErrors));
        CommandManager.InvalidateRequerySuggested();
    }

    private void UpdateValidationErrors(ValidationResult result)
    {
        _validationErrors.Clear();
        foreach (var error in result.Errors)
        {
            _validationErrors[error.PropertyName] = error.ErrorMessage;
        }
        RefreshErrorProperties();
    }

    private string? GetError(string propertyName)
    {
        return _validationErrors.TryGetValue(propertyName, out var error) ? error : null;
    }

    private void RefreshAllProperties()
    {
        OnPropertyChanged(nameof(Matricule));
        OnPropertyChanged(nameof(Nom));
        OnPropertyChanged(nameof(Prenom));
        OnPropertyChanged(nameof(DateNaissance));
        OnPropertyChanged(nameof(LieuNaissance));
        OnPropertyChanged(nameof(Nationalite));
        OnPropertyChanged(nameof(Allergies));
        OnPropertyChanged(nameof(Observations));
        OnPropertyChanged(nameof(EstBoursier));
        OnPropertyChanged(nameof(EstRedoublant));
    }

    private void RefreshErrorProperties()
    {
        OnPropertyChanged(nameof(NomError));
        OnPropertyChanged(nameof(PrenomError));
        OnPropertyChanged(nameof(DateNaissanceError));
        OnPropertyChanged(nameof(FamilleError));
        OnPropertyChanged(nameof(ClasseError));
        OnPropertyChanged(nameof(HasErrors));
    }

    #endregion
}

/// <summary>
/// Option pour le sexe dans la liste déroulante.
/// </summary>
public class SexeOption
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
}
