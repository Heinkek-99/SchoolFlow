using Microsoft.Extensions.DependencyInjection;
using SchoolFlow.Desktop.Services;
using SchoolFlow.Desktop.Services.Interfaces;
using SchoolFlow.Desktop.ViewModels;
using SchoolFlow.Desktop.Views;
using System.Windows;

namespace SchoolFlow.Desktop;

public partial class App : System.Windows.Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Configuration - URL de l'API
        var apiBaseUrl = Environment.GetEnvironmentVariable("SCHOOLFLOW_API_URL") 
                         ?? "https://localhost:7001";

        // Services
        services.AddSingleton<IApiService>(new ApiService(apiBaseUrl));
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<INavigationService, NavigationService>();

        // ViewModels - Principaux
        services.AddSingleton<MainViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<FamillesViewModel>();
        services.AddTransient<ElevesViewModel>();
        services.AddTransient<PaiementsViewModel>();
        services.AddTransient<ClassesViewModel>();

        // ViewModels - Formulaires CRUD
        services.AddTransient<FamilleFormViewModel>();
        services.AddTransient<EleveFormViewModel>();

        // Views
        services.AddSingleton<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }
}
