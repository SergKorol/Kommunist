using System.ComponentModel;
using System.Runtime.CompilerServices;
using XCalendar.Core.Interfaces;

namespace Kommunist.Application.Models;

public abstract class BaseObservableModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}