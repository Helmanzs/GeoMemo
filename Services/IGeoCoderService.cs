namespace GeoMemo.Services;

public interface IGeoCoderService
{
    Task<string?> GetObecFromCoordinatesAsync(double latitude, double longitude);
}
