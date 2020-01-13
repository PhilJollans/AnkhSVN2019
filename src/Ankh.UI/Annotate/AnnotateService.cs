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
        private static Dictionary<string,AnnotateMarginViewModel>      _ViewModelMap = null ;

        static AnnotateService ()
        {
            _ViewModelMap = new Dictionary<string, AnnotateMarginViewModel>() ;
        }

        public void DoBlame ( CommandEventArgs e,
                              SvnOrigin        origin,
                              SvnRevision      revisionStart,
                              SvnRevision      revisionEnd,
                              bool             ignoreEols,
                              SvnIgnoreSpacing ignoreSpacing,
                              bool             retrieveMergeInfo )
        {
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

            // Create a view model and add it to our internal map
            var annView = new AnnotateMarginViewModel ( e.Context ) ;
            _ViewModelMap.Add ( tempFile, annView ) ;
            annView.Initialize ( null, origin, blameResult, tempFile ) ;

            // Open the editor.
            // ToDo: Open files like resx as code.
            var dte = e.GetService<DTE> ( typeof(SDTE) ) ;
            dte.ItemOperations.OpenFile ( tempFile, EnvDTE.Constants.vsViewKindTextView ) ;
        }

        public AnnotateMarginViewModel GetModel ( string tempFile )
        {
            if ( _ViewModelMap.ContainsKey ( tempFile ) )
                return _ViewModelMap [ tempFile ] ;
            else
                return null ;
        }

    }
}
