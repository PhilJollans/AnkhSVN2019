using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using SharpSvn;

namespace Ankh.Scc
{
    partial class SccProvider
    {
        protected void ClearSolutionInfo()
        {
            ProjectMap.ClearSolutionInfo();
        }

        public string SolutionFilename
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return ProjectMap.SolutionFilename;
            }
        }

        public string SolutionDirectory
        {
            get 
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return ProjectMap.SolutionDirectory; 
            }
        }

        public string RawSolutionDirectory
        {
            get 
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return ProjectMap.RawSolutionDirectory; 
            }
        }
    }
}
