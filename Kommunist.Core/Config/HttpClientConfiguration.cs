using Kommunist.Core.Services;
using Kommunist.Core.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Kommunist.Core.Config;

public static class HttpClientConfiguration
{
    public static IServiceCollection AddHttpClientConfiguration(this IServiceCollection services)
    {
        services.AddHttpClient<IEventService, EventService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var baseUrl = config["ApiBaseUrl"] ?? "https://wearecommunity.io";
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddHttpClient<ISearchService, SearchService>((sp, client) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var baseUrl = config["ApiBaseUrl"] ?? "https://wearecommunity.io";
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddSingleton<IFileHostingService, FileHostingService>();
        services.AddSingleton<IEmailService, EmailService>();
        services.AddSingleton<IFilterService, FilterService>();
        services.AddSingleton<ICoordinatesService, CoordinatesService>();
        return services;
    }
}