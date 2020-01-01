using Microsoft.VisualStudio;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.OLE.Interop;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using SharpSvn;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ankh.Scc;

namespace Ankh.UI.Annotate
{
    /// <summary>
    /// WPF version of the original Winforms AnnotateEditorControl, with a different apporach to
    /// instantiating the embedded text editor.
    /// </summary>
    [CLSCompliant ( false )]
    public partial class AnnotateEditorView : UserControl
    {
        private AnnotateEditorViewModel     _vm ;

        private IVsTextView                 textView;
        private IVsCodeWindow               codeWindow;
        private IVsInvisibleEditor          invisibleEditor;
        private IServiceProvider            cachedOleServiceProvider;

        private IWpfTextViewHost            textViewHost ;
        private IWpfTextView                wpfTextView ;

        public AnnotateEditorView ( )
        {
            InitializeComponent ();
            _vm = new AnnotateEditorViewModel() ;
            DataContext = _vm ;
        }

        public void Initialize ( ServiceProvider serviceProvider, SvnOrigin origin, Collection<SvnBlameEventArgs> blameResult, string tempFile)
        {
            // Initialise the view model
            // (the stucture is somewhat fluid at the moment)
            _vm.Initialize ( serviceProvider, origin, blameResult, tempFile ) ;

            // I have stolen this technique from the project https://github.com/yysun/git-tools

            //Get an invisible editor over the file, this makes it much easier than having to manually figure out the right content type,
            //language service, and it will automatically associate the document with its owning project, meaning we will get intellisense
            //in our editor with no extra work.
            var mgr = serviceProvider.GetService ( typeof ( SVsInvisibleEditorManager ) ) as IVsInvisibleEditorManager;

            if (mgr != null)
            {
                ErrorHandler.ThrowOnFailure ( mgr.RegisterInvisibleEditor ( tempFile,
                                                                            pProject: null,
                                                                            dwFlags: (uint)_EDITORREGFLAGS.RIEF_ENABLECACHING,
                                                                            pFactory: null,
                                                                            ppEditor: out invisibleEditor ) ) ;

                //The doc data is the IVsTextLines that represents the in-memory version of the file we opened in our invisibe editor, we need
                //to extract that so that we can create our real (visible) editor.
                IntPtr docDataPointer = IntPtr.Zero;
                Guid guidIVSTextLines = typeof ( IVsTextLines ).GUID;
                ErrorHandler.ThrowOnFailure ( invisibleEditor.GetDocData ( fEnsureWritable: 1, riid: ref guidIVSTextLines, ppDocData: out docDataPointer ) );
                try
                {
                    IVsTextLines docData = (IVsTextLines)Marshal.GetObjectForIUnknown ( docDataPointer );

                    //Get the component model so we can request the editor adapter factory which we can use to spin up an editor instance.
                    IComponentModel componentModel = (IComponentModel)serviceProvider.GetService ( typeof ( SComponentModel ) );
                    IVsEditorAdaptersFactoryService editorAdapterFactoryService = componentModel.GetService<IVsEditorAdaptersFactoryService> ();

                    //Create a code window adapter.
                    codeWindow = editorAdapterFactoryService.CreateVsCodeWindowAdapter ( OleServiceProvider );

                    //Disable the splitter control on the editor as leaving it enabled causes a crash if the user
                    //tries to use it here :(
                    IVsCodeWindowEx codeWindowEx = (IVsCodeWindowEx)codeWindow ;
                    INITVIEW[] initView = new INITVIEW[1];
                    codeWindowEx.Initialize ( (uint)_codewindowbehaviorflags.CWB_DISABLESPLITTER,
                                             VSUSERCONTEXTATTRIBUTEUSAGE.VSUC_Usage_Filter,
                                             szNameAuxUserContext: "",
                                             szValueAuxUserContext: "",
                                             InitViewFlags: 0,
                                             pInitView: initView );

                    //docData.SetStateFlags((uint)BUFFERSTATEFLAGS.BSF_USER_READONLY); //set read only

                    //Associate our IVsTextLines with our new code window.
                    ErrorHandler.ThrowOnFailure ( codeWindow.SetBuffer ( (IVsTextLines)docData ) );

                    //Get our text view for our editor which we will use to get the WPF control that hosts said editor.
                    ErrorHandler.ThrowOnFailure ( codeWindow.GetPrimaryView ( out textView ) );

                    //Get our WPF host from our text view (from our code window).
                    textViewHost = editorAdapterFactoryService.GetWpfTextViewHost ( textView );
                    wpfTextView  = textViewHost.TextView ;

                    //textViewHost.TextView.Options.SetOptionValue(GitTextViewOptions.DiffMarginId, false);
                    wpfTextView.Options.SetOptionValue ( DefaultTextViewHostOptions.ChangeTrackingId, false );
                    wpfTextView.Options.SetOptionValue ( DefaultTextViewHostOptions.GlyphMarginId, false );
                    wpfTextView.Options.SetOptionValue ( DefaultTextViewHostOptions.LineNumberMarginId, false );
                    wpfTextView.Options.SetOptionValue ( DefaultTextViewHostOptions.OutliningMarginId, false );

                    wpfTextView.Options.SetOptionValue ( DefaultTextViewOptions.ViewProhibitUserInputId, true );

                    // Hook up to the layout changed event.
                    wpfTextView.LayoutChanged += TextView_LayoutChanged;

                    var b = textViewHost.HostControl.Parent as Border ;
                    if ( b != null )
                    {
                        b.Child = null ;
                    }

                    // Set the content in the content control
                    AnnotateEditor.Content = textViewHost.HostControl ;
                }
                finally
                {
                    if (docDataPointer != IntPtr.Zero)
                    {
                        //Release the doc data from the invisible editor since it gave us a ref-counted copy.
                        Marshal.Release ( docDataPointer );
                    }
                }
            }
        }

