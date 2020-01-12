using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.IO;
using System.Text.RegularExpressions;

namespace Ankh.UI.Annotate
{
  /// <summary>
  /// Export a <see cref="IWpfTextViewMarginProvider"/>, which returns an instance of the margin for the editor to use.
  /// </summary>
  [Export ( typeof ( IWpfTextViewMarginProvider ) )]
  [Name ( AnnotationMargin.MarginName )]
  [Order ( After = PredefinedMarginNames.Glyph )]
  [MarginContainer ( PredefinedMarginNames.Left )]
  [ContentType ( "code" )]
  [TextViewRole ( PredefinedTextViewRoles.Interactive )]
  internal sealed class AnnotationMarginFactory : IWpfTextViewMarginProvider
  {
    [Import]
    public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

    // AnnotateService is defined as an MEF service.
    [Import]
    public IAnnotateService            AnnotateService { get; set; }

    /// <summary>
    /// Creates an <see cref="IWpfTextViewMargin"/> for the given <see cref="IWpfTextViewHost"/>.
    /// </summary>
    /// <param name="wpfTextViewHost">The <see cref="IWpfTextViewHost"/> for which to create the <see cref="IWpfTextViewMargin"/>.</param>
    /// <param name="marginContainer">The margin that will contain the newly-created margin.</param>
    /// <returns>The <see cref="IWpfTextViewMargin"/>.
    /// The value may be null if this <see cref="IWpfTextViewMarginProvider"/> does not participate for this context.
    /// </returns>
    public IWpfTextViewMargin CreateMargin (IWpfTextViewHost wpfTextViewHost, IWpfTextViewMargin marginContainer)
    {
      // From https://github.com/madskristensen/MarkdownEditor/blob/master/src/Margin/BrowserMarginProvider.cs
      ITextDocument document;
      bool          isok = TextDocumentFactoryService.TryGetTextDocument(wpfTextViewHost.TextView.TextDataModel.DocumentBuffer, out document ) ;

      // Get the filename
      var fn = document.FilePath ;

      // Is there an annotation view model?
      var vm = AnnotateService.GetModel ( fn ) ;
      if ( vm == null )
      {
          return null ;
      }
      else
      {
          return new AnnotationMargin ( wpfTextViewHost.TextView, vm ) ;
      }

    }
  }
}
