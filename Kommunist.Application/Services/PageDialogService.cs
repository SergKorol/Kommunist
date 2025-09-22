using Kommunist.Core.Services.Interfaces;

namespace Kommunist.Application.Services;

public class PageDialogService(IMainPageDialog mainPageDialog) : IPageDialogService
{
    private readonly IMainPageDialog _mainPageDialog = mainPageDialog ?? throw new ArgumentNullException(nameof(mainPageDialog));

    public PageDialogService()
        : this(new MauiMainPageDialog())
    {
    }

    public async Task<string?> DisplayActionSheet(string title, string cancel, string destruction, string[] buttons)
    {
        return await _mainPageDialog.DisplayActionSheet(title, cancel, destruction, buttons);
    }
}
