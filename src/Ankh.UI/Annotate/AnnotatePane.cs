using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell;
using SharpSvn;

namespace Ankh.UI.Annotate
{
    [CLSCompliant(false)]
    public sealed class AnnotateDocumentHost : ISite, IAnkhServiceProvider
    {
        readonly AnnotatePane _pane;
        readonly ServiceProviderHierarchy _spHier = new ServiceProviderHierarchy();

        public AnnotateDocumentHost ( AnnotatePane pane )
        {
           _pane = pane ;
        }

        #region ISite Members

        public IComponent Component
        {
            get { return _pane.Window as IComponent; }
        }

        Container _container;
        public IContainer Container
        {
            get { return _container ?? (_container = new Container()); }
        }

        public bool DesignMode
        {
            get { return false; }
        }

        public string Name
        {
            get { return ToString(); }
            set { }
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(AmbientProperties))
            {
                return GetService<IAnkhPackage>().AmbientProperties;
            }
            object ob = ServiceProviderHierarchy.GetService(serviceType);

            if (ob != null)
                return ob;
            else if (Package != null)
                return Package.GetService(serviceType);
            else
                return null;
        }

        #endregion

        #region IAnkhServiceProvider Members

        [DebuggerStepThrough]
        public T GetService<T>()
            where T : class
        {
            return (T)GetService(typeof(T));
        }

        [DebuggerStepThrough]
        public T GetService<T>(Type serviceType)
            where T : class
        {
            return (T)GetService(serviceType);
        }

        #endregion

        IAnkhPackage _package;
        public IAnkhPackage Package
        {
            get
            {
                if (_package != null)
                    return _package;
#if false
                if (_pane != null && _pane.Package != null)
                    _package = (IAnkhPackage)_pane.Package;
#endif
                return _package;
            }
        }

        public ServiceProviderHierarchy ServiceProviderHierarchy
        {
            get { return _spHier; }
        }

    }

    //
    // Keep it simple
    //
    [ComVisible(true)]
    [CLSCompliant(false)]
    public sealed class AnnotatePane : WindowPane
    {
        private AnnotateDocumentHost        _host ;

        public AnnotatePane ( Collection<SvnBlameEventArgs> blameResult, string tempFile )
        {
            _host = new AnnotateDocumentHost(this);
        }


        protected override void Initialize ( )
        {
            base.Initialize ();
        }
    }
}
