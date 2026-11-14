using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Marketplace.Client.Services;
using Marketplace.Contracts;

namespace Marketplace.Client.ViewModels;

public partial class AdminPickupPointsViewModel(ApiClient api, IDialogService dialogs) : ViewModelBase
{
    [ObservableProperty]
    private PickupPointDto? _selectedPoint;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    public ObservableCollection<PickupPointDto> Points { get; } = [];

    public override Task LoadAsync() => RunAsync(() => ReloadAsync(null));

    partial void OnSelectedPointChanged(PickupPointDto? value)
    {
        if (value is null)
        {
            return;
        }

        Name = value.Name;
        Address = value.Address;
    }

    [RelayCommand]
    private void New()
    {
        SelectedPoint = null;
        Name = string.Empty;
        Address = string.Empty;
        ErrorMessage = null;
        StatusMessage = null;
    }

    [RelayCommand]
    private Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Address))
        {
            ErrorMessage = "Заполните название и адрес пункта выдачи";
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            var request = new PickupPointRequest(Name, Address);
            var saved = SelectedPoint is null
                ? await api.CreatePickupPoint(request)
                : await api.UpdatePickupPoint(SelectedPoint.Id, request);
            await ReloadAsync(saved.Id);
            StatusMessage = $"Пункт выдачи «{saved.Name}» сохранен";
        });
    }

    [RelayCommand]
    private Task DeleteAsync()
    {
        if (SelectedPoint is null)
        {
            ErrorMessage = "Выберите пункт выдачи в списке";
            return Task.CompletedTask;
        }

        if (!dialogs.Confirm($"Удалить пункт выдачи «{SelectedPoint.Name}»?"))
        {
            return Task.CompletedTask;
        }

        return RunAsync(async () =>
        {
            await api.DeletePickupPoint(SelectedPoint.Id);
            await ReloadAsync(null);
            New();
            StatusMessage = "Пункт выдачи удален";
        });
    }

    private async Task ReloadAsync(int? selectId)
    {
        var points = await api.GetPickupPoints();
        Points.Clear();
        foreach (var point in points)
        {
            Points.Add(point);
        }

        SelectedPoint = Points.FirstOrDefault(p => p.Id == selectId);
    }
}
