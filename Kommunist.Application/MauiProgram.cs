using System.Reflection;
using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Kommunist.Application.ViewModels;
using Kommunist.Core.Config;
using Kommunist.Core.Services;
using Kommunist.Core.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MauiApp = Microsoft.Maui.Hosting.MauiApp;

namespace Kommunist.Application;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        var assembly = Assembly.GetExecutingAssembly();
        var configurationBuilder = new ConfigurationBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMarkup()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("Brands-Regular-400.otf", "FAB");
                fonts.AddFont("Free-Regular-400.otf", "FAR");
                fonts.AddFont("Free-Solid-900.otf", "FAS");
            });
        
        builder.ConfigureMauiHandlers(handlers =>
        {
#if IOS
            Microsoft.Maui.Handlers.ImageHandler.Mapper.AppendToMapping(nameof(Image), (handler, view) =>
            {
                if (handler.PlatformView?.Image != null)
                {
                    handler.PlatformView.Image = handler.PlatformView.Image.ImageWithRenderingMode(UIKit.UIImageRenderingMode.AlwaysOriginal);
                }
            });
            
            // handlers.AddHandler<WebView, CustomWebViewHandler>();
#endif
        });
        
        //register services
        builder.Services.AddScoped<IEventService, EventService>();
        builder.Services.AddScoped<EventCalendarViewModel>();
        builder.Services.AddScoped<EventCalendarDetailViewModel>();
        builder.Services.AddScoped<ICalConfigViewModel>();
        builder.Services.AddTransient<MainPage>();
        
        //add configuration
        builder.Services.AddHttpClientConfiguration();

#if DEBUG
        builder.Logging.AddDebug();
#endif
        string environmentName = Environment.GetEnvironmentVariable("MAUI_ENVIRONMENT") ?? "Development";
        var stream = assembly.GetManifestResourceStream($"Kommunist.Application.appsettings.{environmentName}.json");
        if (stream == null) return builder.Build();
        configurationBuilder.AddJsonStream(stream);
        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        builder.Configuration.AddConfiguration(config);

        builder.Services.AddSingleton<IConfiguration>(config);


        return builder.Build();
    }
}