using CommunityToolkit.Maui.Alerts;
using Kommunist.Core.Services.Interfaces;
namespace Kommunist.Application.Services;

public class ToastService : IToastService
{
    public async Task ShowAsync(string message)
    {
        await Toast.Make(message).Show();
    }
}
