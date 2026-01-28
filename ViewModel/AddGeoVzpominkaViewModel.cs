using GeoMemo.Models;
using GeoMemo.Services;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace GeoMemo.ViewModels;

public class AddGeoVzpominkaViewModel : INotifyPropertyChanged
{
    private readonly SQLiteManager _db;
    private readonly ILocationService _locationService;
    private readonly IGeoCoderService _geoCoder;

    public event PropertyChangedEventHandler PropertyChanged;

    private Location _location;
    public double? Latitude => _location?.Latitude;
    public double? Longitude => _location?.Longitude;

    public ObservableCollection<Obec> Obce { get; } = new();
    public Obec SelectedObec { get; set; }

    private string placeholder = "Získávám obec...";
    private string _currentObec;
    public string CurrentObec
    {
        get => string.IsNullOrEmpty(_currentObec) ? placeholder : $"Automaticky nalezená obec: {_currentObec}";
        set { _currentObec = value; OnPropertyChanged(); }
    }


    private string _popis;
    public string Popis
    {
        get => _popis;
        set { _popis = value; OnPropertyChanged(); }
    }

    private int _hodnoceni = 3;
    public int Hodnoceni
    {
        get => _hodnoceni;
        set { _hodnoceni = value; OnPropertyChanged(); }
    }

    private ImageSource _imageSource;
    public ImageSource ImageSource
    {
        get => _imageSource;
        set { _imageSource = value; OnPropertyChanged(); }
    }

    public ICommand SaveCommand { get; }
    public ICommand SelectImageCommand { get; }

    public AddGeoVzpominkaViewModel(SQLiteManager db, ILocationService locationService, IGeoCoderService geoCoder)
    {
        _db = db;
        _locationService = locationService;
        _geoCoder = geoCoder;

        SaveCommand = new Command(async () => await SaveAsync());
        SelectImageCommand = new Command(async () => await PickImageAsync());

        _locationService.LocationChanged += async (s, loc) =>
        {
            _location = loc;
            await UpdateCurrentObecAsync();
        };

        LoadData();
    }
    public void OnDisappearing()
    {
        _locationService.StopListeningAsync();
    }

    public async Task OnAppearingAsync()
    {
        await _locationService.StartListeningAsync();
    }
    private async void LoadData()
    {
        var obce = await _db.GetObceAsync();
        foreach (var o in obce)
            Obce.Add(o);
    }

    private async Task UpdateCurrentObecAsync()
    {
        if (_location == null) return;

        string? geoObec = null;
        try
        {
            geoObec = await _geoCoder.GetObecFromCoordinatesAsync(
                _location.Latitude, _location.Longitude
            );
        }
        catch
        {
            geoObec = null;
            placeholder = "Obec se nepodařilo najít...";
        }

        if (!string.IsNullOrEmpty(geoObec))
        {
            CurrentObec = geoObec;

            if (SelectedObec == null)
            {
                var match = Obce.FirstOrDefault(o => o.Nazev.Equals(geoObec, StringComparison.OrdinalIgnoreCase));
                SelectedObec = match;
            }
        }
    }

    private async Task PickImageAsync()
    {
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Vyber fotku"
            });

            if (result != null)
                ImageSource = ImageSource.FromFile(result.FullPath);
        }
        catch { }
    }

    private async Task SaveAsync()
    {
        if (_location == null)
            return;

        var vzpominka = new GeoVzpominka
        {
            Latitude = _location.Latitude,
            Longitude = _location.Longitude,
            NazevObce = SelectedObec?.Nazev ?? _currentObec ?? "Neurčeno",
            Hodnoceni = Hodnoceni,
            Popis = Popis,
            ImagePath = ImageSource is FileImageSource fis ? fis.File : null,
            CreatedAt = DateTime.UtcNow
        };

        await _db.AddGeoVzpominkaAsync(vzpominka);

        await Application.Current.MainPage.DisplayAlert("Hotovo", "Vzpomínka byla uložena", "OK");

        await Application.Current.MainPage.Navigation.PopAsync();
    }

    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
