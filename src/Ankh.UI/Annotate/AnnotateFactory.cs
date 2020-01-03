using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SharpSvn;
using Ankh.VS.Dialogs;
using Ankh.Scc;

namespace Ankh.UI.Annotate
{
    public interface IAnnotateFactory
    {
        void Create ( SvnOrigin origin, Collection<SvnBlameEventArgs> blameResult, string tempFile ) ;
    }

    [Guid(AnkhId.AnnotateEditorId), ComVisible(true), CLSCompliant(false)]
    [ComDefaultInterface(typeof(IAnnotateFactory))]
    public class AnnotateFactory : AnkhService, IVsEditorFactory, IAnnotateFactory
    {
        private ServiceProvider vsServiceProvider;

        private readonly Stack<Tuple<SvnOrigin,Collection<SvnBlameEventArgs>,string>> _parameters = new Stack<Tuple<SvnOrigin,Collection<SvnBlameEventArgs>,string>>();

        public AnnotateFactory ( IAnkhServiceProvider context )
            : base(context)
        {
        }

        #region IVsEditorFactory Members

        public int CreateEditorInstance (uint grfCreateDoc,
                                         string pszMkDocument,
                                         string pszPhysicalView,
                                         IVsHierarchy pvHier,
                                         uint itemid,
                                         IntPtr punkDocDataExisting,
                                         out IntPtr ppunkDocView,
                                         out IntPtr ppunkDocData,
                                         out string pbstrEditorCaption,
                                         out Guid pguidCmdUI,
                                         out int pgrfCDW)
        {
            ppunkDocData = IntPtr.Zero;
            ppunkDocView = IntPtr.Zero;
            pbstrEditorCaption = "";
            pguidCmdUI = new Guid("{00000000-0000-0000-e4e7-120000008400}");
            pgrfCDW = 0;

            // Validate inputs
            if ((grfCreateDoc & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
            {
                return VSConstants.E_INVALIDARG;
            }

            // Currently screws up on reopening a project if the annotate window was previously opened.
            // TODO fix it
            if ( _parameters.Count == 0 )
                return VSConstants.E_FAIL ;

            var param = _parameters.Pop() ;

            var annView   = new AnnotateEditorView ( Context ) ;

            var doc  = new VSDocumentInstance ( Context, new Guid(AnkhId.AnnotateEditorId) ) ;
            var pane = new AnnotatePane ( param.Item2, param.Item3 ) ;
            pane.Content = annView ;
            annView.Initialize ( vsServiceProvider, param.Item1, param.Item2, param.Item3 ) ;

            ppunkDocView = Marshal.GetIUnknownForObject(pane);
            ppunkDocData = Marshal.GetIUnknownForObject(doc);

            pbstrEditorCaption = "AnkhSVN Annotate" ;

            return VSConstants.S_OK;
        }

        public int SetSite (Microsoft.VisualStudio.OLE.Interop.IServiceProvider psp)
        {
            vsServiceProvider = new ServiceProvider(psp);
            return VSConstants.S_OK;
        }

        public int Close ( )
        {
            return VSConstants.S_OK;
        }

        public int MapLogicalView (ref Guid rguidLogicalView, out string pbstrPhysicalView)
        {
            pbstrPhysicalView = null;

            if (rguidLogicalView == VSConstants.LOGVIEWID_Primary)
            {
                pbstrPhysicalView = null;
                return VSErr.S_OK;
            }

            return VSErr.E_NOTIMPL;
        }

        #endregion

        public void Create ( SvnOrigin origin, Collection<SvnBlameEventArgs> blameResult, string tempFile)
        {
            _parameters.Push ( new Tuple<SvnOrigin,Collection<SvnBlameEventArgs>,string> ( origin, blameResult, tempFile ) ) ;

            IVsUIHierarchy hier;
            uint           id;
            IVsWindowFrame frame;

            VsShellUtilities.OpenDocumentWithSpecificEditor ( Context,
                                                              tempFile,
                                                              new Guid(AnkhId.AnnotateEditorId),
                                                              VSConstants.LOGVIEWID_Primary,
                                                              out hier,
                                                              out id,
                                                              out frame ) ;
        }

    }
}
