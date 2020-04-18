using System;
using System.Collections.Generic;
using Ankh.Selection;

namespace Ankh.Scc
{
    [GlobalService(typeof(IProjectFileMapper))]
    partial class ProjectTracker : IProjectFileMapper
    {
        public IEnumerable<Selection.SccProject> GetAllProjectsContaining(string path)
        {
            return ProjectMap.GetAllProjectsContaining(path);
        }

        public IEnumerable<Selection.SccProject> GetAllProjectsContaining(IEnumerable<string> paths)
        {
            return ProjectMap.GetAllProjectsContaining(paths);
        }

        public IEnumerable<Selection.SccProject> GetAllSccProjects()
        {
            return ProjectMap.GetAllSccProjects();
        }

        public IEnumerable<string> GetAllFilesOf(SccProject project)
        {
            return ProjectMap.GetAllFilesOf(project, false);
        }

        public IEnumerable<string> GetAllFilesOf(Selection.SccProject project, bool exceptExcluded)
        {
            return ProjectMap.GetAllFilesOf(project, exceptExcluded);
        }

        public IEnumerable<string> GetAllFilesOf(ICollection<Selection.SccProject> projects)
        {
            return ProjectMap.GetAllFilesOf(projects, false);
        }

        public IEnumerable<string> GetAllFilesOf(ICollection<Selection.SccProject> projects, bool exceptExcluded)
        {
            return ProjectMap.GetAllFilesOf(projects, exceptExcluded);
        }

        public ICollection<string> GetAllFilesOfAllProjects()
        {
            return ProjectMap.GetAllFilesOfAllProjects(false);
        }

        public ICollection<string> GetAllFilesOfAllProjects(bool exceptExcluded)
        {
            return ProjectMap.GetAllFilesOfAllProjects(exceptExcluded);
        }

        bool IProjectFileMapper.ContainsPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException("path");

            // GitHub Issue #7
            // It appears that a NullReferenceException has occurred in this function.
            // So far as I can tell, it can only occur if ProjectMap is null, which doesn't seem possible.
            // Nevertheless catch and ignore any exception.
            try
            {
                if (ProjectMap.ContainsFile(path))
                    return true;

                if (string.Equals(path, ProjectMap.SolutionFilename, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            catch ( Exception ){}

            return false;
        }

        bool IProjectFileMapper.IsSccExcluded(string path)
        {
            return ProjectMap.IsSccExcluded(path);
        }

        string IProjectFileMapper.SolutionFilename
        {
            get { return ProjectMap.SolutionFilename; }
        }

        public bool IsProjectFileOrSolution(string path)
        {
            return ProjectMap.IsProjectFileOrSolution(path);
        }

        public ISccProjectInfo GetProjectInfo(Selection.SccProject project)
        {
            return ProjectMap.GetProjectInfo(project);
        }

        public ProjectIconReference GetPathIconHandle(string path)
        {
            return ProjectMap.GetPathIconHandle(path);
        }

        public bool IgnoreEnumerationSideEffects(Microsoft.VisualStudio.Shell.Interop.IVsSccProject2 sccProject)
        {
            return ProjectMap.IgnoreEnumerationSideEffects(sccProject);
        }
    }
}
