using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CommunityToolkit.Maui;
using Kommunist.Application.Helpers;
using Kommunist.Application.Services;
using Kommunist.Application.Services.Dialog;
using Kommunist.Application.Services.File;
using Kommunist.Application.Services.Launch;
using Kommunist.Application.Services.Toasts;
using Kommunist.Application.ViewModels;
using Kommunist.Core.Config;
using Kommunist.Core.Services;
using Kommunist.Core.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Plugin.Maui.CalendarStore;
using MauiApp = Microsoft.Maui.Hosting.MauiApp;

namespace Kommunist.Application;

public static class MauiProgram
{
    [SuppressMessage("ReSharper", "ConditionalAccessQualifierIsNonNullableAccordingToAPIContract")]
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        ConfigureMauiApp(builder);
        ConfigureServices(builder);
        ConfigureLogging(builder);
        ConfigureConfiguration(builder);
        
        builder.Services.AddSingleton(CalendarStore.Default);
        
        var app = builder.Build();
        ServiceHelper.Initialize(app.Services);
        
        EntryHandler.Mapper.AppendToMapping("BorderlessEntry", ([SuppressMessage("ReSharper", "UnusedParameter.Local")] handler, view) =>
        {
            if (view is Entry e && e.StyleClass?.Contains("borderless") == true)
            {
#if ANDROID
                var pv = handler.PlatformView;
                pv.Background = null;
                pv.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);

                int h = (int)Android.Util.TypedValue.ApplyDimension(
                    Android.Util.ComplexUnitType.Dip, 12, pv.Context.Resources.DisplayMetrics);
                int v = (int)Android.Util.TypedValue.ApplyDimension(
                    Android.Util.ComplexUnitType.Dip, 8, pv.Context.Resources.DisplayMetrics);
                pv.SetPadding(h, v, h, v);
#endif
#if IOS
                handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
#if WINDOWS
                var tb = handler.PlatformView;
                tb.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
                tb.Background =
                    new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
#endif
            }
        });
        
        return builder.Build();
    }

    private static void ConfigureMauiApp(MauiAppBuilder builder)
    {
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(ConfigureFonts);
    }

    private static void ConfigureFonts(IFontCollection fonts)
    {
        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        fonts.AddFont("Font Awesome 7 Brands-Regular-400.otf", "FAB");
        fonts.AddFont("Font Awesome 7 Free-Regular-400.otf", "FAR");
        fonts.AddFont("Font Awesome 7 Free-Solid-900.otf", "FAS");
    }

    private static void ConfigureServices(MauiAppBuilder builder)
    {
        builder.Services.AddScoped<IEventService, EventService>();
        
        builder.Services.AddScoped<EventCalendarViewModel>();
        builder.Services.AddScoped<EventCalendarDetailViewModel>();
        builder.Services.AddScoped<CalConfigViewModel>();
        builder.Services.AddScoped<EventFiltersViewModel>();
        
        builder.Services.AddSingleton<IToastService, ToastService>();
        builder.Services.AddSingleton<IFileSaverService, FileSaverService>();
        builder.Services.AddSingleton<IFileSystemService, FileSystemService>();
        builder.Services.AddSingleton<ILauncherService, LauncherService>();
        builder.Services.AddSingleton<IPageDialogService, PageDialogService>();
        
        builder.Services.AddTransient<MainPage>();
        
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

    private static IConfiguration? LoadConfiguration()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var environmentName = GetEnvironmentName();
        const string baseName = "Kommunist.Application.appsettings";
        var envResourceName = string.Concat(baseName, ".", environmentName, ".json");
        var defaultResourceName = string.Concat(baseName, ".json");

        using var stream = assembly.GetManifestResourceStream(envResourceName)
                          ?? assembly.GetManifestResourceStream(defaultResourceName);
        return stream != null ? new ConfigurationBuilder().AddJsonStream(stream).Build() : null;
    }

    private static string GetEnvironmentName()
    {
        return Environment.GetEnvironmentVariable("MAUI_ENVIRONMENT") ?? "Development";
    }
}