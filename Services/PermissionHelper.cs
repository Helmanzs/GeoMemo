using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;

namespace GeoMemo.Services
{
    // Zajistí požádání o oprávnění k poloze na hlavním vlákně
    public static class PermissionHelper
    {
        public static async Task<bool> RequestLocationPermissionAsync()
        {
            return await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

                if (status != PermissionStatus.Granted)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Chyba",
                        "Nemáte povolený přístup k poloze. Některé funkce aplikace nebudou fungovat.",
                        "OK"
                    );
                    return false;
                }

                return true;
            });
        }
    }
}
