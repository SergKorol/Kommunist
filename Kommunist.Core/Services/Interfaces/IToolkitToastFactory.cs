namespace Kommunist.Core.Services.Interfaces;

public interface IToolkitToastFactory
{
    IToolkitToast Make(string message);
}
