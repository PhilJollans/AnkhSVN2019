using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Ankh.Commands;
using Ankh.Scc;
using Ankh.Scc.UI;
using Ankh.Selection;
using Ankh.UI.SvnLog;
using Ankh.VS.WpfServices;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SharpSvn;

namespace Ankh.UI.Annotate
{
    /// <summary>
    /// AnnotateRegion represents an area in the margin of the annotated editor window.
    /// It corresponds to a contiguous block of lines chaged by the same SVN revision.
    ///
    /// Multiple AnnotateRegion object may refer to the same SVN revision.
    /// The SVN revision is represented by the AnnotateSource object.
    /// </summary>
    class AnnotateRegion : BindableBase
    {
        readonly AnnotateSource _source;
        readonly int            _startLine;
        int _endLine;

        private bool   _hovered    = false ;
        private bool   _isVisible  = false ;
        private double _height     = 30.0 ;
        private double _top        = 10.0 ;

        internal bool Hovered   { get => _hovered ; set => SetProperty ( ref _hovered, value ) ; }

        public bool           IsVisible  { get => _isVisible ; set => SetProperty ( ref _isVisible, value ) ; }
        public double         Height     { get => _height ; set => SetProperty ( ref _height, value ) ; }
        public double         Top        { get => _top ;    set => SetProperty ( ref _top, value ) ; }
        public AnnotateSource Source     { get => _source; }
        public string         ShortDate  { get => _source.Time.ToShortDateString() ; }
        public string         LocalTime  { get => _source.Time.ToLocalTime().ToString() ; }
        public int            StartLine  { get => _startLine; }
        public int            EndLine    { get => _endLine; internal set => SetProperty ( ref _endLine, value ) ; }

