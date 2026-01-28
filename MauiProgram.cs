using GeoMemo.Services;
using GeoMemo.ViewModels;
using Microsoft.Extensions.Logging;

namespace GeoMemo
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            builder.Services.AddSingleton<SQLiteManager>(sp =>
            {
                var dbPath = Path.Combine(
                    FileSystem.AppDataDirectory,
                    "geovzpominky.db3"
                );

                return new SQLiteManager(dbPath);
            });
            builder.Services.AddSingleton<ILocationService, LocationService>();
            builder.Services.AddSingleton<IGeoCoderService, NominatimService>();
            builder.Services.AddTransient<MainPageViewModel>();

            return builder.Build();
        }
    }
}
