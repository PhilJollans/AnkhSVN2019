using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
        readonly int _startLine;
        int _endLine;

        private bool   _hovered   = false ;
        private bool   _isVisible = false ;
        private double _height    = 30.0 ;
        private double _top       = 10.0 ;

        internal bool Hovered   { get => _hovered ; set => SetProperty ( ref _hovered, value ) ; }

        // Added for WPF version
        public bool           IsVisible { get => _isVisible ; set => SetProperty ( ref _isVisible, value ) ; }
        public double         Height    { get => _height ; set => SetProperty ( ref _height, value ) ; }
        public double         Top       { get => _top ;    set => SetProperty ( ref _top, value ) ; }
        public AnnotateSource Source    { get => _source; }
        public string         ShortDate { get => _source.Time.ToShortDateString() ; }
        public int            StartLine { get => _startLine; }
        public int            EndLine   { get => _endLine; internal set => SetProperty ( ref _endLine, value ) ; }

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
        }


    }

}
