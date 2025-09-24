namespace Kommunist.Core.Services.Interfaces.Shared;

public interface IToolkitToastFactory
{
    IToolkitToast Make(string message);
}
