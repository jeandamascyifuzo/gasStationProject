using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Escale.mobile.Models;
using Escale.mobile.Services;

namespace Escale.mobile.ViewModels;

public partial class StationSelectionViewModel : ObservableObject
{
    [ObservableProperty]
    private List<StationInfo> stations = new();

    [ObservableProperty]
    private bool rememberChoice;

    public StationSelectionViewModel()
    {
        LoadStations();
        RememberChoice = Preferences.Get("RememberStationChoice", false);
    }

    private void LoadStations()
    {
        var user = AppState.Instance.CurrentUser;
        if (user != null)
        {
            Stations = user.AssignedStations;
        }
    }

    [RelayCommand]
    private async Task SelectStation(StationInfo station)
    {
        AppState.Instance.SetStation(station);

        if (RememberChoice)
        {
            Preferences.Set("RememberStationChoice", true);
            Preferences.Set("PreferredStationId", station.Id.ToString());
        }

        await Shell.Current.GoToAsync("///Dashboard");
    }
}
