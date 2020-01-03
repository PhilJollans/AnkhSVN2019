using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using Ankh.Scc;
using Ankh.Scc.UI;
using Ankh.VS.WpfServices;
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
