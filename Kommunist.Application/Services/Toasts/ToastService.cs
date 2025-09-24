using CommunityToolkit.Maui.Alerts;
using Kommunist.Core.Services.Interfaces;
using Kommunist.Core.Services.Interfaces.Shared;

namespace Kommunist.Application.Services.Toasts;

public class ToastService : IToastService
{
    public async Task ShowAsync(string message)
    {
        await Toast.Make(message).Show();
    }
}
