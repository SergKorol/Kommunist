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
        
        ConfigureMauiApp(builder);
        ConfigureServices(builder);
        ConfigureLogging(builder);
        ConfigureConfiguration(builder);
        
        return builder.Build();
    }

    private static void ConfigureMauiApp(MauiAppBuilder builder)
    {
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitMarkup()
            .ConfigureFonts(ConfigureFonts);
    }

    private static void ConfigureFonts(IFontCollection fonts)
    {
        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        fonts.AddFont("Brands-Regular-400.otf", "FAB");
        fonts.AddFont("Free-Regular-400.otf", "FAR");
        fonts.AddFont("Free-Solid-900.otf", "FAS");
    }

    private static void ConfigureServices(MauiAppBuilder builder)
    {
        // Core Services
        builder.Services.AddScoped<IEventService, EventService>();
        
        // ViewModels
        builder.Services.AddScoped<EventCalendarViewModel>();
        builder.Services.AddScoped<EventCalendarDetailViewModel>();
        builder.Services.AddScoped<ICalConfigViewModel>();
        builder.Services.AddScoped<EventFiltersViewModel>();
        
        // Pages
        builder.Services.AddTransient<MainPage>();
        
        // HTTP Configuration
        builder.Services.AddHttpClientConfiguration();
    }

    private static void ConfigureLogging(MauiAppBuilder builder)
    {
#if DEBUG
        builder.Logging.AddDebug();
#endif
    }

    private static void ConfigureConfiguration(MauiAppBuilder builder)
    {
        var config = LoadConfiguration();
        if (config == null) return;
        builder.Configuration.AddConfiguration(config);
        builder.Services.AddSingleton(config);
    }

    private static IConfiguration LoadConfiguration()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var environmentName = GetEnvironmentName();
        var resourceName = string.Concat("Kommunist.Application.appsettings.", environmentName, ".json");
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        return stream != null ? new ConfigurationBuilder().AddJsonStream(stream).Build() : null;
    }

    private static string GetEnvironmentName()
    {
        return Environment.GetEnvironmentVariable("MAUI_ENVIRONMENT") ?? "Development";
    }
}