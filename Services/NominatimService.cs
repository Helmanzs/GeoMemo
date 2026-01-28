using System.Net.Http.Json;
using System.Web;

namespace GeoMemo.Services;

// Nominatim umožňuje reverzní geokódování pomocí OpenStreetMap dat (tzn. z lat a lon získat název) 
public class NominatimService : IGeoCoderService
{
    private readonly HttpClient _httpClient;

    public NominatimService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://nominatim.openstreetmap.org/")
        };

        _httpClient.DefaultRequestHeaders.Add("User-Agent", "GeoMemoApp/1.0");
    }

    public async Task<string?> GetObecFromCoordinatesAsync(double latitude, double longitude)
    {
        var query = HttpUtility.ParseQueryString(string.Empty);
        query["lat"] = latitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        query["lon"] = longitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        query["format"] = "json";
        query["addressdetails"] = "1";

        var url = $"reverse?{query}";

        try
        {
            var response = await _httpClient.GetFromJsonAsync<NominatimResult>(url);
            return response?.address?.city ?? response?.address?.town ?? response?.address?.village;
        }
        catch
        {
            return null;
        }
    }

    private class NominatimResult
    {
        public Address? address { get; set; }
    }

    private class Address
    {
        public string? city { get; set; }
        public string? town { get; set; }
        public string? village { get; set; }
    }
}
