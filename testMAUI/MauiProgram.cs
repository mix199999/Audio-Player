using CommunityToolkit.Maui.Storage;

using CommunityToolkit.Maui;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using Newtonsoft.Json;

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
            });
        builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);
        builder.Services.AddTransient<MainPage>();


        var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appSettings.json");
        if (!File.Exists(appSettingsPath))
        {
            var foldersSettings = new Configuration
            {
                FolderList = new List<string> { Environment.GetFolderPath(Environment.SpecialFolder.MyMusic) },
                AudioPlaylists = new List<AudioPlaylist> { new AudioPlaylist { Name = "", Path = "" } }
            };
            var json = JsonConvert.SerializeObject(foldersSettings, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(appSettingsPath, json);
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