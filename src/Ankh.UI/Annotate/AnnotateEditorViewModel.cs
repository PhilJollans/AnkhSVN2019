using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Ankh.Scc;
using Ankh.VS.WpfServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Formatting;
using SharpSvn;

namespace Ankh.UI.Annotate
{
    /// <summary>
    /// ViewModel for the WPF AnnotateEditorView
    /// </summary>
    class AnnotateEditorViewModel : BindableBase
    {
        private List<AnnotateRegion>             _regions = new List<AnnotateRegion>();
        private SortedList<long, AnnotateSource> _sources = new SortedList<long, AnnotateSource>() ;

        public List<AnnotateRegion> Regions { get => _regions ; }

        public ICommand             SaveRegionCommand { get; private set; }

        public AnnotateEditorViewModel ( )
        {
            // Create command objects
            SaveRegionCommand = new RelayCommand<AnnotateRegion> ( Execute_SaveRegionCommand );
        }

        public void Initialize ( ServiceProvider serviceProvider, SvnOrigin origin, Collection<SvnBlameEventArgs> blameResult, string tempFile )
        {
            // Process the blame results
            AddLines ( origin, blameResult ) ;
        }

        // Originally in AnnotateEditorControl
        public void AddLines ( SvnOrigin origin, Collection<SvnBlameEventArgs> blameResult )
        {
            //_origin = origin;

            AnnotateRegion region = null;

            _regions.Clear();
            _sources.Clear();

            foreach (SvnBlameEventArgs e in blameResult)
            {
                AnnotateSource src;
                if (!_sources.TryGetValue(e.Revision, out src))
                    _sources.Add(e.Revision, src = new AnnotateSource(e, origin));

                int line = (int)e.LineNumber;

                if (region == null || region.Source != src)
                {
                    region = new AnnotateRegion(line, src);
                    _regions.Add(region);
                }
                else
                {
                    region.EndLine = line;
                }
            }
        }

        public void RefreshPositions ( TextViewLayoutChangedEventArgs e, IWpfTextView TextView, double Offset )
        {
            var snapshot = e.NewSnapshot ;

            foreach ( var region in _regions )
            {
                //
                // This logic is based on the method
                //   EditorDiffMargin.UpdateNormalDiffDimensions
                // in the project
                //   https://github.com/laurentkempe/GitDiffMargin
                //

                var startLine = snapshot.GetLineFromLineNumber(region.StartLine);
                var endLine   = snapshot.GetLineFromLineNumber(region.EndLine);

                // Don't think this can ever happen
                if ( startLine == null || endLine == null )
                {
                    region.IsVisible = false ;
                    continue;
                }

                var span = new SnapshotSpan ( startLine.Start, endLine.End ) ;
                if ( !TextView.TextViewLines.FormattedSpan.IntersectsWith(span) )
                {
                    region.IsVisible = false ;
                    continue;
                }

                var startLineView = TextView.GetTextViewLineContainingBufferPosition(startLine.Start);
                var endLineView   = TextView.GetTextViewLineContainingBufferPosition(endLine.Start);

                if (startLineView == null || endLineView == null)
                {
                    region.IsVisible = false ;
                    continue;
                }

                if (TextView.TextViewLines.LastVisibleLine.EndIncludingLineBreak < startLineView.Start)
                {
                    // starts after the last visible line
                    region.IsVisible = false ;
                    continue;
                }

                if (TextView.TextViewLines.FirstVisibleLine.Start > endLineView.EndIncludingLineBreak)
                {
                    // ends before the first visible line
                    region.IsVisible = false ;
                    continue;
                }

                double startTop;
                switch (startLineView.VisibilityState)
                {
                    case VisibilityState.FullyVisible:
                    case VisibilityState.Hidden:
                    case VisibilityState.PartiallyVisible:
                        startTop = startLineView.Top - TextView.ViewportTop ;
                        break;

                    case VisibilityState.Unattached:
                        // if the closest line was past the end we would have already returned
                        startTop = 0 ;
                        break;

                    default:
                        // shouldn't be reachable, but definitely hide if this is the case
                        region.IsVisible = false ;
                        continue;
                }

                double stopBottom;
                switch (endLineView.VisibilityState)
                {
                    case VisibilityState.FullyVisible:
                    case VisibilityState.Hidden:
                    case VisibilityState.PartiallyVisible:
                        stopBottom = endLineView.Bottom - TextView.ViewportTop ;
                        break;

                    case VisibilityState.Unattached:
                        // if the closest line was before the start we would have already returned
                        stopBottom = TextView.ViewportHeight ;
                        break;

                    default:
                        // shouldn't be reachable, but definitely hide if this is the case
                        region.IsVisible = false ;
                        continue;
                }

                region.Top    = startTop + Offset ;
                region.Height = stopBottom - startTop;
                region.IsVisible = true ;
            }
        }

        public void Execute_SaveRegionCommand ( AnnotateRegion r )
        {
            foreach ( var source in _sources.Values )
            {
                source.IsSelected = ( r.Source == source ) ;
            }
        }


    }
}
