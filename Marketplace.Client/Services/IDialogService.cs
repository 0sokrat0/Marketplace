namespace Marketplace.Client.Services;

public interface IDialogService
{
    bool? ShowLogin();

    void ShowInfo(string message);

    void ShowError(string message);

    bool Confirm(string message);
}
