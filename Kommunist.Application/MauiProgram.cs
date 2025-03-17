using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Markup;
using Kommunist.Application.ViewModels;
using Kommunist.Core.Configuration;
using Kommunist.Core.Services;
using Kommunist.Core.Services.Interfaces;
using Microsoft.Extensions.Logging;
using MauiApp = Microsoft.Maui.Hosting.MauiApp;

namespace Kommunist.Application;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
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
        builder.Services.AddTransient<MainPage>();
        
        //add configuration
        builder.Services.AddHttpClientConfiguration();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}