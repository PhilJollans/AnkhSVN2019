using System;
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
        IWpfTextViewMargin    result = null ;

        // Github issue 9
        // https://github.com/PhilJollans/AnkhSVN2019/issues/9
        // Make sure that this method returns null if any error occurs.
        // This probably means that we will never find the error.
        // Note: I intend to add a log file to AnkhSvn, in which case we could at least log the error.
        try
        {
            // From https://github.com/madskristensen/MarkdownEditor/blob/master/src/Margin/BrowserMarginProvider.cs
            ITextDocument document;
            bool          isok = TextDocumentFactoryService.TryGetTextDocument(wpfTextViewHost.TextView.TextDataModel.DocumentBuffer, out document ) ;

            // Get the filename
            var fn = document.FilePath ;

            // Is there an annotation view model?
            var vm = AnnotateService.GetModel ( fn ) ;
            if ( vm != null )
            {
                wpfTextViewHost.TextView.Options.SetOptionValue ( DefaultTextViewOptions.ViewProhibitUserInputId, true ) ;
                result = new AnnotationMargin ( wpfTextViewHost.TextView, vm ) ;
            }
        }
        catch ( Exception )
        {
            // Set result to null as a matter of form.
            // To do, log the error in a log file.
            result = null ;
        }
        return result ;
    }
  }
}
