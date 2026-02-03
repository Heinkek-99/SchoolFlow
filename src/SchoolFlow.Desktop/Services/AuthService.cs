using SchoolFlow.Desktop.Services.Interfaces;
using SchoolFlow.Shared.Dtos;

namespace SchoolFlow.Desktop.Services;

/// <summary>
/// Service d'authentification JWT.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IApiService _apiService;
    private readonly ICacheService _cacheService;

    public AuthService(IApiService apiService, ICacheService cacheService)
    {
        _apiService = apiService;
        _cacheService = cacheService;
    }

    public LoginResponse? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser != null;

    public event Action<bool>? AuthStateChanged;

    public async Task<LoginResponse?> LoginAsync(string username, string password)
    {
        var loginRequest = new { Username = username, Password = password };
        var response = await _apiService.PostAsync<object, LoginResponse>("api/Auth/login", loginRequest);

        if (response != null && !string.IsNullOrEmpty(response.Token))
        {
            CurrentUser = response;
            _apiService.SetAuthToken(response.Token);
            AuthStateChanged?.Invoke(true);
        }

        return response;
    }

    public Task LogoutAsync()
    {
        CurrentUser = null;
        _apiService.ClearAuthToken();
        _cacheService.Clear();
        AuthStateChanged?.Invoke(false);
        return Task.CompletedTask;
    }

    public bool HasRole(string role)
    {
        if (CurrentUser == null) return false;
        return CurrentUser.Role?.Equals(role, StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
