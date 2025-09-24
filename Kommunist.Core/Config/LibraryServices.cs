using Kommunist.Core.Services;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Core.Config;

public static class LibraryServices
{
    public static void AddLibraryServices(this IServiceCollection services)
    {
        services.AddSingleton<IFileHostingService, FileHostingService>();
        services.AddSingleton<IEmailService, EmailService>();
        services.AddSingleton<IFilterService, FilterService>();
        services.AddSingleton<ICoordinatesService, CoordinatesService>();
        services.AddSingleton<IAndroidCalendarService, AndroidCalendarService>();
    }
}