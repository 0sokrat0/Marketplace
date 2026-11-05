using Marketplace.Client.Services;

namespace Marketplace.Client.ViewModels;

public class WelcomeViewModel(SessionService session) : ViewModelBase
{
    public SessionService Session { get; } = session;
}
