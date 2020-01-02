using System;
using System.Diagnostics;
using System.Windows.Input;


namespace Ankh.VS.WpfServices
{

  /// <summary>
  /// Base class of RelayCommand with implements the ICommand interface.
  /// </summary>
  /// <seealso cref="System.Windows.Input.ICommand" />
  public class RelayCommandBase : ICommand
  {
    protected          Action<object>     _executeMethod;
    protected readonly Func<object, bool> _canExecuteMethod;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommandBase"/> class.
    /// </summary>
    /// <param name="execute">The execute.</param>
    /// <param name="canExecute">The can execute.</param>
    /// <exception cref="ArgumentNullException">
    /// execute
    /// or
    /// canexecute
    /// </exception>
    protected RelayCommandBase(Action<object> execute, Func<object, bool> canExecute)
    {
      _executeMethod    = execute ?? throw new ArgumentNullException("execute");
      _canExecuteMethod = canExecute ?? throw new ArgumentNullException("canexecute");
    }

    /// <summary>
    /// Pure Optional! Gets or sets the display text of this relaycommand.
    /// It can be used for Button, Menuitems, etc. .
    /// </summary>
    public string DisplayText { get; set; }


    protected virtual bool CanExecute(object parameter)
    {
      return _canExecuteMethod == null ? true : _canExecuteMethod(parameter);
    }


    protected virtual void Execute(object parameter)
    {
      _executeMethod(parameter);
    }

    //---------------------------------------------------------------
    // ICommand interface
    //---------------------------------------------------------------

    [DebuggerStepThrough]
    bool ICommand.CanExecute(object parameter)
    {
      return CanExecute(parameter);
    }

    [DebuggerStepThrough]
    void ICommand.Execute(object parameter)
    {
      _executeMethod(parameter);
    }

    public event EventHandler CanExecuteChanged
    {
      add
      {
        CommandManager.RequerySuggested += value;
      }
      remove
      {
        CommandManager.RequerySuggested -= value;
      }
    }

  }

}
