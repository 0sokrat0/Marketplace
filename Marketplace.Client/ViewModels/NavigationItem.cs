namespace Marketplace.Client.ViewModels;

public enum Access
{
    Guest,
    User,
    Admin
}

public record NavigationItem(string Title, Type PageType, Access Access);
