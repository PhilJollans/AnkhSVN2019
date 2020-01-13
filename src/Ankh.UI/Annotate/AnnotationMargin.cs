using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Editor;

namespace Ankh.UI.Annotate
{
  /// <summary>
  /// Margin's canvas and visual definition including both size and content
  /// </summary>
  internal class AnnotationMargin : Canvas, IWpfTextViewMargin
  {
    /// <summary>
    /// Margin name.
    /// </summary>
    public const string MarginName = "AnnotationMargin";

    private IWpfTextView                _wpfTextView ;
    private AnnotateMarginViewModel     _vm ;
    private AnnotateMarginView          _view ;

    /// <summary>
    /// A value indicating whether the object is disposed.
    /// </summary>
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnnotationMargin"/> class for a given <paramref name="textView"/>.
    /// </summary>
    /// <param name="textView">The <see cref="IWpfTextView"/> to attach the margin to.</param>
    public AnnotationMargin ( IWpfTextView textView, AnnotateMarginViewModel vm )
    {
        // Store the parameters
        _wpfTextView = textView ;
        _vm          = vm ;

        // Create the WPF view
        _view = new AnnotateMarginView ( _vm ) ;
        _view.Width = 160 ;

        // Hook up to the layout changed event on the editor
        _wpfTextView.LayoutChanged += OnLayoutChanged ;

      //this.Width = 100;
      //this.ClipToBounds = true;
      //this.Background = new SolidColorBrush ( Colors.Cornsilk );

      // Add a green colored label that says "Hello AnnotationMargin"
      var label = new Label
      {
        Background = new SolidColorBrush ( Colors.WhiteSmoke ),
        Content = "Note",
      };

      this.Children.Add ( label );
    }

    #region IWpfTextViewMargin

    /// <summary>
    /// Gets the <see cref="Sytem.Windows.FrameworkElement"/> that implements the visual representation of the margin.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
    public FrameworkElement VisualElement
    {
      // Since this margin implements Canvas, this is the object which renders
      // the margin.
      get
      {
        this.ThrowIfDisposed ();
        return _view ;
      }
    }

    #endregion

    #region ITextViewMargin

    /// <summary>
    /// Gets the size of the margin.
    /// </summary>
    /// <remarks>
    /// For a horizontal margin this is the height of the margin,
    /// since the width will be determined by the <see cref="ITextView"/>.
    /// For a vertical margin this is the width of the margin,
    /// since the height will be determined by the <see cref="ITextView"/>.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
    public double MarginSize
    {
      get
      {
        this.ThrowIfDisposed ();
        return this.ActualWidth;
      }
    }

    /// <summary>
    /// Gets a value indicating whether the margin is enabled.
    /// </summary>
    /// <exception cref="ObjectDisposedException">The margin is disposed.</exception>
    public bool Enabled
    {
      get
      {
        this.ThrowIfDisposed ();

        // The margin should always be enabled
        return true;
      }
    }

    /// <summary>
    /// Gets the <see cref="ITextViewMargin"/> with the given <paramref name="marginName"/> or null if no match is found
    /// </summary>
    /// <param name="marginName">The name of the <see cref="ITextViewMargin"/></param>
    /// <returns>The <see cref="ITextViewMargin"/> named <paramref name="marginName"/>, or null if no match is found.</returns>
    /// <remarks>
    /// A margin returns itself if it is passed its own name. If the name does not match and it is a container margin, it
    /// forwards the call to its children. Margin name comparisons are case-insensitive.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="marginName"/> is null.</exception>
    public ITextViewMargin GetTextViewMargin (string marginName)
    {
      return string.Equals ( marginName, AnnotationMargin.MarginName, StringComparison.OrdinalIgnoreCase ) ? this : null;
    }

    /// <summary>
    /// Disposes an instance of <see cref="AnnotationMargin"/> class.
    /// </summary>
    public void Dispose ( )
    {
      if (!this.isDisposed)
      {
        GC.SuppressFinalize ( this );
        this.isDisposed = true;
      }
    }

    #endregion

    private void OnLayoutChanged(object sender, TextViewLayoutChangedEventArgs e)
    {
        _vm.RefreshPositions ( e, _wpfTextView, 0 ) ;
    }

    /// <summary>
    /// Checks and throws <see cref="ObjectDisposedException"/> if the object is disposed.
    /// </summary>
    private void ThrowIfDisposed ( )
    {
      if (this.isDisposed)
      {
        throw new ObjectDisposedException ( MarginName );
      }
    }
  }
}
