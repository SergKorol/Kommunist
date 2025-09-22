namespace Kommunist.Application.Services;

public class MauiMainPageDialog : IMainPageDialog
{
    public async Task<string?> DisplayActionSheet(string title, string cancel, string destruction, string[] buttons)
    {
        var mainPage = Microsoft.Maui.Controls.Application.Current?.MainPage
            ?? throw new InvalidOperationException("Application.Current.MainPage is not available.");

        return await mainPage.DisplayActionSheet(title, cancel, destruction, buttons);
    }
}
