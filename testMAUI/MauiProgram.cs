using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace testMAUI;

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
		builder.Services.AddSingleton<IFileSaver>(FileSaver.Default);
		builder.Services.AddTransient<MainPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

        var configuration = new ConfigurationBuilder()
		.AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
		.Build();
        var folders = configuration.GetSection("folders").Get<List<string>>();
        return builder.Build();
	}
}
