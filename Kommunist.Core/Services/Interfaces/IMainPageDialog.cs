using System.Threading.Tasks;

namespace Kommunist.Application.Services;

public interface IMainPageDialog
{
    Task<string?> DisplayActionSheet(string title, string cancel, string destruction, string[] buttons);
}
