using Microsoft.Maui.Devices.Sensors;

namespace GeoMemo.Services;

public interface ILocationService
{
    Task<Location> GetCurrentLocationAsync();
    event EventHandler<Location> LocationChanged;
    Task StartListeningAsync();
    Task StopListeningAsync();
    double GetDistance(double? lat1, double? lon1, double? lat2, double? lon2);
}
