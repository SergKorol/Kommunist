using CommunityToolkit.Maui.Alerts;
using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Application.Services;

public sealed class MauiToastService : IToastService
{
    public Task ShowAsync(string message)
    {
        return Toast.Make(message).Show();
    }
}
