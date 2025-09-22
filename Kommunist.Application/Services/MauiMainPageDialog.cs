namespace Kommunist.Application.Services;

public class MauiMainPageDialog : IMainPageDialog
{
    public async Task<string?> DisplayActionSheet(string title, string cancel, string destruction, string[] buttons)
    {
        var app = Microsoft.Maui.Controls.Application.Current
            ?? throw new InvalidOperationException("Application.Current is not available.");
        var mainPage = app.Windows.Count > 0
            ? app.Windows[0].Page
            : throw new InvalidOperationException("No application windows are available.");

        if (mainPage != null) return await mainPage.DisplayActionSheet(title, cancel, destruction, buttons);
        throw new InvalidOperationException("Main page is not available.");
    }
}
