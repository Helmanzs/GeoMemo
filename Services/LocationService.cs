using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;
using System.Timers;

namespace GeoMemo.Services;

// Služba pro získávání aktuální polohy a sledování změn polohy uživatele
public class LocationService : ILocationService
{
    private System.Timers.Timer _timer;
    private Location? _lastLocation;

    public event EventHandler<Location>? LocationChanged;

    public LocationService()
    {
        _timer = new System.Timers.Timer(5000);
        _timer.Elapsed += async (s, e) => await OnTimerElapsed();
    }

    // Časovač, který pravidelně získává aktuální polohu a vyvolává událost při změně (např. kdyby uživatel
    // seřazoval podle toho, co je k němu nejblíže a pohyboval se přitom)
    private async Task OnTimerElapsed()
    {
        var loc = await GetCurrentLocationAsync();
        if (loc != null)
        {
            if (_lastLocation == null ||
                GetDistance(_lastLocation.Latitude, _lastLocation.Longitude, loc.Latitude, loc.Longitude) > 1)
            {
                _lastLocation = loc;
                LocationChanged?.Invoke(this, loc);
            }
        }
    }

    // získání aktuální polohy uživatele
    public async Task<Location?> GetCurrentLocationAsync()
    {
        PermissionHelper.RequestLocationPermissionAsync().Wait();

        try
        {
            var request = new GeolocationRequest(
                GeolocationAccuracy.Best,
                TimeSpan.FromSeconds(10)
            );

            return await Geolocation.GetLocationAsync(request);
        }
        catch
        {
            return null;
        }
    }

    public Task StartListeningAsync()
    {
        _timer.Start();
        return Task.CompletedTask;
    }

    public Task StopListeningAsync()
    {
        _timer.Stop();
        return Task.CompletedTask;
    }

    // výpočet vzdálenosti mezi dvěma body na Zemi pomocí Haversineovy formule
    public double GetDistance(double? lat1, double? lon1, double? lat2, double? lon2)
    {
        if (!lat1.HasValue || !lon1.HasValue || !lat2.HasValue || !lon2.HasValue)
            return double.MaxValue;

        double R = 6371000;
        double dLat = ToRad(lat2.Value - lat1.Value);
        double dLon = ToRad(lon2.Value - lon1.Value);
        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                   Math.Cos(ToRad(lat1.Value)) * Math.Cos(ToRad(lat2.Value)) *
                   Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRad(double deg) => deg * Math.PI / 180;
}
