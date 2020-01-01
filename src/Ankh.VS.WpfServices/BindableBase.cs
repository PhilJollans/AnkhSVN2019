using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ankh.VS.WpfServices
{
  /// <summary>
  /// Base class for (view model) classes supporting INotifyPropertyChanged
  /// </summary>
  public class BindableBase : INotifyPropertyChanged
  {

    public BindableBase()
    {
    }

    /// <summary>
    /// Tritt ein, wenn sich ein Eigenschaftswert ändert.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Sets the property and raise OnPropertyChanged
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="storage">The storage.</param>
    /// <param name="value">The value.</param>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns></returns>
    protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
    {
      if (object.Equals(storage, value))
        return false;

      storage = value;
      this.OnPropertyChanged(propertyName);

      return true;
    }

    /// <summary>
    /// Called when property changed.
    /// </summary>
    /// <param name="propertyName">Name of the property (Auto detected by [CallerMemberName]).</param>
    protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
    {
      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

  }
}
