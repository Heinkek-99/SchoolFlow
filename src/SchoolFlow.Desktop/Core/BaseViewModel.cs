using System.Windows;

namespace SchoolFlow.Desktop.Core;

/// <summary>
/// ViewModel de base avec gestion du chargement et des erreurs.
/// </summary>
public abstract class BaseViewModel : ObservableObject
{
    private bool _isLoading;
    private string? _errorMessage;
    private bool _hasError;

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set
        {
            SetProperty(ref _errorMessage, value);
            HasError = !string.IsNullOrEmpty(value);
        }
    }

    public bool HasError
    {
        get => _hasError;
        private set => SetProperty(ref _hasError, value);
    }

    protected void ClearError() => ErrorMessage = null;

    protected void ShowError(string message)
    {
        ErrorMessage = message;
        MessageBox.Show(message, "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    protected void ShowSuccess(string message)
    {
        MessageBox.Show(message, "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    protected bool Confirm(string message, string title = "Confirmation")
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    /// <summary>
    /// Méthode appelée lors de la navigation vers ce ViewModel.
    /// </summary>
    public virtual Task OnNavigatedToAsync() => Task.CompletedTask;

    /// <summary>
    /// Méthode appelée lors de la navigation depuis ce ViewModel.
    /// </summary>
    public virtual Task OnNavigatedFromAsync() => Task.CompletedTask;
}
