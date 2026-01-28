using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Devices.Sensors;

namespace GeoMemo.ViewModels;

public class MainPageViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private Location _location;
    private string _status = "Zjišťuji polohu…";

    public double? Latitude => _location?.Latitude;
    public double? Longitude => _location?.Longitude;
    public double? Accuracy => _location?.Accuracy;

    public string Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
        }
    }

    public async Task LoadAsync()
    {
        await GetLocationAsync();
    }

    private async Task GetLocationAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }


        try
        {
            var request = new GeolocationRequest(
                GeolocationAccuracy.Medium,
                TimeSpan.FromSeconds(10)
            );

            _location = await Geolocation.GetLocationAsync(request);

            if (_location == null)
            {
                Status = "Polohu se nepodařilo zjistit.";
                return;
            }

            Status = "Aktuální poloha:";
            OnPropertyChanged(nameof(Latitude));
            OnPropertyChanged(nameof(Longitude));
            OnPropertyChanged(nameof(Accuracy));
        }
        catch (Exception ex)
        {
            Status = "Chyba: " + ex.Message;
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}


/*public class MainPageViewModel
{
    private readonly SQLiteManager _db;

    public ObservableCollection<Obec> Obce { get; } = new();

    public MainPageViewModel(SQLiteManager database)
    {
        _db = database;
    }

    public async Task LoadAsync()
    {
        await _db.InitAsync();
        var obce = await _db.GetObceAsync();

        Obce.Clear();
        foreach (var obec in obce)
            Obce.Add(obec);
    }

}*/
