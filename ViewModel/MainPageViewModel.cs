using GeoMemo.Models;
using GeoMemo.Services;
using Microsoft.Maui.Devices.Sensors;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace GeoMemo.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    private readonly SQLiteManager _db;
    private readonly ILocationService _locationService;
    public event PropertyChangedEventHandler PropertyChanged;
    public ICommand AddGeoVzpominkaCommand { get; }
    public ICommand RemoveGeoVzpominkaCommand { get; }
    public ICommand ShowSortOptionsCommand { get; }

    private enum SortMode
    {
        Hodnoceni,
        Blizko,
        Abecedne
    }

    private SortMode _currentSort = SortMode.Hodnoceni;
    private Location _location;

    public ObservableCollection<GeoVzpominka> Vzpominky { get; } = new();

    public MainPageViewModel(SQLiteManager db, ILocationService locationService)
    {
        _db = db;
        _locationService = locationService;

        // Příkaz pro přidání nové vzpomínky
        AddGeoVzpominkaCommand = new Command(async () =>
        {
            var vm = new AddGeoVzpominkaViewModel(_db, new LocationService(), new NominatimService());
            var page = new AddGeoVzpominkaPage(vm);
            await Application.Current.MainPage.Navigation.PushAsync(page);
        });

        // Příkaz pro odstranění vzpomínky
        RemoveGeoVzpominkaCommand = new Command<GeoVzpominka>(async vzpominka => await RemoveGeoVzpominkaAsync(vzpominka));

        // Příkaz pro zobrazení možností řazení
        ShowSortOptionsCommand = new Command(async () => await ShowSortOptionsAsync());

        // sbírání notifikací o změně polohy
        _locationService.LocationChanged += (s, loc) =>
        {
            _location = loc;
            if (_currentSort == SortMode.Blizko)
                SortVzpominky();
        };
    }

    public void OnDisappearing()
    {
        _locationService.StopListeningAsync();
    }

    public async Task StartLocationAsync()
    {
        await _locationService.StartListeningAsync();
    }

    private async Task RemoveGeoVzpominkaAsync(GeoVzpominka vzpominka)
    {
        if (vzpominka == null) return;

        System.Diagnostics.Debug.WriteLine($"Chci smazat vzpomínku: {vzpominka.NazevObce}");

        bool ok = await Application.Current.MainPage.DisplayAlert(
            "Smazat",
            $"Opravdu chcete smazat vzpomínku '{vzpominka.NazevObce}'?",
            "Ano",
            "Ne"
        );

        if (!ok) return;

        try
        {
            await _db.DeleteVzpominka(vzpominka);
            Vzpominky.Remove(vzpominka);
            System.Diagnostics.Debug.WriteLine($"Vzpomínka '{vzpominka.NazevObce}' smazána.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Chyba při mazání: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Chyba", "Nepodařilo se smazat vzpomínku.", "OK");
        }
    }

    public async Task LoadAsync()
    {
        var vzpominky = await _db.GetAllVzpominkaSafe();

        var sorted = vzpominky.OrderByDescending(v => v.Hodnoceni).ToList();

        Vzpominky.Clear();
        foreach (var vz in sorted)
            Vzpominky.Add(vz);
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private async Task ShowSortOptionsAsync()
    {
        string action = await Application.Current.MainPage.DisplayActionSheet(
            "Řadit podle",
            "Zrušit",
            null,
            "Hodnocení",
            "Blízko",
            "Abecedně"
        );

        switch (action)
        {
            case "Hodnocení":
                _currentSort = SortMode.Hodnoceni;
                break;
            case "Blízko":
                _currentSort = SortMode.Blizko;
                break;
            case "Abecedně":
                _currentSort = SortMode.Abecedne;
                break;
            default:
                return;
        }

        SortVzpominky();
    }

    private void SortVzpominky()
    {
        List<GeoVzpominka> sorted = _currentSort switch
        {
            SortMode.Hodnoceni => Vzpominky.OrderByDescending(v => v.Hodnoceni).ToList(),
            SortMode.Abecedne => Vzpominky.OrderBy(v => v.NazevObce).ToList(),
            SortMode.Blizko when _location != null =>
                Vzpominky.OrderBy(v => _locationService.GetDistance(v.Latitude, v.Longitude, _location.Latitude, _location.Longitude)).ToList(),
            _ => Vzpominky.ToList()
        };

        Vzpominky.Clear();
        foreach (var vz in sorted)
            Vzpominky.Add(vz);
    }
}
