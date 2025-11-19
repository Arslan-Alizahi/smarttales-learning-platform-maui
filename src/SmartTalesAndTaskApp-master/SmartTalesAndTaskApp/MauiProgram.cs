using Microsoft.Extensions.Logging;
using SmartTalesAndTaskApp.Services;
using Plugin.Maui.Audio;
using CommunityToolkit.Maui;

namespace SmartTalesAndTaskApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();
            builder.Services.AddSingleton<OcrService>();
            // Register the audio manager
            builder.Services.AddSingleton(AudioManager.Current);


#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
            #endif

            return builder.Build();
        }
    }
}
