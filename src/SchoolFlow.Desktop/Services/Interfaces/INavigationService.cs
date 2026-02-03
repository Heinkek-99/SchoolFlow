using SchoolFlow.Desktop.Core;

namespace SchoolFlow.Desktop.Services.Interfaces;

/// <summary>
/// Interface pour la navigation entre ViewModels.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// ViewModel actuellement affiché.
    /// </summary>
    BaseViewModel? CurrentViewModel { get; }

    /// <summary>
    /// Événement déclenché lors d'un changement de vue.
    /// </summary>
    event Action? ViewChanged;

    /// <summary>
    /// Navigue vers un ViewModel spécifique.
    /// </summary>
    Task NavigateToAsync<TViewModel>() where TViewModel : BaseViewModel;

    /// <summary>
    /// Navigue vers un ViewModel avec un paramètre.
    /// </summary>
    Task NavigateToAsync<TViewModel>(object parameter) where TViewModel : BaseViewModel;

    /// <summary>
    /// Retourne à la vue précédente.
    /// </summary>
    Task GoBackAsync();

    /// <summary>
    /// Indique si un retour arrière est possible.
    /// </summary>
    bool CanGoBack { get; }
}
