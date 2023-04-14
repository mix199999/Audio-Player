﻿using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

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

		//using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("testMAUI.appSettings.json");
		//var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
		//builder.Configuration.AddConfiguration(config);

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
