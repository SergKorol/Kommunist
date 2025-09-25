namespace Kommunist.Core.Services.Interfaces.Shared;
public interface IPageDialogService
{
    Task<string?> DisplayActionSheet(string title, string cancel, string? destruction, string[] buttons);
}
