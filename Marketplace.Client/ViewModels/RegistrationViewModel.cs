using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Marketplace.Client.Services;
using Marketplace.Contracts;

namespace Marketplace.Client.ViewModels;

public partial class RegistrationViewModel(ApiClient api, SessionService session) : ViewModelBase
{
    [ObservableProperty]
    private string _login = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string RepeatPassword { get; set; } = string.Empty;

    public event Action<bool>? CloseRequested;

    [RelayCommand]
    private Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Заполните логин и пароль";
            return Task.CompletedTask;
        }

        if (Password != RepeatPassword)
        {
            ErrorMessage = "Пароли не совпадают";
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            var response = await api.Register(new RegisterRequest(Login, Password, RepeatPassword));
            session.SignIn(response);
            CloseRequested?.Invoke(true);
        });
    }
}
