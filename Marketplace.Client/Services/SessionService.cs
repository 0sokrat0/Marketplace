using CommunityToolkit.Mvvm.ComponentModel;
using Marketplace.Contracts;

namespace Marketplace.Client.Services;

public partial class SessionService(ApiClient api) : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAuthenticated), nameof(IsAdmin), nameof(DisplayName))]
    private string? _login;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAdmin))]
    private string? _role;

    public bool IsAuthenticated => Login is not null;

    public bool IsAdmin => Role == Roles.Admin;

    public string DisplayName => Login ?? "Гость";

    public void SignIn(AuthResponse response)
    {
        api.SetToken(response.Token);
        Role = response.Role;
        Login = response.Login;
    }

    public void SignOut()
    {
        api.SetToken(null);
        Login = null;
        Role = null;
    }
}
