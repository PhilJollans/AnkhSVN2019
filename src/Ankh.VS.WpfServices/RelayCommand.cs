using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ankh.VS.WpfServices
{
  /// <summary>
  /// This RelayCommand implementation is strongly oriented
  /// to the RelayCommand implementation of the Prism framework.
  /// </summary>
  /// <seealso cref="MultiLanguage2019.RelayCommandBase" />
  public class RelayCommand : RelayCommandBase
  {

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand"/> class only with an execute delegate.
    /// </summary>
    /// <param name="executeMethod">The execute method.</param>
    public RelayCommand(Action executeMethod)
          : this(executeMethod, () => true)
    {
      //empty
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand"/> class with execute and canexecute delegate.
    /// </summary>
    /// <param name="executeMethod">The execute method.</param>
    /// <param name="canExecuteMethod">The can execute method.</param>
    /// <exception cref="ArgumentNullException">executeMethod</exception>
    public RelayCommand(Action executeMethod, Func<bool> canExecuteMethod)
       : base((o) => executeMethod(), (o) => canExecuteMethod())
    {
      if (executeMethod == null || canExecuteMethod == null)
        throw new ArgumentNullException(nameof(executeMethod));
    }


    /// <summary>
    /// Gets or sets the execute method.
    /// This gives the possibility to modify the delegate,
    /// change it or add another delegate.
    /// </summary>
    public Action<object> ExecuteMethod
    {
      get
      {
        return base._executeMethod;
      }
      set
      {
        base._executeMethod = value;
      }
    }



    /// <summary>
    /// This is Obsolete, do not use if possible!
    /// Initializes a new instance of the <see cref="RelayCommand"/> class.
    /// </summary>
    /// <param name="executeMethod">The execute method.</param>
    /// <exception cref="ArgumentNullException">executeMethod</exception>
    [Obsolete("Don't use this type-unsafe implementation if possible. It's better to uUse the type-safe RelayCommand<T> !")]
    public RelayCommand(Action<object> executeMethod)
          : base((o) => executeMethod(o), (o) => true)
    {
      if (executeMethod == null)
        throw new ArgumentNullException(nameof(executeMethod));
    }


    /// <summary>
    /// This is Obsolete, do not use if possible!
    /// Initializes a new instance of the <see cref="RelayCommand"/> class.
    /// </summary>
    /// <param name="executeMethod">The execute method.</param>
    /// <param name="canExecuteMethod">The can execute method.</param>
    /// <exception cref="ArgumentNullException">executeMethod</exception>
    [Obsolete("Don't use this type-unsafe implementation if possible. It's better to uUse the type-safe RelayCommand<T> !")]
    public RelayCommand(Action<object> executeMethod, Func<object, bool> canExecuteMethod)
       : base((o) => executeMethod(o), (o) => canExecuteMethod(o))
    {
      if (executeMethod == null || canExecuteMethod == null)
        throw new ArgumentNullException(nameof(executeMethod));
    }

    /// <summary>
    /// Executes this instance.
    /// </summary>
    public virtual void Execute()
    {
      Execute(null);
    }

    /// <summary>
    /// Determines whether this instance can execute.
    /// </summary>
    /// <returns>
    ///   <c>true</c> if this instance can execute; otherwise, <c>false</c>.
    /// </returns>
    public virtual bool CanExecute()
    {
      return CanExecute(null);
    }

  }

  /// <summary>
  /// This is the generic RelayCommand implementation, strongly oriented
  /// to the RelayCommand implementation of the Prism framework.
  /// </summary>
  /// <seealso cref="MultiLanguage2019.RelayCommandBase" />
  public class RelayCommand<T> : RelayCommandBase
  {
    public RelayCommand(Action<T> executeMethod)
         : this(executeMethod, (o) => true)
    {
      //empty
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
    /// </summary>
    /// <param name="executeMethod">The execute method.</param>
    /// <param name="canExecuteMethod">The can execute method.</param>
    /// <exception cref="ArgumentNullException">executeMethod</exception>
    public RelayCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
           : base((o) => executeMethod((T)o), (o) => canExecuteMethod((T)o))
    {
      if (executeMethod == null || canExecuteMethod == null)
        throw new ArgumentNullException(nameof(executeMethod));
    }

    /// <summary>
    /// Executes with the generic parameter.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    public virtual void Execute(T parameter)
    {
      Execute(parameter);
    }

    /// <summary>
    /// Determines whether this instance can execute with the generic parameter.
    /// </summary>
    /// <param name="parameter">The parameter.</param>
    /// <returns>
    ///   <c>true</c> if this instance can execute the specified parameter; otherwise, <c>false</c>.
    /// </returns>
    public virtual bool CanExecute(T parameter)
    {
      return base.CanExecute(parameter);
    }

  }


}
