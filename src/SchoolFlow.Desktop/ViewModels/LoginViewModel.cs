using SchoolFlow.Desktop.Core;
using SchoolFlow.Desktop.Services.Interfaces;
using System.Windows.Input;

namespace SchoolFlow.Desktop.ViewModels;

/// <summary>
/// ViewModel pour la page de connexion.
/// </summary>
public class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    
    private string _username = string.Empty;
    private string _password = string.Empty;
    private bool _rememberMe;

    public LoginViewModel(IAuthService authService, INavigationService navigationService)
    {
        _authService = authService;
        _navigationService = navigationService;

        LoginCommand = new AsyncRelayCommand(_ => LoginAsync(), _ => CanLogin());
    }

    #region Propriétés

    public string Username
    {
        get => _username;
        set
        {
            SetProperty(ref _username, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            SetProperty(ref _password, value);
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public bool RememberMe
    {
        get => _rememberMe;
        set => SetProperty(ref _rememberMe, value);
    }

    #endregion

    #region Commandes

    public ICommand LoginCommand { get; }

    #endregion

    #region Méthodes

    private bool CanLogin()
    {
        return !string.IsNullOrWhiteSpace(Username) && 
               !string.IsNullOrWhiteSpace(Password) && 
               !IsLoading;
    }

    private async Task LoginAsync()
    {
        try
        {
            IsLoading = true;
            ClearError();

            var result = await _authService.LoginAsync(Username, Password);

            if (result != null)
            {
                // La navigation est gérée par MainViewModel via AuthStateChanged
            }
            else
            {
                ShowError("Nom d'utilisateur ou mot de passe incorrect.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Erreur de connexion : {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public override Task OnNavigatedToAsync()
    {
        // Réinitialiser les champs
        Username = string.Empty;
        Password = string.Empty;
        ClearError();
        return Task.CompletedTask;
    }

    #endregion
}
