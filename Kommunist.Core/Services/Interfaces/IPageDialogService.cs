namespace Kommunist.Core.Services.Interfaces;
public interface IPageDialogService
{
    Task<string?> DisplayActionSheet(string title, string cancel, string destruction, string[] buttons);
}
