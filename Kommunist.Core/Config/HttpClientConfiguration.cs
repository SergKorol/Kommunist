using System;
using Kommunist.Core.Services;
using Kommunist.Core.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Kommunist.Core.Config;

public static class HttpClientConfiguration
{
    public static IServiceCollection AddHttpClientConfiguration(this IServiceCollection services)
    {
        services.AddHttpClient<IEventService, EventService>(client => client.BaseAddress = new Uri("https://wearecommunity.io"));
        services.AddHttpClient<ISearchService, SearchService>(client => client.BaseAddress = new Uri("https://wearecommunity.io"));
        services.AddSingleton<IFileHostingService, FileHostingService>();
        services.AddSingleton<IEmailService, EmailService>();
        return services;
    }
}