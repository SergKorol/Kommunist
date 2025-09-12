namespace Kommunist.Application.Helpers;

public static class ServiceHelper
{
    private static IServiceProvider? _services;
    
    public static void Initialize(IServiceProvider services)
        => _services = services;

    private static IServiceProvider Current
        => _services
           ?? Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services
           ?? throw new InvalidOperationException("MAUI Services are not available.");


    public static T Get<T>() where T : notnull
        => Current.GetService<T>()
           ?? throw new InvalidOperationException($"Service {typeof(T)} not found.");
}