        public ICommand             CompareWorkingCopyCommand { get; private set; }
        public ICommand             ShowChangesCommand        { get; private set; }
        public ICommand             ViewHistoryCommand        { get; private set; }
        public ICommand             CopyToWorkingCopyCommand  { get; private set; }
        public ICommand             CopyRevisionCommand       { get; private set; }
        public ICommand             PropertiesCommand         { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotateRegion"/> class.
        /// </summary>
        /// <param name="startLine">The start line.</param>
        /// <param name="endLine">The end line.</param>
        /// <param name="source">The source.</param>
        public AnnotateRegion(int line, AnnotateSource source)
        {
            if(source == null)
                throw new ArgumentNullException("source");

            _source = source;
            _startLine = _endLine = line;

            // Create command objects
            CompareWorkingCopyCommand = new RelayCommand ( Execute_CompareWorkingCopyCommand );
            ShowChangesCommand        = new RelayCommand ( Execute_ShowChangesCommand        );
            ViewHistoryCommand        = new RelayCommand ( Execute_ViewHistoryCommand        );
            CopyToWorkingCopyCommand  = new RelayCommand ( Execute_CopyToWorkingCopyCommand  );
            CopyRevisionCommand       = new RelayCommand ( Execute_CopyRevisionCommand       );
            PropertiesCommand         = new RelayCommand ( Execute_PropertiesCommand         );
        }

        public void Execute_CompareWorkingCopyCommand()
        {
            try
            {
                SvnRevision  from = new SvnRevision ( _source.Revision ) ;
                SvnRevision  to   = SvnRevision.Working ;
                AnkhDiffArgs da   = new AnkhDiffArgs();

                // I have stored the context in the AnnotateSource object instead of this object.
                IAnkhDiffHandler diff = _source.Context.GetService<IAnkhDiffHandler>();

                da.BaseFile = diff.GetTempFile ( _source.Origin.Target, from, true ) ;

                // User canceled ??
                if ( da.BaseFile != null )
                {
                    da.MineFile = ((SvnPathTarget)_source.Origin.Target).FullPath ;
                    da.BaseTitle = diff.GetTitle(_source.Origin.Target, from);
                    da.MineTitle = diff.GetTitle(_source.Origin.Target, to);
                    diff.RunDiff(da);
                }
            }
            catch (Exception ex)
            {
                // There is a bit too much code which might fail in the exception handler for my taste
                IAnkhErrorHandler eh = _source.Context.GetService<IAnkhErrorHandler>();

                if (eh != null && eh.IsEnabled(ex))
                    eh.OnError(ex);
                else
                    throw;
            }
        }

        public void Execute_ShowChangesCommand()
        {
            try
            {
                SvnRevision  from = new SvnRevision ( _source.Revision - 1 ) ;
                SvnRevision  to   = new SvnRevision ( _source.Revision ) ;
                AnkhDiffArgs da   = new AnkhDiffArgs();

                // I have stored the context in the AnnotateSource object instead of this object.
                IAnkhDiffHandler diff = _source.Context.GetService<IAnkhDiffHandler>();

                string[] files = diff.GetTempFiles ( _source.Origin.Target, from, to, true ) ;

                // User canceled ??
                if ( files != null )
                {
                    da.BaseFile = files[0];
                    da.MineFile = files[1];
                    File.SetAttributes ( da.MineFile, FileAttributes.ReadOnly | FileAttributes.Normal ) ;

                    da.BaseTitle = diff.GetTitle(_source.Origin.Target, from);
                    da.MineTitle = diff.GetTitle(_source.Origin.Target, to);
                    diff.RunDiff(da);
                }
            }
            catch (Exception ex)
            {
                // There is a bit too much code which might fail in the exception handler for my taste
                IAnkhErrorHandler eh = _source.Context.GetService<IAnkhErrorHandler>();

                if (eh != null && eh.IsEnabled(ex))
                    eh.OnError(ex);
                else
                    throw;
            }
        }

        public void Execute_ViewHistoryCommand()
        {
            try
            {
                //
                // This duplicates the original functionality in \Ankh\Commands\LogCommand.cs, which is
                // to show the log window with the selected item and older items only. It does not
                // automatically select the first item, so the lower two panes are not initialised.
                //
                // In my opinion it would make more sense to show the whole history and to select the
                // specified revision in the list.
                //
                var package = _source.Context.GetService<IAnkhPackage>() ;
                package.ShowToolWindow ( AnkhToolWindow.Log ) ;

                var logToolControl = _source.Context.GetService<ISelectionContext>().ActiveFrameControl as LogToolWindowControl;
                if ( logToolControl != null )
                {
                    logToolControl.StartLog ( new SvnOrigin[] { _source.Origin },
                                              new SvnRevision ( _source.Revision ),
                                              null ) ;
                }
            }
            catch (Exception ex)
            {
                // There is a bit too much code which might fail in the exception handler for my taste
                IAnkhErrorHandler eh = _source.Context.GetService<IAnkhErrorHandler>();

                if (eh != null && eh.IsEnabled(ex))
                    eh.OnError(ex);
                else
                    throw;
            }
        }

        public void Execute_CopyToWorkingCopyCommand()
        {
            try
            {
                //
                // In this case, there is too much code in
                //   \Ankh\Commands\RepositoryExplorer\CopyToWorkingCopy.cs
                // to simply duplicate it and there isn't a simple back door.
                //
                // The code relies on the selection manager to get the selected revision.
                // I am having difficulty understanding the selection manager, so I have
                // decided to hack it, by adding a new method FakeSingleSelection().
                //
                var sc = _source.Context.GetService<ISelectionContext>() ;
                sc.FakeSingleSelection ( _source as ISvnRepositoryItem ) ;

                // It took some time to work this out, but it seems we can open a back door into the command handling.
                var cm = _source.Context.GetService<CommandMapper>() ;
                var cx = _source.Context.GetService<AnkhContext>() ;
                CommandEventArgs args = new CommandEventArgs ( AnkhCommand.CopyToWorkingCopy, cx ) ;
                cm.Execute ( AnkhCommand.CopyToWorkingCopy, args ) ;
            }
            catch (Exception ex)
            {
                // There is a bit too much code which might fail in the exception handler for my taste
                IAnkhErrorHandler eh = _source.Context.GetService<IAnkhErrorHandler>();

                if (eh != null && eh.IsEnabled(ex))
                    eh.OnError(ex);
                else
                    throw;
            }
        }

        public void Execute_CopyRevisionCommand()
        {
            try
            {
                Clipboard.SetText ( _source.Revision.ToString(), TextDataFormat.Text ) ;
            }
            catch (Exception ex)
            {
                // There is a bit too much code which might fail in the exception handler for my taste
                IAnkhErrorHandler eh = _source.Context.GetService<IAnkhErrorHandler>();

                if (eh != null && eh.IsEnabled(ex))
                    eh.OnError(ex);
                else
                    throw;
            }
        }

        public void Execute_PropertiesCommand()
        {
            try
            {
                // Leave for now.
                // I don't yet know how to make this work.

                //const int IDG_VS_CTXT_ITEM_PROPERTIES = 0x020E ;

                //var dte = _source.Context.GetService<DTE> ( typeof(SDTE) ) ;
                //dte.Commands.Raise ( VsMenus.guidSHLMainMenu.ToString(), IDG_VS_CTXT_ITEM_PROPERTIES, null, null ) ;
            }
            catch (Exception ex)
            {
                // There is a bit too much code which might fail in the exception handler for my taste
                IAnkhErrorHandler eh = _source.Context.GetService<IAnkhErrorHandler>();

                if (eh != null && eh.IsEnabled(ex))
                    eh.OnError(ex);
                else
                    throw;
            }
        }

    }

}
