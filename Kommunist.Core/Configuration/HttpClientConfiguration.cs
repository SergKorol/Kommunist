using Kommunist.Core.Services;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Core.Configuration;

public static class HttpClientConfiguration
{
    public static IServiceCollection AddHttpClientConfiguration(this IServiceCollection services)
    {
        services.AddHttpClient<IEventService, EventService>(client => client.BaseAddress = new Uri("https://wearecommunity.io"));
        
        return services;
    }
}