        private void TextView_LayoutChanged (object sender, TextViewLayoutChangedEventArgs e)
        {
            double Offset = 0.0 ;
            try
            {
                var fet = wpfTextView.VisualElement ;
                var feh = textViewHost.HostControl ;
                var p   = fet.TransformToAncestor(feh).Transform(new Point(0,0)) ;
                Offset = p.Y ;
            }
            catch ( Exception ){}

            _vm.RefreshPositions ( e, wpfTextView, Offset ) ;
        }

        /// <summary>
        /// The shell's service provider as an OLE service provider (needed to create the editor bits).
        /// </summary>
        private IServiceProvider OleServiceProvider
        {
            get
            {
                if ( cachedOleServiceProvider == null )
                {
                    //ServiceProvider.GlobalProvider is a System.IServiceProvider, but the editor pieces want an OLE.IServiceProvider, luckily the
                    //global provider is also IObjectWithSite and we can use that to extract its underlying (OLE) IServiceProvider object.
                    IObjectWithSite objWithSite = (IObjectWithSite)ServiceProvider.GlobalProvider;

                    Guid interfaceIID = typeof(IServiceProvider).GUID;
                    IntPtr rawSP;
                    objWithSite.GetSite(ref interfaceIID, out rawSP);
                    try
                    {
                        if (rawSP != IntPtr.Zero)
                        {
                            //Get an RCW over the raw OLE service provider pointer.
                            this.cachedOleServiceProvider = (Microsoft.VisualStudio.OLE.Interop.IServiceProvider)Marshal.GetObjectForIUnknown(rawSP);
                        }
                    }
                    finally
                    {
                        if (rawSP != IntPtr.Zero)
                        {
                            //Release the raw pointer we got from IObjectWithSite so we don't cause leaks.
                            Marshal.Release(rawSP);
                        }
                    }
                }

                return this.cachedOleServiceProvider;
            }
        }

    }
}
