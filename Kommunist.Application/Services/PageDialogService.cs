using Kommunist.Core.Services.Interfaces;
namespace Kommunist.Application.Services;

public class PageDialogService : IPageDialogService
{
    public async Task<string?> DisplayActionSheet(string title, string cancel, string destruction, string[] buttons)
    {
        return await App.Current.MainPage.DisplayActionSheet(title, cancel, destruction, buttons);
    }
}
