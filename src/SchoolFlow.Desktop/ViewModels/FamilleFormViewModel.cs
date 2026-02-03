using FluentValidation.Results;
using SchoolFlow.Desktop.Core;
using SchoolFlow.Desktop.Services;
using SchoolFlow.Desktop.Services.Interfaces;
using SchoolFlow.Desktop.Validators;
using SchoolFlow.Shared.Dtos;
using System.Windows.Input;

namespace SchoolFlow.Desktop.ViewModels;

/// <summary>
/// ViewModel pour la création et modification d'une famille.
/// </summary>
public class FamilleFormViewModel : BaseViewModel, IParameterReceiver
{
    private readonly IApiService _apiService;
    private readonly INavigationService _navigationService;
    private readonly ICacheService _cacheService;
    private readonly FamilleValidator _validator;

    private FamilleFormModel _formModel = new();
    private Dictionary<string, string> _validationErrors = new();
    private bool _isEditMode;
    private string _pageTitle = "Nouvelle Famille";

    public FamilleFormViewModel(
        IApiService apiService,
        INavigationService navigationService,
        ICacheService cacheService)
    {
        _apiService = apiService;
        _navigationService = navigationService;
        _cacheService = cacheService;
        _validator = new FamilleValidator();

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

    // Propriétés du formulaire
    public string NomPere
    {
        get => _formModel.NomPere;
        set { _formModel.NomPere = value; OnPropertyChanged(); ValidateProperty(nameof(NomPere)); }
    }

    public string? PrenomPere
    {
        get => _formModel.PrenomPere;
        set { _formModel.PrenomPere = value; OnPropertyChanged(); }
    }

    public string? NomMere
    {
        get => _formModel.NomMere;
        set { _formModel.NomMere = value; OnPropertyChanged(); }
    }

    public string? PrenomMere
    {
        get => _formModel.PrenomMere;
        set { _formModel.PrenomMere = value; OnPropertyChanged(); }
    }

    public string Telephone
    {
        get => _formModel.Telephone;
        set { _formModel.Telephone = value; OnPropertyChanged(); ValidateProperty(nameof(Telephone)); }
    }

    public string? TelephoneSecondaire
    {
        get => _formModel.TelephoneSecondaire;
        set { _formModel.TelephoneSecondaire = value; OnPropertyChanged(); }
    }

    public string? Email
    {
        get => _formModel.Email;
        set { _formModel.Email = value; OnPropertyChanged(); ValidateProperty(nameof(Email)); }
    }

    public string? Adresse
    {
        get => _formModel.Adresse;
        set { _formModel.Adresse = value; OnPropertyChanged(); }
    }

    public string? Quartier
    {
        get => _formModel.Quartier;
        set { _formModel.Quartier = value; OnPropertyChanged(); }
    }

    public string? Ville
    {
        get => _formModel.Ville;
        set { _formModel.Ville = value; OnPropertyChanged(); }
    }

    // Erreurs de validation
    public string? NomPereError => GetError(nameof(NomPere));
    public string? TelephoneError => GetError(nameof(Telephone));
    public string? EmailError => GetError(nameof(Email));
    public bool HasErrors => _validationErrors.Count > 0;

    #endregion

    #region Commandes

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    #endregion

    #region Méthodes

    public void ReceiveParameter(object parameter)
    {
        if (parameter is FamilleDetailDto famille)
        {
            LoadFamille(famille);
        }
        else if (parameter is Guid familleId)
        {
            _ = LoadFamilleByIdAsync(familleId);
        }
    }

    private void LoadFamille(FamilleDetailDto famille)
    {
        _formModel = new FamilleFormModel
        {
            Id = famille.Id,
            NomPere = famille.NomPere ?? string.Empty,
            PrenomPere = famille.PrenomPere,
            NomMere = famille.NomMere,
            PrenomMere = famille.PrenomMere,
            Telephone = famille.TelephonePere ?? string.Empty,
            TelephoneSecondaire = famille.TelephoneMere,
            Email = famille.EmailPere,
            Adresse = famille.Adresse,
            Quartier = famille.Adresse,
            Ville = famille.Ville
        };

        IsEditMode = true;
        PageTitle = $"Modifier - Famille {famille.NomPere}";
        RefreshAllProperties();
    }

    private async Task LoadFamilleByIdAsync(Guid familleId)
    {
        try
        {
            IsLoading = true;
            var famille = await _apiService.GetAsync<FamilleDetailDto>($"api/Familles/{familleId}");
            if (famille != null)
            {
                LoadFamille(famille);
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
        return !IsLoading && !HasErrors && !string.IsNullOrWhiteSpace(NomPere) && !string.IsNullOrWhiteSpace(Telephone);
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
                    NomPere,
                    PrenomPere,
                    NomMere,
                    PrenomMere,
                    Telephone,
                    TelephoneSecondaire,
                    Email,
                    Adresse,
                    Quartier,
                    Ville
                };

                await _apiService.PutAsync<object, FamilleDto>($"api/Familles/{_formModel.Id}", updateCommand);
                ShowSuccess("Famille mise à jour avec succès.");
            }
            else
            {
                // Création
                var createCommand = new
                {
                    NomPere,
                    PrenomPere,
                    NomMere,
                    PrenomMere,
                    Telephone,
                    TelephoneSecondaire,
                    Email,
                    Adresse,
                    Quartier,
                    Ville
                };

                await _apiService.PostAsync<object, FamilleDto>("api/Familles", createCommand);
                ShowSuccess("Famille créée avec succès.");
            }

            // Invalider le cache et retourner
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
        OnPropertyChanged(nameof(NomPere));
        OnPropertyChanged(nameof(PrenomPere));
        OnPropertyChanged(nameof(NomMere));
        OnPropertyChanged(nameof(PrenomMere));
        OnPropertyChanged(nameof(Telephone));
        OnPropertyChanged(nameof(TelephoneSecondaire));
        OnPropertyChanged(nameof(Email));
        OnPropertyChanged(nameof(Adresse));
        OnPropertyChanged(nameof(Quartier));
        OnPropertyChanged(nameof(Ville));
    }

    private void RefreshErrorProperties()
    {
        OnPropertyChanged(nameof(NomPereError));
        OnPropertyChanged(nameof(TelephoneError));
        OnPropertyChanged(nameof(EmailError));
        OnPropertyChanged(nameof(HasErrors));
    }

    #endregion
}
