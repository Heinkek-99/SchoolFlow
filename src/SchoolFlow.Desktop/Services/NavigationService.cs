using Microsoft.Extensions.DependencyInjection;
using SchoolFlow.Desktop.Core;
using SchoolFlow.Desktop.Services.Interfaces;

namespace SchoolFlow.Desktop.Services;

/// <summary>
/// Service de navigation entre ViewModels.
/// </summary>
public class NavigationService : ObservableObject, INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<BaseViewModel> _navigationStack = new();
    private BaseViewModel? _currentViewModel;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public BaseViewModel? CurrentViewModel
    {
        get => _currentViewModel;
        private set
        {
            SetProperty(ref _currentViewModel, value);
            ViewChanged?.Invoke();
        }
    }

    public event Action? ViewChanged;

    public bool CanGoBack => _navigationStack.Count > 0;

    public async Task NavigateToAsync<TViewModel>() where TViewModel : BaseViewModel
    {
        await NavigateToAsync<TViewModel>(null!);
    }

    public async Task NavigateToAsync<TViewModel>(object parameter) where TViewModel : BaseViewModel
    {
        if (CurrentViewModel != null)
        {
            await CurrentViewModel.OnNavigatedFromAsync();
            _navigationStack.Push(CurrentViewModel);
        }

        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        
        if (viewModel is IParameterReceiver receiver && parameter != null)
        {
            receiver.ReceiveParameter(parameter);
        }

        CurrentViewModel = viewModel;
        await viewModel.OnNavigatedToAsync();
    }

    public async Task GoBackAsync()
    {
        if (!CanGoBack) return;

        if (CurrentViewModel != null)
        {
            await CurrentViewModel.OnNavigatedFromAsync();
        }

        CurrentViewModel = _navigationStack.Pop();
        await CurrentViewModel.OnNavigatedToAsync();
    }
}

/// <summary>
/// Interface pour les ViewModels qui reçoivent des paramètres.
/// </summary>
public interface IParameterReceiver
{
    void ReceiveParameter(object parameter);
}
