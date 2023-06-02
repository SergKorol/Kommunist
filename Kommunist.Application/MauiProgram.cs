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
            });
        
        //register services
        builder.Services.AddScoped<IEventService, EventService>();
        builder.Services.AddScoped<EventCalendarViewModel>();
        builder.Services.AddTransient<MainPage>();
        // builder.Services.AddHttpClient<IEventService, EventService>(client => client.BaseAddress = new Uri("https://wearecommunity.io"));
        
        //add configuration
        builder.Services.AddHttpClientConfiguration();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}