using System.ComponentModel.Composition;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ankh.Commands;
using Ankh.Scc;
using Ankh.VS;
using SharpSvn;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Ankh.UI.Annotate
{
    [CLSCompliant(false)]
    public interface IAnnotateService
    {
        // Based on the original DoBlame method in class ItemAnnotateCommand
        void DoBlame ( CommandEventArgs e,
                       SvnOrigin        origin,
                       SvnRevision      revisionStart,
                       SvnRevision      revisionEnd,
                       bool             ignoreEols,
                       SvnIgnoreSpacing ignoreSpacing,
                       bool             retrieveMergeInfo ) ;

        AnnotateMarginViewModel     GetModel ( string tempFile ) ;
    }

    [Export ( typeof ( IAnnotateService ) )]
    [CLSCompliant(false)]
    public class AnnotateService : IAnnotateService
    {
        //
        // We will use a dictionary with
        // key:     full path to temporary file for the annotated view
        // value:   view model class for the margin (MVVM implementation)
        //
        private static Dictionary<string,AnnotateMarginParameters>      _ViewModelMap = null ;

        static AnnotateService ()
        {
            _ViewModelMap = new Dictionary<string, AnnotateMarginParameters>() ;
        }

        public void DoBlame ( CommandEventArgs e,
                              SvnOrigin        origin,
                              SvnRevision      revisionStart,
                              SvnRevision      revisionEnd,
                              bool             ignoreEols,
                              SvnIgnoreSpacing ignoreSpacing,
                              bool             retrieveMergeInfo )
        {
            ThreadHelper.ThrowIfNotOnUIThread() ;

            // There are two SVN related operations:
            // [1] Getting the file at revisionEnd, which will be displayed in the editor
            // [2] Getting the blame information, which will be displayed in the margin

            // This is the parameter structure for [1] getting the file
            SvnWriteArgs wa = new SvnWriteArgs();
            wa.Revision = revisionEnd;

            // This is the parameter structure for [2] getting the blame information
            SvnBlameArgs ba = new SvnBlameArgs();
            ba.Start                   = revisionStart;
            ba.End                     = revisionEnd;
            ba.IgnoreLineEndings       = ignoreEols;
            ba.IgnoreSpacing           = ignoreSpacing;
            ba.RetrieveMergedRevisions = retrieveMergeInfo;

            SvnTarget target = origin.Target;

            // Can we make this an MEF service?
            IAnkhTempFileManager tempMgr = e.GetService<IAnkhTempFileManager>();
            string tempFile = tempMgr.GetTempFileNamed(target.FileName);

            Collection<SvnBlameEventArgs> blameResult = null;

            bool retry = false;
            ProgressRunnerResult r = e.GetService<IProgressRunner>().RunModal("Annotating", delegate(object sender, ProgressWorkerArgs ee)
            {
                // Here we [1] get the file at revisionEnd
                using (FileStream fs = File.Create(tempFile))
                {
                    ee.Client.Write(target, fs, wa);
                }

                // Here we [2] get the blame information
                ba.SvnError +=
                    delegate(object errorSender, SvnErrorEventArgs errorEventArgs)
                    {
                        if (errorEventArgs.Exception is SvnClientBinaryFileException)
                        {
                            retry = true;
                            errorEventArgs.Cancel = true;
                        }
                    };
                ee.Client.GetBlame(target, ba, out blameResult);
            });

            if (retry)
            {
                using (AnkhMessageBox mb = new AnkhMessageBox(e.Context))
                {
                    // Move to resources later :)
                    if (DialogResult.Yes != mb.Show ( "You are trying to annotate a binary file. Are you sure you want to continue?",
                                                      "Binary file detected",
                                                      MessageBoxButtons.YesNo, MessageBoxIcon.Information))
                        return;

                    r = e.GetService<IProgressRunner>()
                            .RunModal("Annotating",
                                      delegate(object sender, ProgressWorkerArgs ee)
                                      {
                                          ba.IgnoreMimeType = true;
                                          ee.Client.GetBlame(target, ba, out blameResult);
                                      });
                }
            }

            if (!r.Succeeded)
                return;

            // Create a parameter struture and add it to our internal map.
            // Creating the actual view model class is now deferred to the GetModel method.
            var annParam = new AnnotateMarginParameters { Context = e.Context, Origin = origin, BlameResult = blameResult } ;
            _ViewModelMap.Add ( tempFile, annParam ) ;

            // Open the editor.
            // ToDo: Open files like resx as code.
            var dte = e.GetService<DTE> ( typeof(SDTE) ) ;
            dte.ItemOperations.OpenFile ( tempFile, EnvDTE.Constants.vsViewKindTextView ) ;

            // Suggestion from https://stackoverflow.com/questions/59741278/how-can-i-set-the-caption-on-an-editor-window-in-visual-studio
            // Unfortunately it doesn't work.
            //dte.ActiveWindow.Caption = $"{dte.ActiveWindow.Caption} - Annotated" ;
        }

        public AnnotateMarginViewModel GetModel ( string tempFile )
        {
            if ( _ViewModelMap.ContainsKey ( tempFile ) )
            {
                // If the editor pane is split into two independent parts, a second margin will be
                // generated. To handle this we need a separate ViewModel for each part of the split
                // window.
                var annParam = _ViewModelMap [ tempFile ] ;
                var annView  = new AnnotateMarginViewModel ( annParam.Context ) ;
                annView.Initialize ( annParam.Origin, annParam.BlameResult, tempFile ) ;
                return annView ;
            }
            else
                return null ;
        }

    }

    internal class AnnotateMarginParameters
    {
        public IAnkhServiceProvider             Context         { get; set; }
        public SvnOrigin                        Origin          { get; set; }
        public Collection<SvnBlameEventArgs>    BlameResult     { get; set; }
    }
}
