using SchoolFlow.Desktop.Core;
using SchoolFlow.Desktop.Services.Interfaces;
using System.Windows.Input;

namespace SchoolFlow.Desktop.ViewModels;

/// <summary>
/// ViewModel principal - Gère la navigation et le menu.
/// </summary>
public class MainViewModel : BaseViewModel
{
    private readonly INavigationService _navigationService;
    private readonly IAuthService _authService;
    private string _titre = "SchoolFlow - Gestion Scolaire";
    private bool _isMenuVisible;
    private string _userName = string.Empty;
    private string _userRole = string.Empty;

    public MainViewModel(
        INavigationService navigationService, 
        IAuthService authService)
    {
        _navigationService = navigationService;
        _authService = authService;

        // Commandes de navigation
        NavigateToDashboardCommand = new AsyncRelayCommand(_ => NavigateToAsync<DashboardViewModel>());
        NavigateToFamillesCommand = new AsyncRelayCommand(_ => NavigateToAsync<FamillesViewModel>());
        NavigateToElevesCommand = new AsyncRelayCommand(_ => NavigateToAsync<ElevesViewModel>());
        NavigateToPaiementsCommand = new AsyncRelayCommand(_ => NavigateToAsync<PaiementsViewModel>());
        NavigateToClassesCommand = new AsyncRelayCommand(_ => NavigateToAsync<ClassesViewModel>());
        LogoutCommand = new AsyncRelayCommand(_ => LogoutAsync());

        // Écoute des changements d'authentification
        _authService.AuthStateChanged += OnAuthStateChanged;
        _navigationService.ViewChanged += OnViewChanged;
    }

    #region Propriétés

    public string Titre
    {
        get => _titre;
        set => SetProperty(ref _titre, value);
    }

    public bool IsMenuVisible
    {
        get => _isMenuVisible;
        set => SetProperty(ref _isMenuVisible, value);
    }

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public string UserRole
    {
        get => _userRole;
        set => SetProperty(ref _userRole, value);
    }

    public BaseViewModel? CurrentViewModel => _navigationService.CurrentViewModel;

    #endregion

    #region Commandes

    public ICommand NavigateToDashboardCommand { get; }
    public ICommand NavigateToFamillesCommand { get; }
    public ICommand NavigateToElevesCommand { get; }
    public ICommand NavigateToPaiementsCommand { get; }
    public ICommand NavigateToClassesCommand { get; }
    public ICommand LogoutCommand { get; }

    #endregion

    #region Méthodes

    public override async Task OnNavigatedToAsync()
    {
        if (_authService.IsAuthenticated)
        {
            UpdateUserInfo();
            IsMenuVisible = true;
            await _navigationService.NavigateToAsync<DashboardViewModel>();
        }
        else
        {
            IsMenuVisible = false;
            await _navigationService.NavigateToAsync<LoginViewModel>();
        }
    }

    private async Task NavigateToAsync<TViewModel>() where TViewModel : BaseViewModel
    {
        await _navigationService.NavigateToAsync<TViewModel>();
    }

    private async Task LogoutAsync()
    {
        if (Confirm("Êtes-vous sûr de vouloir vous déconnecter ?", "Déconnexion"))
        {
            await _authService.LogoutAsync();
        }
    }

    private void OnAuthStateChanged(bool isAuthenticated)
    {
        if (isAuthenticated)
        {
            UpdateUserInfo();
            IsMenuVisible = true;
        }
        else
        {
            IsMenuVisible = false;
            UserName = string.Empty;
            UserRole = string.Empty;
            _ = _navigationService.NavigateToAsync<LoginViewModel>();
        }
    }

    private void OnViewChanged()
    {
        OnPropertyChanged(nameof(CurrentViewModel));
    }

    private void UpdateUserInfo()
    {
        if (_authService.CurrentUser != null)
        {
            UserName = _authService.CurrentUser.NomComplet ?? "Utilisateur";
            UserRole = _authService.CurrentUser.Role ?? "Inconnu";
        }
    }

    #endregion
}
