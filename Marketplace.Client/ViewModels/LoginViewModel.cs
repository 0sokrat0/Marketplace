using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Marketplace.Client.Services;
using Marketplace.Contracts;

namespace Marketplace.Client.ViewModels;

public partial class LoginViewModel(ApiClient api, SessionService session) : ViewModelBase
{
    [ObservableProperty]
    private string _login = string.Empty;

    public string Password { get; set; } = string.Empty;

    public event Action<bool>? CloseRequested;

    [RelayCommand]
    private Task SignInAsync()
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Введите логин и пароль";
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            var response = await api.Login(new LoginRequest(Login, Password));
            session.SignIn(response);
            CloseRequested?.Invoke(true);
        });
    }
}
