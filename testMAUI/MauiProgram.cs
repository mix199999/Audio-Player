using CommunityToolkit.Maui.Storage;

using CommunityToolkit.Maui;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Newtonsoft.Json;
using System.IO;

namespace testMAUI;

public static class MauiProgram
{

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Roboto-Regular.ttf", "RobotoRegular");
                fonts.AddFont("Roboto-Bold.ttf", "RobotoBold");
            });
        builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);
        builder.Services.AddTransient<MainPage>();

        var config_path = Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "GNOM");

        if (!Directory.Exists(config_path))
        {
            Directory.CreateDirectory(config_path);
        }

        // var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appSettings.json");
        var appSettingsPath = Path.Combine(config_path, "appSettings.json");
        // var favoriteSongsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favoritesongs.M3U");
        var favoriteSongsPath = Path.Combine(config_path, "favoritesongs.M3U");

        if (!File.Exists(appSettingsPath))
        {
            var foldersSettings = new Configuration
            {
                FolderList = new List<string> { Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) },
                AudioPlaylists = new List<AudioPlaylist> { new AudioPlaylist { Name = "Favorite Songs", Path = favoriteSongsPath } },
                FirstTimeRun = true, 
                Theme = new Theme { Gradient = false, PrimaryColor = "ffffff", SecondaryColor = "000000", GradientColor = "ffffff", Flip = false, HtoV = false, DarkButtons = true }
            };
            var json = JsonConvert.SerializeObject(foldersSettings, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(appSettingsPath, json);

            File.WriteAllText(favoriteSongsPath, "#EXTM3U\n");
        }

        using var stream = File.Open(appSettingsPath, FileMode.Open);
        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();

        builder.Configuration.AddConfiguration(config);



#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }


}